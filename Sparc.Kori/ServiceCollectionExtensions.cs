using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text.Json;

namespace Sparc.Kori;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKori(this IServiceCollection services, Uri baseUri)
    {
        services.AddLocalization();
        // services.AddHttpClient<KoriHttpEngine>(client => client.BaseAddress = new Uri("https://localhost:53579"));

        services.AddScoped<KoriEngine>()
            .AddScoped<KoriLanguageEngine>()
            .AddScoped<KoriContentEngine>()
            .AddScoped<KoriSearchEngine>()
            .AddScoped<KoriImageEngine>()
            .AddScoped<KoriJsEngine>()
            .AddScoped(typeof(DexieRepository<>));

        KoriEngine.BaseUri = baseUri;
        return services;
    }

    static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    public static async Task<TResponse?> PostAsync<TResponse>(this HttpClient client, string url, object request)
    {
        try
        {
            var response = await client.PostAsJsonAsync(url, request);
            var result = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TResponse>(result, JsonOptions);
        }
        catch (Exception)
        {
            return default;
        }
    }
}
