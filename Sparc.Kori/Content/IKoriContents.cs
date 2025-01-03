namespace Sparc.Kori;

public record KoriTextContent(
    string Id,
    string OriginalText,
    string? Language = null, 
    string? Text = null, 
    string? Html = null, 
    string? ContentType = null, 
    KoriAudio? Audio = null);

public interface IKoriContents : IBlossomHttpClient<KoriTextContent>
{
}
