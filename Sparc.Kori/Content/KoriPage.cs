using Refit;

namespace Sparc.Kori;

public record KoriPage(string Name, string Domain, string Path, List<string> Languages, ICollection<KoriTextContent> Content, string Id);

public interface IKoriPages : IBlossomHttpClient<KoriPage>
{
    [Post("/register")]
    Task<KoriPage> Register(string url, string title);
}