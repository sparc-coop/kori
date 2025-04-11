using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using System.Collections.Concurrent;

namespace Sparc.Kori;

public class KoriLocalizer(IKoriContents content, NavigationManager nav, KoriJsEngine js) : IStringLocalizer
{
    public IKoriContents Contents { get; } = content;
    public NavigationManager Nav { get; } = nav;
    public string? CurrentPageId { get; private set; }

    ConcurrentDictionary<string, KoriLocalizedString?> Translations { get; set; } = [];

    public async Task InitializeAsync()
    {
        Translations = [];
        CurrentPageId = new Uri(Nav.Uri).GetLeftPart(UriPartial.Path);

        await UpdateTranslationsAsync();
        Nav.LocationChanged += async (s, e) => await UpdateTranslationsAsync();
    }

    public async Task OnAfterRenderAsync(string elementSelector, bool firstRender = true)
    {
        var missingTranslations = firstRender
            ? await js.Init(elementSelector, Translations, this)
            : [];

        await UpdateTranslationsAsync(missingTranslations);
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
    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => Translations.Values.Where(x => x != null)!;

    [JSInvokable]
    public async Task<IDictionary<string, KoriLocalizedString?>> UpdateTranslationsAsync(IEnumerable<string>? missingTranslations = null)
    {
        if (CurrentPageId == null)
            throw new InvalidOperationException("You must initialize Kori via Kori.InitializeAsync() before using the translator.");

        if (missingTranslations != null)
            Register(missingTranslations);

        missingTranslations = Translations.Keys.Where(x => Translations[x] == null);

        var translations = await Contents.ExecuteQuery("GetAll", CurrentPageId!, missingTranslations);
        if (translations != null)
            Update(translations);

        return Translations;
    }

    private void Register(IEnumerable<string> names)
    {
        foreach (var name in names)
            Register(name);
    }

    private void Register(string name)
    {
        Translations.TryAdd(name, null);
    }

    private void Update(KoriTextContent translation)
    {
        Translations.AddOrUpdate(translation.Id, x => new KoriLocalizedString(translation), (x, v) => new KoriLocalizedString(translation));
    }

    private void Update(IEnumerable<KoriTextContent> translations)
    {
        foreach (var translation in translations)
            Update(translation);
    }
}
