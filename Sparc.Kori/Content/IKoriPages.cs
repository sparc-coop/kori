using Refit;

namespace Sparc.Kori;

public record KoriPage(
    string Id, 
    string Name, 
    List<string> Languages, 
    ICollection<KoriTextContent> Content);

public interface IKoriPages : IBlossomHttpClient<KoriPage>
{
    [Post("/register")]
    Task<KoriPage> Register(string url, string title);
}