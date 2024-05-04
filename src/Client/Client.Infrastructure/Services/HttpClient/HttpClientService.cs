using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Contract.User.Responses;
using Blazored.LocalStorage;
using Client.Infrastructure.Consts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MudBlazor;
using Newtonsoft.Json;

namespace Client.Infrastructure.Services.HttpClient;

public class HttpClientService(
    IHttpClientFactory httpClientFactory,
    ISnackbar snackbar,
    ILocalStorageService localStorageService,
    IConfiguration configuration)
    : IHttpClientService
{
    private readonly string _baseUrl = configuration[Config.ApiBaseUrl]!;
    private string? _exceptionMessage;

    public async Task<ServerResponse> GetAsync(string url)
    {
        Console.WriteLine(_baseUrl + "fadfasdfsadfad");
        var response = await SendAsync(new HttpRequestMessage(HttpMethod.Get, _baseUrl + url));
        return new ServerResponse
        {
            Success = response?.IsSuccessStatusCode ?? false,
            StatusCode = response?.StatusCode
        };
    }

    public async Task<ServerResponse<T>> GetFromJsonAsync<T>(string url)
    {
        var response = await SendAsync(new HttpRequestMessage(HttpMethod.Get, _baseUrl + url));
        return new ServerResponse<T>
        {
            Success = response?.IsSuccessStatusCode ?? false,
            StatusCode = response?.StatusCode,
            Response = response?.IsSuccessStatusCode == true ? await response.Content.ReadFromJsonAsync<T>() : default
        };
    }

    public async Task<ServerResponse> PostAsJsonAsync(string url, object content)
    {
        var response = await SendAsync(new HttpRequestMessage(HttpMethod.Post, _baseUrl + url)
            { Content = JsonContent.Create(content) });
        //  response!.EnsureSuccessStatusCode();
        return new ServerResponse
        {
            Success = response?.IsSuccessStatusCode ?? false,
            StatusCode = response?.StatusCode,
        };
    }

    public async Task<ServerResponse<T>> PostAsJsonAsync<T>(string url, object content)
    {
        var response = await SendAsync(new HttpRequestMessage(HttpMethod.Post, _baseUrl + url)
            { Content = JsonContent.Create(content) });
        T? resultContent = default;
        if (response?.IsSuccessStatusCode == true)
        {
            resultContent = await response.Content.ReadFromJsonAsync<T>();
        }

        return new ServerResponse<T>
        {
            Success = response?.IsSuccessStatusCode ?? false,
            StatusCode = response?.StatusCode,
            Response = resultContent
        };
    }

    public async Task<ServerResponse> PutAsJsonAsync(string url, object content)
    {
        var response = await SendAsync(new HttpRequestMessage(HttpMethod.Put, _baseUrl + url)
            { Content = JsonContent.Create(content) });
        return new ServerResponse
        {
            Success = response?.IsSuccessStatusCode ?? false,
            StatusCode = response?.StatusCode
        };
    }

    public async Task<ServerResponse> DeleteAsync(string url)
    {
        var response = await SendAsync(new HttpRequestMessage(HttpMethod.Delete, _baseUrl + url));
        return new ServerResponse
        {
            Success = response?.IsSuccessStatusCode ?? false,
            StatusCode = response?.StatusCode
        };
    }

    private async Task<HttpResponseMessage?> SendAsync(HttpRequestMessage request)
    {
        try
        {
            var token = await localStorageService.GetItemAsync<JwtTokenResponse>("authToken");
            var client = httpClientFactory.CreateClient(Startup.SunnyEastClientName);
            if (token != null)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            }

            var response = await client.SendAsync(request);
            await CheckForException(response);
            // if (!response.IsSuccessStatusCode)
            // {
            //     var errorMessage = await response.Content.ReadAsStringAsync();
            //     snackbar.Add(errorMessage);
            // }
            // if (response.StatusCode == HttpStatusCode.BadRequest)
            // {
            //     //todo get message from api and show in snackbar
            // }

            //todo create handlers for another status codes 409 500 and others;
            return response;
        }
        catch (Exception e)
        {
            if (_exceptionMessage != null)
                throw new Exception(_exceptionMessage);
            
            snackbar.Add("Проблема соеденения с сервером. Попробуйте попытку позже.");
            Console.WriteLine(e);
        }

        return null;
    }

    private async Task CheckForException(HttpResponseMessage? response)
    {
        _exceptionMessage = null;
        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            var problemDetails = JsonConvert.DeserializeObject<ProblemDetails>(errorMessage);
            if (problemDetails != null && !string.IsNullOrEmpty(problemDetails.Title))
            {
                _exceptionMessage = problemDetails.Title;
                throw new Exception(_exceptionMessage);
            }
        }
    }
}