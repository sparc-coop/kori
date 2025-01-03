using Microsoft.Extensions.Localization;

namespace Sparc.Kori;

public class KoriLocalizedString : LocalizedString
{
    public KoriLocalizedString(KoriTextContent content) : base(content.OriginalText, content.Text ?? content.OriginalText, content.Text == null)
    {
        Language = content.Language;
    }

    public KoriLocalizedString(string name) : base(name, name, true) { }

    public string? Language { get; }
}
