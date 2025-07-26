using Application.Contract.Shops.Responses;

namespace Application.Contract.Shops.Queries;

public class GetShopsQuery : IRequest<List<ShopResponse>>;