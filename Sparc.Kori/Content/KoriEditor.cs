using Microsoft.JSInterop;
using System.Text;

namespace Sparc.Kori;
public record KoriAudio(string Url, long Duration, string Voice, ICollection<KoriWord> Subtitles);
public record KoriWord(string Text, long Duration, long Offset);

public class KoriEditor(IJSRuntime js)
{
    public async Task ApplyMarkdown(string symbol, string position) =>
        throw new NotImplementedException();
        // await js.ApplyMarkdown(symbol, position);

    public async Task PlayAsync(KoriTextContent content)
    {
        if (content?.Audio?.Url == null)
            return;

        await js.InvokeVoidAsync("playAudio", content.Audio.Url);
    }

    public async Task BeginEditAsync()
    {
        await js.InvokeVoidAsync("edit");
    }

    public async Task ApplyMarkdown(string symbol)
    {
        await js.InvokeVoidAsync("applyMarkdown", symbol);
    }

    // public async Task Save() => await js.Save();

    public async Task CancelAsync()
    {
        await js.InvokeVoidAsync("cancelEdit");
    }

    static string LoremIpsum(int wordCount)
    {
        var words = new[]{"lorem", "ipsum", "dolor", "sit", "amet", "consectetuer",
        "adipiscing", "elit", "sed", "diam", "nonummy", "nibh", "euismod",
        "tincidunt", "ut", "laoreet", "dolore", "magna", "aliquam", "erat"};

        var rand = new Random();
        StringBuilder result = new();

        for (int i = 0; i < wordCount; i++)
        {
            var word = words[rand.Next(words.Length)];
            var punctuation = i == wordCount - 1 ? "." : rand.Next(8) == 2 ? "," : "";

            if (i > 0)
                result.Append($" {word}{punctuation}");
            else
                result.Append($"{word[0].ToString().ToUpper()}{word.AsSpan(1)}");
        }

        return result.ToString();
    }
}
