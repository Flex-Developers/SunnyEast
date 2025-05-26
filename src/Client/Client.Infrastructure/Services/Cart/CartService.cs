using Application.Contract.Cart.Commands;
using Application.Contract.Cart.Queries;
using Application.Contract.Cart.Responses;
using Client.Infrastructure.Exceptions;
using Client.Infrastructure.Services.HttpClient;

namespace Client.Infrastructure.Services.Cart;

public class CartService(IHttpClientService httpClientService) : ICartService
{
    private const string CartEndpoint = "/api/cart";

    public async Task<bool> PostAsync(CreateCartCommand command)
    {
        var serverResponse = await httpClientService.PostAsJsonAsync<CreateCartCommand?>(CartEndpoint, command);

        if (!serverResponse.Success)
        {
            throw new HttpRequestFailedException(
                httpClientService.ExceptionMessage ?? "Не удалось создать корзину",
                (int?)serverResponse.StatusCode
            );
        }

        return true;
    }

    public async Task<bool> PutAsync(UpdateCartCommand command)
    {
        var serverResponse = await httpClientService.PutAsJsonAsync(CartEndpoint, command);

        if (!serverResponse.Success)
        {
            throw new HttpRequestFailedException(
                httpClientService.ExceptionMessage ?? "Не удалось обновить корзину",
                (int?)serverResponse.StatusCode
            );
        }

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var serverResponse = await httpClientService.DeleteAsync($"{CartEndpoint}/{id}");

        if (!serverResponse.Success)
        {
            throw new HttpRequestFailedException(
                httpClientService.ExceptionMessage ?? "Не удалось удалить корзину",
                (int?)serverResponse.StatusCode
            );
        }

        return true;
    }

    public async Task<CartResponse> GetAsync(GetCartQuery query)
    {
        var url = $"{CartEndpoint}/get?Slug={query.Slug}";
        var serverResponse = await httpClientService.GetFromJsonAsync<CartResponse>(url);

        if (!serverResponse.Success || serverResponse.Response == null)
            throw new HttpRequestFailedException(httpClientService.ExceptionMessage ?? "Не удалось получить корзину");

        return serverResponse.Response;
    }

    public async Task<List<CartResponse>> GetAllAsync(GetCartsQuery query)
    {
        var url = $"{CartEndpoint}/all?ShopSlug={query.ShopSlug}";
        var serverResponse = await httpClientService.GetFromJsonAsync<List<CartResponse>>(url);

        if (!serverResponse.Success || serverResponse.Response == null)
            throw new HttpRequestFailedException(httpClientService.ExceptionMessage ?? "Не удалось получить список корзин");

        return serverResponse.Response;
    }
}