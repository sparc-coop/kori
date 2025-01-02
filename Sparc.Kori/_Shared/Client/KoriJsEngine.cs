using Microsoft.JSInterop;

namespace Sparc.Kori;

public class KoriJsEngine(IJSRuntime js) 
    : BlossomJsRunner(js, "./_content/Sparc.Blossom/Kori/KoriApp.razor.js")
{
    public async Task Init(string elementId) => await InvokeVoidAsync("init", elementId);

    public async Task<string> CheckSelectedContentType() 
        => await InvokeAsync<string>("checkSelectedContentType");

    public async Task EditImage() => await InvokeVoidAsync("editImage");

    public async Task Edit() => await InvokeVoidAsync("edit");

    public async Task ApplyMarkdown(string symbol, string position) 
        => await InvokeVoidAsync("applyMarkdown", symbol, position);

    public async Task<string> GetActiveImageSrc()
        => await InvokeAsync<string>("getActiveImageSrc");

    public async Task UpdateImageSrc(string originalSrc, string newSrc)
        => await InvokeVoidAsync("updateImageSrc", originalSrc, newSrc);

    public async Task<string> GetPageTitle()
        => await InvokeAsync<string>("getPageTitle");

    public async Task Save() => await InvokeVoidAsync("save");
}