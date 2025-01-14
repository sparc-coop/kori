using System.Text;

namespace Kori;

public record SourceContent(string PageId, string ContentId);
public record TranslateContentRequest(Dictionary<string, string> ContentDictionary, bool AsHtml, string LanguageId);
public record TranslateContentResponse(string Domain, string Path, string Id, string Language, Dictionary<string, Content> Content);

public class Page : BlossomEntity<string>
{
    public string Domain { get; private set; }
    public string Path { get; private set; }
    public string Name { get; private set; }
    public SourceContent? SourceContent { get; private set; }
    public List<Language> Languages { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? LastActiveDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public AudioContent? Audio { get; private set; }
    private ICollection<Content> Contents { get; set; } = [];

    internal Page(string id)
    {
        Id = id;
        Domain = new Uri(id).Host;
        Path = new Uri(id).AbsolutePath;
        Name = "New Page";
        Languages = [];
        StartDate = DateTime.UtcNow;
        LastActiveDate = DateTime.UtcNow;
    }

    public Page(string id, string name) : this(id)
    {
        Name = name;
    }

    public Page(Uri uri, string name) : this(uri.ToString(), name)
    {
    }

    internal Page(Page page, Content content) : this(page.Id, page.Name)
    {
        // Create a subpage from a message

        SourceContent = new(page.Id, content.Id);
        //Languages = page.Languages;
        //ActiveUsers = page.ActiveUsers;
        //Translations = page.Translations;
    }

    public void AddLanguage(Language language)
    {
        if (Languages.Any(x => x.Id == language.Id))
            return;

        Languages.Add(language);
    }

    internal async Task<ICollection<Content>> TranslateAsync(Language toLanguage, KoriTranslatorProvider provider)
    {
        var needsTranslation = Contents.Where(x => !x.HasTranslation(toLanguage)).ToList();
        if (needsTranslation.Count == 0)
            return Contents;

        var languages = needsTranslation.GroupBy(x => x.Language);
        foreach (var language in languages)
        {
            var translator = await provider.For(language.Key, toLanguage);
            if (translator == null)
                continue;

            var translatedContents = await translator.TranslateAsync(language, toLanguage);
            foreach (var translatedContent in translatedContents)
            {
                var existing = needsTranslation.FirstOrDefault(x => x.Id == translatedContent.SourceContentId);
                existing?.AddTranslation(translatedContent);
            }
        }

        return Contents;
    }

    internal async Task SpeakAsync(ISpeaker speaker, List<Content> contents)
    {
        Audio = await speaker.SpeakAsync(contents);
    }

    internal void Close()
    {
        EndDate = DateTime.UtcNow;
    }
    
    internal void UpdateName(string name) => Name = name;

    internal async Task<IEnumerable<Content>> TranslateAsync(ICollection<Content> contents, Language language, KoriTranslatorProvider translator)
    {
        Contents = contents;
        return await TranslateAsync(language, translator);
    }
}

