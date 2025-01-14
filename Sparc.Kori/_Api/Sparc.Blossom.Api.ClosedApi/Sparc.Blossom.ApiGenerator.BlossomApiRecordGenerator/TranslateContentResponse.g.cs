namespace Sparc.Blossom.Api;
#nullable disable

public record TranslateContentResponse(string Domain, string Path, string Id, string Language, Dictionary<string, Content> Content);