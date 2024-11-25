namespace Kori.PageCommands;

public class AddLanguage(IRepository<Page> pages, Translator translator)
{
    public async Task ExecuteAsync(string pageId, string languageId)
    {
        var page = await pages.FindAsync(pageId);
        if (page == null)
            throw new Exception("Page not found!");
        if (page.Languages.Any(x => x.Id == languageId))
            return;
        
        var language = await translator.GetLanguageAsync(languageId);
        if (language == null)
            throw new Exception("Language not found!");
        page.Languages.Add(language);

        await pages.UpdateAsync(page);
    }
}