using Application.Common.Interfaces.Contexts;
using Application.Contract.Shops.Queries;
using Application.Contract.Shops.Responses;
using Application.Contract.Staff.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Shops.Queries;

public sealed class GetShopsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetShopsQuery, List<ShopResponse>>
{
    public async Task<List<ShopResponse>> Handle(GetShopsQuery req, CancellationToken ct)
    {
        // 1) Магазины
        var shops = await context.Shops
            .AsNoTracking()
            .Select(s => new { s.Id, s.Slug, s.Address, s.Images })
            .OrderBy(s => s.Address)
            .ToListAsync(ct);

        // 2) Сотрудники: без локального массива, через джойн на Shops
        var staffRows = await context.Staff
            .AsNoTracking()
            .Where(s => s.ShopId != null
                        && s.IsActive
                        && s.StaffRole != Domain.Enums.StaffRole.None)
            .Join(context.Shops.AsNoTracking().Select(sh => sh.Id),
                  s  => s.ShopId!.Value,
                  id => id,
                  (s, _) => new
                  {
                      ShopId   = s.ShopId!.Value,
                      s.UserId,
                      s.StaffRole,
                      s.User.Name,
                      s.User.Surname,
                      s.User.UserName
                  })
            .ToListAsync(ct);

        // 3) Группировка
        var staffByShop = staffRows
            .GroupBy(x => x.ShopId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => new ShopStaffBriefResponse
                {
                    UserId      = x.UserId,
                    Role        = (StaffRole)x.StaffRole,
                    DisplayName = DisplayName(x.Name, x.Surname, x.UserName!)
                })
                .OrderByDescending(p => p.Role)
                .ThenBy(p => p.DisplayName)
                .ToList()
            );

        // 4) Ответ
        return shops.Select(s => new ShopResponse
        {
            Slug    = s.Slug,
            Address = s.Address,
            Images  = s.Images,
            Staff   = staffByShop.TryGetValue(s.Id, out var list) ? list : new()
        }).ToList();
    }

    private static string DisplayName(string? name, string? surname, string userName)
    {
        var full = $"{name} {surname}".Trim();
        return string.IsNullOrWhiteSpace(full) ? userName : full;
    }
}
