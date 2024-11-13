using Microsoft.Extensions.Configuration;
using Sparc.Blossom;
using System.Net.Http.Json;

namespace Kori;

internal class AzureTranslator : ITranslator
{
    readonly HttpClient Client;

    internal static LanguageList? Languages;

    internal AzureTranslator(IConfiguration configuration)
    {
        Client = new HttpClient
        {
            BaseAddress = new Uri("https://api.cognitive.microsofttranslator.com"),
        };
        Client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", configuration.GetConnectionString("Cognitive"));
        Client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", "southcentralus");
    }

    public async Task<List<Content>> TranslateAsync(Content message, List<Language> toLanguages)
    {
        var translatedMessages = new List<Content>();

        // Split the translations into 10 max per call
        var batches = Batch(toLanguages, 10);
        foreach (var batch in batches)
        {
            object[] body = [new { message.Text }];
            List<string> translatedTagKeys = [];

            var languageDictionary = batch.ToDictionary(x => x.Id.Split('-').First(), x => x);

            var from = $"&from={message.Language.Split('-').First()}";
            var to = "&to=" + string.Join("&to=", languageDictionary.Keys);

            var result = await Client.PostAsJsonAsync<TranslationResult[]>($"/translate?api-version=3.0{from}{to}", body);

            if (result != null && result.Length > 0)
            {
                var translatedText = result.First();
                var translatedTags = result.Skip(1).ToList();

                foreach (Translation t in translatedText.Translations)
                {
                    var translatedMessage = new Content(message, languageDictionary[t.To], t.Text);

                    translatedMessages.Add(translatedMessage);

                    var cost = message.Text!.Length / 1_000_000M * -10.00M; // $10 per 1M characters
                    message.AddCharge(0, cost, $"Translate message from {message.Language} to {t.To}");
                }
            }
        }

        return translatedMessages;
    }

    public async Task<List<Language>> GetLanguagesAsync()
    {
        Languages ??= await Client.GetFromJsonAsync<LanguageList>("/languages?api-version=3.0&scope=translation");

        return Languages!.translation
            .Select(x => new Language(x.Key, x.Value.name, x.Value.nativeName, x.Value.dir == "rtl"))
            .ToList();
    }

    // from https://stackoverflow.com/a/13731854
    internal static IEnumerable<IEnumerable<T>> Batch<T>(IEnumerable<T> items,
                                                       int maxItems)
    {
        return items.Select((item, inx) => new { item, inx })
                    .GroupBy(x => x.inx / maxItems)
                    .Select(g => g.Select(x => x.item));
    }
}

internal record TranslationResult(DetectedLanguage DetectedLanguage, TextResult SourceText, Translation[] Translations);
internal record DetectedLanguage(string Language, float Score);
internal record TextResult(string Text, string Script);
internal record Translation(string Text, TextResult Transliteration, string To, Alignment Alignment, SentenceLength SentLen);
internal record Alignment(string Proj);
internal record SentenceLength(int[] SrcSentLen, int[] TransSentLen);
internal record LanguageList(Dictionary<string, LanguageItem> translation);//dictionary of languages //List<LanguageItem>> translation);//
internal record LanguageItem(string name, string nativeName, string dir, List<Dialect>? Dialects);