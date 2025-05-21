using Microsoft.JSInterop;

namespace Sparc.Kori;

public class KoriEngine(
    KoriLanguageEngine language,
    KoriEditor content,
    KoriImageEngine images,
    KoriJsEngine js)
{
    public static Uri BaseUri { get; set; } = new("https://localhost");
    public string Mode { get; set; } = "";
    public KoriEditor Content { get; set; } = content;

    public event EventHandler<EventArgs>? StateChanged;

    public async Task ChangeMode(string mode)
    {
        if (Mode == mode)
        {
            Mode = string.Empty;
            return;
        }

        Mode = mode;

        switch (Mode)
        {
            case "Search":
                await OpenSearchMenuAsync();
                break;
            case "Language":
                OpenTranslationMenu();
                break;
            case "Blog":
                OpenBlogMenu();
                break;
            case "A/B Testing":
                OpenABTestingMenu();
                break;
            case "Edit":
                await Content.BeginEditAsync();
                break;
            case "EditImage":
                await images.BeginEditAsync();
                break;
            default:
                break;
        }
    }

    public async Task CloseAsync()
    {
        Mode = "Default";
        // await Search.CloseAsync();
    }

    public void OpenTranslationMenu()
    {
        Mode = "Language";
    }

    public async Task OpenSearchMenuAsync()
    {
        Mode = "Search";
        // await search.OpenAsync();
    }

    public void OpenBlogMenu()
    {
        Mode = "Blog";
    }

    public void OpenABTestingMenu()
    {
        Mode = "ABTesting";
    }

    [JSInvokable]
    public async Task EditAsync()
    {
        var contentType = await js.CheckSelectedContentType();

        if (contentType == "image")
        {
            Mode = "EditImage";
            await js.EditImage();
        }
        else
        {
            Mode = "Edit";
            await js.Edit();
        }

        InvokeStateHasChanged();
    }

    private void InvokeStateHasChanged()
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    [JSInvokable]
    public void SetDefaultMode()
    {
        Mode = "Default";
        InvokeStateHasChanged();
    }
}