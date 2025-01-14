using Microsoft.Extensions.Localization;

namespace Sparc.Kori;

public class KoriLocalizedString : LocalizedString
{
    internal KoriLocalizedString(Blossom.Api.Content content) : base(content.OriginalText, content.Text ?? content.OriginalText, content.Text == null)
    {
        Language = content.Language.Id;
    }

    public KoriLocalizedString(string name) : base(name, name, true) { }

    public string? Language { get; }
}
