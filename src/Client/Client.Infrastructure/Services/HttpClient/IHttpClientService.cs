﻿namespace Client.Infrastructure.Services.HttpClient;

public interface IHttpClientService
{
    public Task<ServerResponse> GetAsync(string url);
    public Task<ServerResponse<T>> GetFromJsonAsync<T>(string url);

    public Task<ServerResponse> PostAsJsonAsync(string url, object content);

    public Task<ServerResponse> PutAsJsonAsync(string url, object content);

    public Task<ServerResponse> DeleteAsync(string url);
}