namespace Sparc.Blossom.Api;
#nullable disable

public record TranslateContentRequest(Dictionary<string, string> ContentDictionary, bool AsHtml, string LanguageId);