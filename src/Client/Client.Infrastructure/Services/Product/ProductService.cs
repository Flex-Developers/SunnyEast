using Application.Contract.Product.Commands;
using Application.Contract.Product.Queries;
using Application.Contract.Product.Responses;
using Client.Infrastructure.Services.HttpClient;

namespace Client.Infrastructure.Services.Product;

public class ProductService(IHttpClientService httpClient) : IProductService
{
    private const string BaseProductUrl = "/api/product";

    public async Task<bool> Post(CreateProductCommand command)
    {
        var serverResponse = await httpClient.PostAsJsonAsync(BaseProductUrl, command);
        return serverResponse.Success;
    }

    public async Task<bool> Put(UpdateProductCommand command)
    {
        var serverResponse = await httpClient.PutAsJsonAsync(BaseProductUrl, command);
        return serverResponse.Success;
    }

    public async Task<ProductResponse?> Get(string slug)
    {
        var serverResponse = await httpClient.GetFromJsonAsync<List<ProductResponse>>($"{BaseProductUrl}?slug={slug}");
        return serverResponse.Success ? serverResponse.Response?.FirstOrDefault() ?? null : null;
    }

    public async Task<List<ProductResponse>> Get(GetProductsQuery query)
    {
        var serverResponse = await httpClient.GetFromJsonAsync<List<ProductResponse>>(BaseProductUrl);
        return serverResponse.Success ? serverResponse.Response ?? [] : [];
    }

    public async Task<bool> Delete(string slug)
    {
        var serverResponse = await httpClient.DeleteAsync($"{BaseProductUrl}/{slug}");
        return serverResponse.Success;
    }
}