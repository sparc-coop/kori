using Sparc.Blossom.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparc.Kori.Content;

public record PostContentRequest(string RoomSlug, string Language, Dictionary<string, string>? Messages = null, bool AsHtml = false);
public record GetAllContentResponse(string Name, string Slug, string Language, Dictionary<string, string> Content);
public class PostContent(IRepository<Page> pages)
{
    public IRepository<Page> Pages { get; } = pages;

    public async Task<GetAllContentResponse> ExecuteAsync(PostContentRequest request)
    {
        var page = await GetPageAsync(request.RoomSlug);

        var response = new GetAllContentResponse(page.Name, page.Slug, page.Language, new Dictionary<string, string>());

        return response;
    }

    private async Task<Page> GetPageAsync(string slug)
    {
        var page = Pages.Query.FirstOrDefault(x => x.Name == slug || x.Slug == slug);

        if(page == null)
        {
            page = new Page(slug, "Content");
            await Pages.AddAsync(page);
        }

        return page;
    }
}

