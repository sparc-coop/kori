using Sparc.Blossom.Authentication;
using System.Net.Http.Headers;

namespace Sparc.Kori;

public class KoriUser : BlossomUser
{
    public List<KoriLanguage> Languages { get; set; } = [];

    public void SetLanguages(List<KoriLanguage> languages) => Languages = languages;
}
