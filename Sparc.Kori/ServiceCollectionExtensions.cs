using Microsoft.Extensions.DependencyInjection;

namespace Sparc.Kori;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKori(this IServiceCollection services, Uri baseUri, Uri? koriApiUri = null)
    {
        services.AddLocalization();
        koriApiUri ??= new Uri("https://kori.page");
        services.AddBlossomApi<KoriTextContent>(koriApiUri, "Contents");
        services.AddBlossomApi<KoriLanguage>(koriApiUri, "Languages");
        services.AddBlossomApi<KoriPage>(koriApiUri, "Pages");


        services.AddScoped<KoriEngine>()
            .AddScoped<KoriLanguageEngine>()
            .AddScoped<KoriContentEngine>()
            .AddScoped<KoriSearchEngine>()
            .AddScoped<KoriImageEngine>()
            .AddScoped<KoriJsEngine>();

        KoriEngine.BaseUri = baseUri;
        return services;
    }

    public static IServiceCollection AddKori(this IServiceCollection services, string baseUri)
     => services.AddKori(new Uri(baseUri));
}
