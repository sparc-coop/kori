using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace Sparc.Kori;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddKori(this WebApplicationBuilder builder, Uri baseUri)
    {
        builder.Services.AddLocalization();
        builder.Services.AddScoped<KoriEngine>();
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
}
