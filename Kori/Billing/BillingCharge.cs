using System.Globalization;

namespace Kori;

public class BillingCharge(string tenantUri, string description, string currency, decimal amount, string? uri = null, decimal? cost = null)
    : BlossomEntity<string>
{
    public string TenantUri { get; } = tenantUri;
    public string? Uri { get; set; } = uri;
    public string? ExternalId { get; private set; }
    public string Currency { get; } = currency;
    public decimal Amount { get; } = amount;
    public decimal? Cost { get; } = cost;
    public string Description { get; } = description;

    public string? Metadata { get; private set; }

    internal void SetExternalInfo(string id, string? data = null)
    {
        ExternalId = id;
        Metadata = data;
    }

    public override string ToString()
    {
        TryGetCurrencySymbol(Currency, out string? symbol);
        return $"{symbol ?? (Currency.ToUpper() + " ")}{Amount}";
    }

    internal static bool TryGetCurrencySymbol(string ISOCurrencySymbol, out string? symbol)
    {
        ISOCurrencySymbol = ISOCurrencySymbol.ToUpper();

        symbol = CultureInfo
            .GetCultures(CultureTypes.AllCultures)
            .Where(c => !c.IsNeutralCulture)
            .Select(culture =>
            {
                try
                {
                    return new RegionInfo(culture.Name);
                }
                catch
                {
                    return null;
                }
            })
            .Where(ri => ri != null && ri.ISOCurrencySymbol == ISOCurrencySymbol)
            .Select(ri => ri!.CurrencySymbol)
            .FirstOrDefault();
        return symbol != null;
    }
}
