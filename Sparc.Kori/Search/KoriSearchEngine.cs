namespace Sparc.Kori;

public class KoriSearchEngine(KoriJsEngine js)
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

}
