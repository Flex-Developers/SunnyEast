using Application.Contract.ProductCategory.Commands;
using Application.Contract.ProductCategory.Responses;
using Client.Infrastructure.Services.HttpClient;

namespace Client.Infrastructure.Services.Category;

public class CategoryService(IHttpClientService httpClient) : ICategoryService
{
    public async Task<List<ProductCategoryResponse>?> Get()
    {
        var serverResponse = await httpClient.GetFromJsonAsync<List<ProductCategoryResponse>>("/api/productCategory");
        return serverResponse.Success ? serverResponse.Response : null;
    }

    public async Task<bool> Post(CreateProductCategoryCommand command)
    {
        var serverResponse = await httpClient.PostAsJsonAsync("/api/productCategory", command);
        return serverResponse.Success;
    }

    public async Task<bool> Put(UpdateProductCategoryCommand command)
    {
        var serverResponse = await httpClient.PutAsJsonAsync("/api/productCategory", command);
        return serverResponse.Success;
    }

    public async Task<bool> Delete(string slug)
    {
        var serverResponse = await httpClient.DeleteAsync("/api/productCategory/slug");
        return serverResponse.Success;
    }
}