using System.Text.Json;
using System.Text.Json.Serialization;

namespace Client.Infrastructure.Services.HttpClient;

/// <summary>Единый JsonSerializerOptions с Enum-строками.</summary>
public static class HttpJsonOptions
{
    public static readonly JsonSerializerOptions Instance = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}