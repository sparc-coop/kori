using System.Globalization;

namespace Kori;

public class Dialect(string localeName)
{
    public string Language { get; private set; } = localeName.Split('-').First();
    public string Locale { get; private set; } = localeName.Split('-').Last();
    public string DisplayName { get; private set; } = CultureInfo.GetCultureInfo(localeName).DisplayName;
    public string NativeName { get; private set; } = CultureInfo.GetCultureInfo(localeName).NativeName;
    public List<Voice> Voices { get; private set; } = [];

    public void AddVoice(Voice voice)
    {
        var existing = Voices.FindIndex(x => x.ShortName == voice.ShortName);

        if (existing == -1)
            Voices.Add(voice);
        else
            Voices[existing] = voice;
    }
}