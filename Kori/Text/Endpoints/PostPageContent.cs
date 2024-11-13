
using Microsoft.EntityFrameworkCore;

namespace Kori.Text.Endpoints;

public record PostPageContentRequest(string Domain, string Path, string Language, Dictionary<string, string>? Content = null, bool AsHtml = false);
public record GetAllContentResponse(string Domain, string Path, string Language, Dictionary<string, Content> Content);
public class PostPageContent(IRepository<Page> pages, IRepository<Content> content, Translator translator)
{
    private IRepository<Page> Pages { get; } = pages;
    private IRepository<Content> Content { get; } = content;
    private Translator Translator { get; } = translator;

    internal async Task<GetAllContentResponse> ExecuteAsync(PostPageContentRequest request)
    {
        var page = await GetPageAsync(request.Domain, request.Path);

        await AddLanguageIfNeeded(page, request.Language);
        await TranslateContentAsync(request, page);

        //var content = await GetAllMessagesAsDictionaryAsync(request, page, null);

        var response = new GetAllContentResponse(page.Domain, page.Path, "en", new Dictionary<string, Content>());

        return response;
    }

    private async Task<Page> GetPageAsync(string domain, string path)
    {
        var page = await Pages.Query.FirstOrDefaultAsync(x => x.Domain == domain && x.Path == path);

        if (page == null)
        {
            page = new Page(domain, path, null);
            await Pages.AddAsync(page);
        }

        return page;
    }
    private async Task AddLanguageIfNeeded(Page page, string languageId)
    {
        if (!page.Languages.Any(x => x.Id == languageId))
        {
            var newLanguage = await Translator.GetLanguageAsync(languageId);

            if (newLanguage == null) throw new Exception("Language not found");

            page.AddLanguage(newLanguage);

            await Pages.UpdateAsync(page);
        }
    }
    private async Task TranslateContentAsync(PostPageContentRequest request, Page page)
    {
        if (request.Content == null || request.Content.Count == 0)
            return;

        foreach (var (key, value) in request.Content)
        {
            var tag = key?.Trim();
            var text = value?.Trim();

            if (!string.IsNullOrEmpty(tag) || !string.IsNullOrEmpty(text))
            {
                //var contentRequest = new TypeContentRequest(
                //    page.Name,
                //    request.Language,
                //    text,
                //    tag
                //);
                //var createdContent = await TypeContent.ExecuteAsUserAsync(contentRequest, null);
            }
            else
            {
                Console.WriteLine($"Ignoring message with empty Tag or Text. Tag = '{tag}', Text = '{text}'");
            }
        }
    }
}
