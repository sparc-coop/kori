using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparc.Kori.Users;

public class UserBilling
{
    public UserBilling()
    {
        Currency = "USD";
        TicksBalance = TimeSpan.FromMinutes(10).Ticks; // Initial free minutes
    }

    public void SetUpCustomer(string customerId, string currency)
    {
        CustomerId = customerId;
        Currency = currency;
    }

    public string? CustomerId { get; set; }

    public long TicksBalance { get; set; }
    public string Currency { get; set; }
}
