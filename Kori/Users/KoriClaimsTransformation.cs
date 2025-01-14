using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace Sparc.Kori;

public class KoriClaimsTransformation(IHttpContextAccessor http) : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var languages = http?.HttpContext.Request.Headers["Accept-Language"].ToString();
        if (!string.IsNullOrWhiteSpace(languages))
        {
            var languageIdentity = new ClaimsIdentity("Kori");
            languageIdentity.AddClaim(new(ClaimTypes.Locality, languages));
            principal.AddIdentity(languageIdentity);
        }

        return Task.FromResult(principal);
    }
}
