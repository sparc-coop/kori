namespace Kori;

public record Word(long Offset, long Duration, string Text);
public record Audio(string Url, string Voice, long Duration)
{
    List<Word> Subtitles = [];
}
