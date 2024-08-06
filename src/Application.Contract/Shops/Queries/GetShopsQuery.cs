using Domain.Entities;

namespace Application.Contract.Shops.Queries;

public class GetShopsQuery : IRequest<List<Shop>>;