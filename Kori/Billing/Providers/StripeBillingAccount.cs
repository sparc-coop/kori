using Stripe;

namespace Kori;

public class StripeBillingAccount(string tenantUri, string type, string externalId, string currency) 
    : BillingAccount(tenantUri, type, externalId, currency)
{
    public async Task<BillingCharge> CreatePaymentAsync(decimal amount, string? paymentIntentId = null)
    {
        var options = new PaymentIntentListOptions
        {
            Customer = ExternalId
        };

        var paymentIntentService = new PaymentIntentService();
        StripeList<PaymentIntent> paymentIntents = paymentIntentService.List(options);
        
        var stripeAmount = StripeAmount(amount, Currency);
        var paymentIntent = paymentIntentId != null
            ? await paymentIntentService.UpdateAsync(paymentIntentId, new()
            {
                Amount = stripeAmount
            })
            : paymentIntents.Any(x => x.Status != "succeeded")
            ? await paymentIntentService.UpdateAsync(paymentIntents.First(x => x.Status != "succeeded").Id, new()
            {
                Amount = stripeAmount
            })
            : await paymentIntentService.CreateAsync(new()
            {
                Amount = stripeAmount,
                Customer = ExternalId,
                Currency = Currency.ToLower(),
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
            });

        var charge = new BillingCharge(TenantUri, paymentIntent.Description, paymentIntent.Currency, LocalAmount(paymentIntent));
        charge.SetExternalInfo(paymentIntent.Id);
        return charge;
    }

    public async Task<bool> ProcessAsync(string eventJson, string signatureHeader, string endpointKey)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(eventJson, signatureHeader, endpointKey, throwOnApiVersionMismatch: false);

            if (stripeEvent.Type == "payment_intent.succeeded")
            {
                Refill(stripeEvent);
            }
            else if (stripeEvent.Type == "payment_intent.payment_failed")
            {
                await FailAsync(stripeEvent);
            }

            return true;
        }
        catch (StripeException)
        {
            return false;
        }
        catch (Exception)
        {
            throw;
        }
    }

    internal async override Task RegisterAsync(User user)
    {
        var customer = await new CustomerService().CreateAsync(new()
        {
            //Email = user.Email,
            Name = user.Name,
            Metadata = new() { { "IbisUserId", user.Id } }
        });

        ExternalId = customer.Id;
    }

    internal void Refill(Event stripeEvent)
    {
        var intent = stripeEvent.Data.Object as PaymentIntent;
        if (intent?.CustomerId != ExternalId)
            throw new InvalidOperationException("PaymentIntent does not belong to this account.");
        
        var charge = new BillingCharge(TenantUri, intent.Description, intent.Currency.ToUpper(), LocalAmount(intent));
        charge.SetExternalInfo(intent.Id, intent.ToJson());
        AddCharge(charge);
    }

    private Task FailAsync(Event stripeEvent)
    {
        return Task.CompletedTask;
    }

    static long StripeAmount(decimal amount, string currency)
    {
        return (long)(amount * StripeCurrencyMultiplier(currency));
    }

    static decimal LocalAmount(PaymentIntent paymentIntent)
    {
        return paymentIntent.Amount / StripeCurrencyMultiplier(paymentIntent.Currency);
    }

    static readonly List<string> ZeroDecimalCurrencies = ["bif", "clp", "djf", "gnf", "jpy", "kmf", "krw", "mga", "pyg", "rwf", "ugx", "vnd", "vuv", "xaf", "xof", "xpf"];
    static decimal StripeCurrencyMultiplier(string currency) => ZeroDecimalCurrencies.Contains(currency.ToLower()) ? 1 : 100;
}
