// using Application.Common.Interfaces.Contexts;
// using Application.Common.Interfaces.Services;
// using Application.Contract.Identity;
// using Application.Contract.Order.Queries;
// using Application.Contract.Order.Responses;
// using AutoMapper;
// using AutoMapper.QueryableExtensions;
// using MediatR;
// using Microsoft.EntityFrameworkCore;
//
// namespace Application.Features.Orders.Queries;
//
// public sealed class GetSalesmanOrdersQueryHandler(IApplicationDbContext context, IMapper mapper, ICurrentUserService currentUser)
//     : IRequestHandler<GetSalesmanOrdersQuery, List<OrderResponse>>
// {
//     public async Task<List<OrderResponse>> Handle(GetSalesmanOrdersQuery request, CancellationToken cancellationToken)
//     {
//         var userRole = currentUser.GetUserRole();
//         var userName = currentUser.GetUserName();
//
//         if (userRole is not (ApplicationRoles.Salesman or ApplicationRoles.Administrator))
//             throw new UnauthorizedAccessException("Недостаточно прав.");
//
//         // Получаем магазины, которыми владеет/управляет текущий пользователь.
//         // Замените на свою модель (OwnerId / таблица связей ShopUsers и т.п.)
//         var myShopIds = await context.Shops
//             .Where(s => s. == userName) // <-- ваша логика
//             .Select(s => s.Id)
//             .ToListAsync(cancellationToken);
//
//         var orders = context.Orders
//             .Include(o => o.Customer)
//             .Include(o => o.OrderItems)
//             .Include(o => o.Shop)
//             .Where(o => myShopIds
//                 .Contains(o.ShopId)); // <-- убедитесь, что у Order есть ShopId. Если нет – фильтруйте по Slug.
//
//         if (!string.IsNullOrWhiteSpace(request.ShopSlug))
//             orders = orders.Where(o => o.ShopSlug == request.ShopSlug);
//
//         orders = request.OnlyArchived
//             ? orders.Where(o => o.IsInArchive)
//             : orders.Where(o => !o.IsInArchive);
//
//         return await orders
//             .ProjectTo<OrderResponse>(mapper.ConfigurationProvider)
//             .OrderByDescending(o => o.CreatedAt)
//             .ToListAsync(cancellationToken);
//     }
// }