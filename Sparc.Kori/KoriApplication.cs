using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Sparc.Blossom;
using Sparc.Blossom.Authentication;
using Sparc.Blossom.Realtime;
using System.Reflection;

namespace Sparc.Kori;

public static class KoriApplication
{
    public static WebApplication Run<TApp>(
        string[] args,
        Uri baseUri,
        Action<WebApplicationBuilder>? builderOptions = null,
        Action<WebApplication>? appOptions = null,
        IComponentRenderMode? renderMode = null,
        Assembly? apiAssembly = null)
    {
        return BlossomApplication.Run<TApp>(args, builder =>
        {
            builderOptions?.Invoke(builder);
            builder.AddKori(baseUri);
        }, app =>
        {
            appOptions?.Invoke(app);
            app.UseKori();
        }, renderMode, apiAssembly);
    }

    public static WebApplication Run<TApp, TUser>(
        string[] args,
        Uri baseUri,
        Action<WebApplicationBuilder>? builderOptions = null,
        Action<WebApplication>? appOptions = null,
        IComponentRenderMode? renderMode = null)
        where TUser : BlossomUser, new()
    {
        return BlossomApplication.Run<TApp, TUser>(args, builder =>
        {
            builderOptions?.Invoke(builder);
            builder.AddKori(baseUri);
        }, app =>
        {
            appOptions?.Invoke(app);
            app.UseKori();
        }, renderMode);
    }

    public static WebApplication Run<TApp, TUser, THub>(
        string[] args,
        Uri baseUri,
        Action<WebApplicationBuilder>? builderOptions = null,
        Action<WebApplication>? appOptions = null,
        IComponentRenderMode? renderMode = null)
        where TUser : BlossomUser, new()
        where THub : BlossomHub
    {
        return BlossomApplication.Run<TApp, TUser, THub>(args, builder =>
        {
            builderOptions?.Invoke(builder);
            builder.AddKori(baseUri);
        }, app =>
        {
            appOptions?.Invoke(app);
            app.UseKori();
        }, renderMode);
    }
}
