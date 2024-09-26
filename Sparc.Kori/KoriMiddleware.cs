using Microsoft.AspNetCore.Http;

namespace Kori;

internal class KoriMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context, Kori kori)
    {
        await kori.InitializeAsync(context);
        await _next(context);
    }
}