using Sparc.Blossom.Authentication;
using System.Security.Claims;

namespace Kori;

public record ActivePage(string PageId, DateTime JoinDate);

public class KoriUser : BlossomUser
{
    public KoriUser()
    {
        Id = Guid.NewGuid().ToString();
        UserId = Id;
        DateCreated = DateTime.UtcNow;
        DateModified = DateTime.UtcNow;
        LanguagesSpoken = [];
        Avatar = new(Id, "");
        BillingInfo = new();
        ABMode = false;
    }

    public KoriUser(string email) : this()
    {
        Email = email;
        Username = email.ToUpper();
        Avatar = new(Id, email);
    }

    public string UserId { get { return Id; } set { Id = value; } }
    private string? _email;
    public string? Email
    {
        get { return _email; }
        private set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _email = null;
                return;
            }

            _email = value.Trim().ToLower();
        }
    }

    public DateTime DateCreated { get; private set; }
    public DateTime DateModified { get; private set; }
    public UserBilling BillingInfo { get; private set; }
    public UserAvatar Avatar { get; private set; }
    public List<Language> LanguagesSpoken { get; private set; }
    public string? PhoneNumber { get; private set; }
    public bool ABMode { get; set; }

    internal void ChangeVoice(Language language, Voice? voice = null)
    {
        var hasLanguageChanged = Avatar.Language != language;

        if (!LanguagesSpoken.Any(x => x.Id == language.Id))
            LanguagesSpoken.Add(language);

        Avatar.Language = language with { DialectId = voice?.Locale, VoiceId = voice?.ShortName };
        Avatar.Gender = voice?.Gender;
    }

    internal Language? PrimaryLanguage => LanguagesSpoken.FirstOrDefault(x => x == Avatar.Language);

    public static KoriUser System => new("system");

    internal void Refill(long ticksToAdd)
    {
        BillingInfo.TicksBalance += ticksToAdd;
    }

    //internal void AddCharge(CostIncurred costIncurred)
    //{
    //    BillingInfo.TicksBalance -= costIncurred.Ticks;
    //    Broadcast(new BalanceChanged(Id, BillingInfo.TicksBalance));
    //}

    internal void UpdateAvatar(UserAvatar avatar)
    {
        Avatar.Id = Id;
        Avatar.Language = avatar.Language;
        Avatar.BackgroundColor = avatar.BackgroundColor;
        Avatar.Pronouns = avatar.Pronouns;
        Avatar.Name = avatar.Name;
        Avatar.Description = avatar.Description;
        Avatar.SkinTone = avatar.SkinTone;
        Avatar.Emoji = avatar.Emoji;
        Avatar.HearOthers = avatar.HearOthers;
        Avatar.MuteMe = avatar.MuteMe;
    }

    internal void SetUpBilling(string customerId, string currency)
    {
        BillingInfo.SetUpCustomer(customerId, currency);
    }

    internal void GoOnline(string connectionId)
    {
        Avatar.IsOnline = true;
    }

    internal void GoOffline()
    {
        Avatar.IsOnline = false;
    }

    protected override void RegisterClaims()
    {
        AddClaim(ClaimTypes.Email, Email);
        AddClaim(ClaimTypes.GivenName, Avatar.Name);
        if (Avatar.Language != null)
            AddClaim(ClaimTypes.Locality, Avatar.Language.Id);
    }

    public void ToggleABMode()
    {
        ABMode = !ABMode;
    }
}
