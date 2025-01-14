namespace Sparc.Blossom.Api;
#nullable enable
public partial class Contents : BlossomAggregateProxy<Content>
{
    public Contents(IRunner<Content> runner) : base(runner) { }

    public async Task<Content> Create(string pageId, Language language, string text, KoriUser? user, string? originalText, string contentType) => await Runner.Create(pageId, language, text, user, originalText, contentType);

    
    public async Task<IEnumerable<Content>> Search(string searchTerm) => await Runner.ExecuteQuery("Search", searchTerm);
public async Task<IEnumerable<Content>> Original(string pageId) => await Runner.ExecuteQuery("Original", pageId);
public async Task<IEnumerable<Content>> All(string pageId) => await Runner.ExecuteQuery("All", pageId);
public async Task<IEnumerable<Content>> GetAll(string pageId, string? fallbackLanguageId, IEnumerable<string>? missingTranslations) => await Runner.ExecuteQuery<IEnumerable<Content>>("GetAll", pageId, fallbackLanguageId, missingTranslations);
public async Task<IEnumerable<Language>> Languages() => await Runner.ExecuteQuery<IEnumerable<Language>>("Languages");

}