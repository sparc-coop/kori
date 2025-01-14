namespace Sparc.Blossom.Api;
#nullable enable
public partial class Pages : BlossomAggregateProxy<Page>
{
    public Pages(IRunner<Page> runner) : base(runner) { }

    public async Task<Page> Create(string id, string name) => await Runner.Create(id, name);
public async Task<Page> Create(Uri uri, string name) => await Runner.Create(uri, name);

    
    public async Task<IEnumerable<Page>> Search(string searchTerm) => await Runner.ExecuteQuery("Search", searchTerm);
public async Task<Page> Register(string url, string title) => await Runner.ExecuteQuery<Page>("Register", url, title);

}