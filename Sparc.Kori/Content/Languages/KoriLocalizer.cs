using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using System.Collections.Concurrent;

namespace Sparc.Kori;

public class KoriLocalizer(IKoriContents content, NavigationManager nav, KoriJsEngine js) : IStringLocalizer
{
    public IKoriContents Contents { get; } = content;
    public NavigationManager Nav { get; } = nav;
    public string CurrentPageId { get; private set; }

    ConcurrentDictionary<string, KoriLocalizedString?> Translations { get; set; } = [];

    public async Task InitializeAsync()
    {
        await GetPageContentAsync();
        Nav.LocationChanged += async (s, e) => await GetPageContentAsync();
    }

    public async Task OnAfterRenderAsync(string elementSelector)
    {
        var missingTranslations = await js.Init(elementSelector, Translations, this);
        await AddMissingTranslationsAsync(missingTranslations);
    }

    public LocalizedString this[string name]
    {
        get
        {
            if (!Translations.TryGetValue(name, out var value))
                return value ?? new LocalizedString(name, name);

            Translations.TryAdd(name, null);
            return new LocalizedString(name, name);
        }
    }

    public LocalizedString this[string name, params object[] arguments] => this[name];
    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => Translations.Values.Where(x => x != null);

    private async Task GetPageContentAsync()
    {
        CurrentPageId = new Uri(Nav.Uri).GetLeftPart(UriPartial.Path);
        var translations = await Contents.ExecuteQuery("GetAll", CurrentPageId);
        if (translations != null)
            Translations = new(translations.ToDictionary(t => t.Id, t => new KoriLocalizedString(t)));
    }

    [JSInvokable]
    async Task<IDictionary<string, KoriLocalizedString?>> AddMissingTranslationsAsync(IEnumerable<string> keys)
    {
        var missingKeys = keys.Where(k => !Translations.ContainsKey(k)).ToList();
        if (missingKeys.Count == 0)
            return Translations;

        var missingTranslations = await Contents.ExecuteQuery("GetAll", CurrentPageId, missingKeys);
        foreach (var translation in missingTranslations)
            Translations[translation.Id] = new KoriLocalizedString(translation);

        return Translations;
    }
}
