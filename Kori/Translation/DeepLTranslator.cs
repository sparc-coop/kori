using DeepL;
using DeepL.Model;

namespace Kori;

public class DeepLTranslator(IConfiguration configuration) : ITranslator
{
    readonly DeepL.Translator Client = new(configuration.GetConnectionString("DeepL")!);

    public static SourceLanguage[]? Languages;

    public async Task<List<Content>> TranslateAsync(Content message, List<Language> toLanguages)
    {
        var translatedMessages = new List<Content>();
        TextTranslateOptions options = new()
        {
            SentenceSplittingMode = SentenceSplittingMode.Off
        };

        // Split the translations into 10 max per call
        foreach (var language in toLanguages)
        {
            var toLanguage = language.Id.ToUpper() == "EN" ? "en-US" : language.Id;
            var result = await Client.TranslateTextAsync(message.Text!, message.Language, toLanguage, options);
            var translatedMessage = new Content(message, language, result.Text);
            translatedMessages.Add(translatedMessage);
            var cost = message.Text!.Length / 1_000_000M * -25.00M; // $25 per 1M characters
            message.AddCharge(0, cost, $"Translate message from {message.Language} to {toLanguage}");
        }

        return translatedMessages;
    }

    public async Task<List<Language>> GetLanguagesAsync()
    {
        Languages ??= await Client.GetSourceLanguagesAsync();
        return Languages
            .Select(x => new Language(x.Code, x.Name, x.Name, false))
            .ToList();
    }
}