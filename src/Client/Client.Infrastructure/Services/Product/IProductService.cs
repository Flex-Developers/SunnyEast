using Application.Contract.Product.Commands;
using Application.Contract.Product.Queries;
using Application.Contract.Product.Responses;

namespace Client.Infrastructure.Services.Product;

public interface IProductService
{
    public Task<bool> Post(CreateProductCommand command);
    public Task<bool> Put(UpdateProductCommand command);
    public Task<ProductResponse?> Get(string slug);
    public Task<List<ProductResponse>> Get(GetProductsQuery query);
    public Task<List<ProductResponse>> GetByCategoryName(string categoryName);
    public Task<bool> Delete(string slug);
}