namespace Sparc.Blossom.Api;
#nullable enable
        
public partial class KoriUser 
{
    public string UserId { get; set; } = default!;
public string? Email { get; set; }
public DateTime DateCreated { get; set; } = default!;
public DateTime DateModified { get; set; } = default!;
public UserBilling BillingInfo { get; set; } = default!;
public UserAvatar Avatar { get; set; } = default!;
public List<Language> LanguagesSpoken { get; set; } = default!;
public string? PhoneNumber { get; set; }
public bool ABMode { get; set; } = default!;
public KoriUser System { get; set; } = default!;

    
}