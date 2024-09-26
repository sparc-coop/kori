using Microsoft.AspNetCore.Http;

namespace Sparc.Kori;

internal class KoriMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context, KoriEngine kori)
    {
        await kori.InitializeAsync(context);
        await _next(context);
    }
}