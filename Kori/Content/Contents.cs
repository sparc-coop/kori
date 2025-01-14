namespace Kori;

public class Contents(BlossomAggregateOptions<Content> options, IRepository<Page> pages, KoriTranslatorProvider translator) : BlossomAggregate<Content>(options)
{
    public BlossomQuery<Content> Search(string searchTerm) => Query().Where(content =>
         ((content.Text != null && content.Text.ToLower().Contains(searchTerm) == true) ||
         (content.OriginalText != null && content.OriginalText.ToLower().Contains(searchTerm) == true) ||
         (content.Domain != null && content.Domain.ToLower().Contains(searchTerm) == true) ||
         (content.Path != null && content.Path.ToLower().Contains(searchTerm) == true)));

    public async Task<IEnumerable<Content>> GetAll(string pageId, string? fallbackLanguageId = null, IEnumerable<string>? missingTranslations = null)
    {
        try
        {
            var language = await translator.GetLanguageAsync(User, fallbackLanguageId);
            var page = await pages.FindAsync(pageId);
            if (page == null)
            {
                await pages.AddAsync(new Page(pageId));
                page = await pages.FindAsync(pageId);
            }

            var original = (await Original(pageId).Execute()).ToList();

            if (missingTranslations != null)
            {
                var newContent = missingTranslations
                    .Where(x => !original.Any(y => y.OriginalText == x))
                    .Select(x => new Content(pageId, language, x))
                    .ToList();

                await Repository.AddAsync(newContent);
                original.AddRange(newContent);
            }

            var content = await page!.TranslateAsync(original, language, translator);
            return content;
        }
        catch (Exception e)
        {
            throw new Exception($"Failed to get content for page {pageId}", e);
        }
    }

    public BlossomQuery<Content> Original(string pageId) => Query().Where(x => x.PageId == pageId && x.SourceContentId == null);

    public BlossomQuery<Content> All(string pageId) => Query().Where(content => content.PageId == pageId && content.SourceContentId == null);

    public async Task<IEnumerable<Language>> Languages()
        => await translator.GetLanguagesAsync();
}
