using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.Account.Commands;
using Application.Contract.Verification.Enums;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Account.Commands;

public sealed class ConfirmLinkCommandHandler(
    ICurrentUserService current,
    IVerificationSessionStore store,
    IApplicationDbContext db,
    UserManager<ApplicationUser> um)
    : IRequestHandler<ConfirmLinkCommand, Unit>
{
    public async Task<Unit> Handle(ConfirmLinkCommand request, CancellationToken ct)
    {
        var s = await store.GetAsync(request.SessionId, ct)
                ?? throw new NotFoundException("Сессия не найдена.");
        if (!s.IsVerified || s.Purpose != "link")
            throw new BadRequestException("Код не подтверждён.");

        var userName = current.GetType().GetMethod("GetUserName")?.Invoke(current, null) as string
                       ?? (current.GetType().GetProperty("UserName")?.GetValue(current) as string)
                       ?? throw new UnauthorizedAccessException();

        // текущий пользователь
        var me = await db.Users.FirstAsync(u => u.UserName == userName, ct);

        // Если контакт принадлежит другому пользователю — «переносим» контакт и объединяем данные
        if (s.Selected == OtpChannel.email && !string.IsNullOrWhiteSpace(s.Email))
        {
            var email = s.Email.Trim();
            var other = await db.Users.FirstOrDefaultAsync(u =>
                u.Id != me.Id && u.Email != null && u.Email.ToLower() == email.ToLower(), ct);

            if (other is not null)
            {
                // Отвязываем у него e-mail (для уникальности)
                other.Email = null;
                other.NormalizedEmail = null;
                await db.SaveChangesAsync(ct);

                // Самая ранняя дата регистрации
                if (other.CreatedAt < me.CreatedAt)
                    me.CreatedAt = other.CreatedAt;

                // Переносим заказы/данные
                await MergeUserDataAsync(db, other.Id, me.Id, ct);
                await DeleteUserCompletelyAsync(db, um, other, ct);
            }

            // Теперь назначаем e-mail текущему
            me.Email = email;
            me.NormalizedEmail = email.ToUpperInvariant();
        }
        else if (s.Selected == OtpChannel.phone && !string.IsNullOrWhiteSpace(s.Phone))
        {
            // В сессии хранится E.164 (+7XXXXXXXXXX) — приводим к формату проекта +7-XXX-XXX-XX-XX
            var dashed = FormatDashedFromE164(s.Phone);
            var other = await db.Users.FirstOrDefaultAsync(u =>
                u.Id != me.Id && u.PhoneNumber != null && u.PhoneNumber == dashed, ct);

            if (other is not null)
            {
                // Отвязываем у него телефон
                other.PhoneNumber = null;
                await db.SaveChangesAsync(ct);

                if (other.CreatedAt < me.CreatedAt)
                    me.CreatedAt = other.CreatedAt;

                await MergeUserDataAsync(db, other.Id, me.Id, ct);
                await DeleteUserCompletelyAsync(db, um, other, ct);
            }

            me.PhoneNumber = dashed;
        }
        else
        {
            throw new BadRequestException("Контакт для привязки не найден.");
        }

        await db.SaveChangesAsync(ct);
        await store.RemoveAsync(s.SessionId, ct); // очистили сессию
        return Unit.Value;
    }

    private static string FormatDashedFromE164(string phoneE164)
    {
        var digits = Regex.Replace(phoneE164 ?? "", @"\D", ""); // 7XXXXXXXXXX
        if (digits.Length != 11 || digits[0] != '7')
            throw new BadRequestException("Некорректный номер телефона.");

        return Regex.Replace(digits, @"^7(\d{3})(\d{3})(\d{2})(\d{2})$", "+7-$1-$2-$3-$4");
    }

    /// <summary>
    /// Объединение «данных пользователя». По-умолчанию перевешиваем сущности, в названии которых есть "Order"
    /// и у которых есть Guid-свойство CustomerId или UserId. При необходимости дополни для своих таблиц.
    /// </summary>
    private static async Task MergeUserDataAsync(IApplicationDbContext dbCtx, Guid fromUserId, Guid toUserId,
        CancellationToken ct)
    {
        var ef = dbCtx as DbContext ?? throw new InvalidOperationException("DbContext not available");
        var model = ef.Model.GetEntityTypes();

        foreach (var et in model)
        {
            var clr = et.ClrType;

            // фильтруем только сущности-заказы
            if (!clr.Name.Contains("Order", StringComparison.OrdinalIgnoreCase))
                continue;

            var propCustomer = clr.GetProperty("CustomerId") ?? clr.GetProperty("UserId");
            if (propCustomer is null) continue;

            var underlying = Nullable.GetUnderlyingType(propCustomer.PropertyType) ?? propCustomer.PropertyType;
            if (underlying != typeof(Guid)) continue;

            // DbSet<T> Set<T>() — вызываем через рефлексию, т.к. T известен только рантаймно
            var setGeneric = typeof(DbContext)
                .GetMethod(nameof(DbContext.Set), Type.EmptyTypes)!
                .MakeGenericMethod(clr);

            var set = (IQueryable)setGeneric.Invoke(ef, null)!; // DbSet<T> как IQueryable

            // e => e.CustomerId == fromUserId
            var param = Expression.Parameter(clr, "e");
            var left = Expression.Property(param, propCustomer);

            // если поле Guid?, берём .Value
            Expression leftGuid = left.Type == typeof(Guid)
                ? left
                : Expression.Property(left, "Value");

            var right = Expression.Constant(fromUserId, typeof(Guid));
            var body = Expression.Equal(leftGuid, right);
            var lambda = Expression.Lambda(body, param);

            // Queryable.Where<T>(IQueryable<T>, Expression<Func<T,bool>>)
            var whereMethod = typeof(Queryable).GetMethods()
                .First(m => m.Name == "Where" && m.GetParameters().Length == 2)
                .MakeGenericMethod(clr);

            var filtered = (IQueryable)whereMethod.Invoke(null, new object[] { set, lambda })!;

            // переносим владение
            var valueToSet = propCustomer.PropertyType == typeof(Guid)
                ? (object)toUserId
                : (Guid?)toUserId;

            foreach (var entity in filtered.Cast<object>().ToList())
                propCustomer.SetValue(entity, valueToSet);
        }

        await ef.SaveChangesAsync(ct);
    }
    
    static async Task DeleteUserCompletelyAsync(IApplicationDbContext db, UserManager<ApplicationUser> um, ApplicationUser other, CancellationToken ct)
    {
        var staff = await db.Staff.Where(s => s.UserId == other.Id).ToListAsync(ct);
        if (staff.Count > 0) db.Staff.RemoveRange(staff);

        var subs = await db.NotificationSubscriptions.Where(s => s.UserId == other.Id).ToListAsync(ct);
        if (subs.Count > 0) db.NotificationSubscriptions.RemoveRange(subs);

        await db.SaveChangesAsync(ct);
        await um.DeleteAsync(other);
    }
}