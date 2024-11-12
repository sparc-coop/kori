using Sparc.Blossom.Authentication;
using System.Security.Claims;

namespace Kori;

public record ActivePage(string PageId, DateTime JoinDate);

public class User : BlossomUser
{
    public User()
    {
        Id = Guid.NewGuid().ToString();
        UserId = Id;
        DateCreated = DateTime.UtcNow;
        DateModified = DateTime.UtcNow;
        LanguagesSpoken = [];
        ActivePages = [];
        Avatar = new(Id, "");
        BillingInfo = new();
        ABMode = false;
    }

    public User(string email) : this()
    {
        Email = email;
        Username = email.ToUpper();
        Avatar = new(Id, email);
    }

    public User(string azureId, string email) : this(email)
    {
        AzureB2CId = azureId;
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
    public string? SlackTeamId { get; private set; }
    public string? SlackUserId { get; private set; }
    public string? AzureB2CId { get; private set; }
    public UserBilling BillingInfo { get; private set; }
    public UserAvatar Avatar { get; private set; }
    public List<Language> LanguagesSpoken { get; private set; }
    public List<ActivePage> ActivePages { get; private set; }
    public string? PhoneNumber { get; private set; }
    public bool ABMode { get; set; }

    internal void JoinPage(string pageId)
    {
        if (!ActivePages.Any(x => x.PageId == pageId))
            ActivePages.Add(new(pageId, DateTime.UtcNow));
    }

    internal string? LeavePage(string pageId)
    {
        if (pageId == null) return null;
        ActivePages.RemoveAll(x => x.PageId == pageId);
        return pageId;
    }

    internal void ChangeVoice(Language language, Voice? voice = null)
    {
        var hasLanguageChanged = Avatar.Language != language.Id;

        if (!LanguagesSpoken.Any(x => x.Id == language.Id))
            LanguagesSpoken.Add(language);

        Avatar.Language = language.Id;
        Avatar.LanguageIsRTL = language.IsRightToLeft;
        Avatar.Voice = voice?.ShortName;
        Avatar.Dialect = voice?.Locale;
        Avatar.Gender = voice?.Gender;
    }

    internal Language? PrimaryLanguage => LanguagesSpoken.FirstOrDefault(x => x.Id == Avatar.Language);

    public static User System => new("system");

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
        Avatar.Voice = avatar.Voice;
        Avatar.Language = avatar.Language;
        Avatar.LanguageIsRTL = avatar.LanguageIsRTL;
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

    internal void RegisterWithSlack(string team_id, string user_id)
    {
        SlackTeamId = team_id;
        SlackUserId = user_id;
    }

    protected override void RegisterClaims()
    {
        AddClaim(ClaimTypes.Email, Email);
        AddClaim(ClaimTypes.GivenName, Avatar.Name);
        AddClaim("sub", AzureB2CId);
        AddClaim("Language", Avatar.Language);
    }

    public void ToggleABMode()
    {
        ABMode = !ABMode;
    }
}
