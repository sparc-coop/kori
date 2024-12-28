using Microsoft.Extensions.Localization;

namespace Sparc.Kori;

public class KoriLocalizedString : LocalizedString
{
    public KoriLocalizedString(string name, string value) : base(name, value)
    {
    }
}
