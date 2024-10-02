namespace Kori;

public abstract class BillingAccount(string tenantUri, string type, string externalId, string currency)
{
    public string TenantUri { get; private set; } = tenantUri;
    public string Type { get; private set; } = type;
    public string ExternalId { get; protected set; } = externalId;
    public string Currency { get; private set; } = currency;
    public decimal Balance { get; private set; }

    public ICollection<BillingCharge> Charges { get; private set; } = [];

    internal abstract Task RegisterAsync(User user);

    internal void AddCharge(BillingCharge charge)
    {
        Charges.Add(charge);
        Balance += charge.Amount;
    }
}
