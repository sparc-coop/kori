using Microsoft.Extensions.Localization;

namespace Sparc.Kori;

public class KoriLocalizer(IKoriContents content) : IStringLocalizer
{
    public IKoriContents Contents { get; } = content;
    Dictionary<string, KoriLocalizedString> Translations { get; set; } = [];

    public async Task InitializeAsync(Uri uri)
    {
        var pageId = uri.GetLeftPart(UriPartial.Path);
        var translations = await Contents.ExecuteQuery("GetAll", pageId);
        if (translations != null)
            Translations = translations.ToDictionary(t => t.Id, t => new KoriLocalizedString(t));
    }

    public LocalizedString this[string name] => Translations.TryGetValue(name, out var value) ? value : new KoriLocalizedString(name);

    public LocalizedString this[string name, params object[] arguments] => Translations.TryGetValue(name, out var value) ? value : new KoriLocalizedString(name);

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => Translations.Values;
}
