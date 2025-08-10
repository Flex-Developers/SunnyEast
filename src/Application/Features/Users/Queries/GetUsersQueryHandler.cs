using Application.Common.Interfaces.Contexts;
using Microsoft.AspNetCore.Identity;
using Domain.Entities;
using Application.Contract.Identity;
using Application.Common.Interfaces.Services;
using Application.Contract.User.Queries;
using Application.Contract.User.Responses;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetUsersQueryHandler(
    IApplicationDbContext context,
    IMapper mapper,
    UserManager<ApplicationUser> userManager,
    ICurrentUserService currentUser // не меняем сервис, просто читаем uid
) : IRequestHandler<GetUserQuery, List<CustomerResponse>>
{
    public async Task<List<CustomerResponse>> Handle(GetUserQuery request, CancellationToken ct)
    {
        // базовый фильтр по пользователям
        var query = context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.UserName))
            query = query.Where(x => x.UserName == request.UserName);
        if (!string.IsNullOrWhiteSpace(request.Name))
            query = query.Where(x => x.Name.Contains(request.Name));
        if (!string.IsNullOrWhiteSpace(request.Phone))
            query = query.Where(x => x.PhoneNumber != null && x.PhoneNumber.Contains(request.Phone));

        var users = await query.AsNoTracking().OrderBy(x => x.Name).ThenBy(x => x.Surname).ToListAsync(ct);

        // staff-идентификаторы (как было)
        var staffSet = (await context.Staff.AsNoTracking().Select(s => s.UserId).ToListAsync(ct)).ToHashSet();

        // супер-админы — одним запросом через UserManager
        var superAdmins = await userManager.GetUsersInRoleAsync(ApplicationRoles.SuperAdmin);
        var superSet = superAdmins.Select(u => u.Id).ToHashSet();

        var myId = currentUser.GetUserId(); // берём как есть, сервис не меняем

        // маппим и проставляем флаги
        var result = users.Select(u =>
        {
            var dto = mapper.Map<CustomerResponse>(u);
            dto.IsStaff = staffSet.Contains(u.Id);
            dto.IsSuperAdmin = superSet.Contains(u.Id);
            dto.IsCurrentUser = (u.Id == myId);
            return dto;
        }).ToList();

        return result;
    }
}