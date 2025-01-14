using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Sparc.Kori;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKori(this IServiceCollection services, Uri baseUri, Uri? koriApiUri = null)
    {
        services.AddLocalization();
        koriApiUri ??= new Uri("https://kori.page");
        services.AddBlossomApi<KoriTextContent, IKoriContents>(koriApiUri, "contents");
        services.AddBlossomApi<KoriLanguage>(koriApiUri, "languages");
        services.AddBlossomApi<KoriPage, IKoriPages>(koriApiUri, "pages");


        services.AddScoped<KoriEngine>()
            .AddScoped<KoriLanguageEngine>()
            .AddScoped<KoriEditor>()
            .AddScoped<KoriSearchEngine>()
            .AddScoped<KoriImageEngine>()
            .AddScoped<KoriJsEngine>()
            .AddScoped<KoriLocalizer>();

        services.AddTransient<IClaimsTransformation, KoriClaimsTransformation>();

        KoriEngine.BaseUri = baseUri;
        return services;
    }

    public static IServiceCollection AddKori(this IServiceCollection services, string baseUri, string? hostApi = null)
     => services.AddKori(new Uri(baseUri), hostApi != null ? new Uri(hostApi) : null);
}
