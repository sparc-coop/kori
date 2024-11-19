using static System.Net.WebRequestMethods;

namespace Sparc.Kori;

//public record KoriSearch(string ContentId, string Tag, string Text, string Domain, string Path);

public class KoriSearchEngine(KoriHttpEngine http, KoriJsEngine js)
{
    public async Task OpenAsync()
    {
        await js.InvokeVoidAsync("showSidebar");
    }
    
    public async Task CloseAsync()
    {
        Console.WriteLine("Closing search side bar in Kori.cs");
        await js.InvokeVoidAsync("closeSearch");
    }

    public async Task<List<KoriSearch>> SearchAsync(string searchTerm)
    {
        await http.SearchContentAsync(searchTerm);
        await http.SearchPageAsync(searchTerm);
    }
}
