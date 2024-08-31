using Application.Contract.ProductCategory.Commands;
using Application.Contract.ProductCategory.Responses;

namespace Client.Infrastructure.Services.Category;

public interface ICategoryService
{
    public Task<ProductCategoryResponse?> GetByName(string name);
    public Task<List<ProductCategoryResponse>?> Get();
    public Task<bool> Post(CreateProductCategoryCommand command);
    public Task<bool> Put(UpdateProductCategoryCommand command);
    public Task<bool> Delete(string slug);
}