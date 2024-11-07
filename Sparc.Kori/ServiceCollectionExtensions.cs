using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sparc.Kori._Plugins;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Sparc.Blossom.Data;
using Sparc.Kori.Content;

namespace Sparc.Kori;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddKori(this WebApplicationBuilder builder, Uri baseUri)
    {
        builder.Services.AddCosmos<KoriContext>(builder.Configuration.GetConnectionString("Database")!, "kori", ServiceLifetime.Transient)
            .AddScoped<PostContent>();

        builder.Services.AddLocalization();
        builder.Services.AddHttpClient<KoriHttpEngine>(client => client.BaseAddress = new Uri("https://localhost:7132"));

        builder.Services.AddScoped<KoriEngine>()
            .AddScoped<KoriLanguageEngine>()
            .AddScoped<KoriContentEngine>()
            .AddScoped<KoriSearchEngine>()
            .AddScoped<KoriImageEngine>()
            .AddScoped<KoriJsEngine>();
        

        KoriEngine.BaseUri = baseUri;
        return builder;
    }

    public static IApplicationBuilder UseKori(this IApplicationBuilder app)
    {
        var supportedCultures = CultureInfo.GetCultures(CultureTypes.AllCultures)
            .Select(x => x.Name)
            .ToArray();

        app.UseRequestLocalization(options => options
            .AddSupportedCultures(supportedCultures)
            .AddSupportedUICultures(supportedCultures));

        //app.UseMiddleware<KoriMiddleware>();

        return app;
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
