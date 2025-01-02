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
            .AddScoped<KoriHttpEngine>()
            .AddScoped<KoriJsEngine>();

        KoriEngine.BaseUri = baseUri;
        return services;
    }

    public static IServiceCollection AddKori(this IServiceCollection services, string baseUri)
     => services.AddKori(new Uri(baseUri));
}
