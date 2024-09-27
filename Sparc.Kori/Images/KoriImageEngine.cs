using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Headers;

namespace Sparc.Kori;

public class KoriImageEngine(HttpClient client, KoriJsEngine js)
{
    IBrowserFile? SelectedImage;

    KoriContentRequest? CurrentRequest;

    public Task InitializeAsync(string path, string language)
    {
        CurrentRequest = new KoriContentRequest(path, language);
        return Task.CompletedTask;
    }

    public async Task BeginEditAsync()
    {
        await js.InvokeVoidAsync("editImage");
    }

    public void OnImageSelected(InputFileChangeEventArgs e)
    {
        SelectedImage = e.File;
    }

    public async Task BeginSaveAsync()
    {
        var contentType = await js.InvokeAsync<string>("checkSelectedContentType");
        if (contentType == "image" && SelectedImage != null)
        {
            var originalImageSrc = await GetActiveImageSrcFromJs();

            if (originalImageSrc != null)
            {
                await SaveImageAsync(originalImageSrc, SelectedImage);
            }
        }
        else
        {

            await js.InvokeVoidAsync("save");
        }
    }

    async Task<string> GetActiveImageSrcFromJs()
    {
        return await js.InvokeAsync<string>("getActiveImageSrc");
    }

    private async Task SaveImageAsync(string key, IBrowserFile imageFile)
    {
        if (CurrentRequest == null)
        {
            Console.WriteLine("Image engine not initialized");
            return;
        }

        using var content = new MultipartFormDataContent();

        var size15MB = 1024 * 1024 * 15;
        var fileContent = new StreamContent(imageFile.OpenReadStream(maxAllowedSize: size15MB));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(imageFile.ContentType);
        content.Add(fileContent, "File", imageFile.Name);

        content.Add(new StringContent(CurrentRequest.Path), "RoomId");
        content.Add(new StringContent(CurrentRequest.Language), "Language");
        content.Add(new StringContent(key), "Tag");

        var response = await client.PostAsync<KoriTextContent>("publicapi/UploadImage", content);
        if (response != null)
        {
            await js.InvokeVoidAsync("updateImageSrc", key, response.Text);
            Console.WriteLine("Image sent successfully!");
        }
        else
        {
            Console.WriteLine("Error sending image");
        }
    }
}
