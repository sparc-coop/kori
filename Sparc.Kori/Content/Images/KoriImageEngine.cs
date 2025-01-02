using Microsoft.AspNetCore.Components.Forms;

namespace Sparc.Kori;

public class KoriImageEngine(KoriHttpEngine http, KoriJsEngine js)
{
    IBrowserFile? SelectedImage;


    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task BeginEditAsync()
    {
        await js.EditImage();
    }

    public void OnImageSelected(InputFileChangeEventArgs e)
    {
        SelectedImage = e.File;
    }

    public async Task BeginSaveAsync()
    {
        if (SelectedImage == null)
            return;

        var originalImageSrc = await js.GetActiveImageSrc();
        if (originalImageSrc != null)
        {
            var result = await http.SaveContentAsync(originalImageSrc, SelectedImage);
            if (result?.Text != null)
            {
                await js.UpdateImageSrc(originalImageSrc, result.Text);
                Console.WriteLine("Image sent successfully!");
            }
            else
            {
                Console.WriteLine("Error sending image");
            }
        }
    }
}
