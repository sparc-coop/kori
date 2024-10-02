using Sparc.Blossom.Authentication;

namespace Kori;

public class User(string tenantUri) : BlossomUser()
{
    public string TenantUri { get; set; } = tenantUri;
    public string? Name { get; private set; }
    public string Initials => string.IsNullOrWhiteSpace(Name) ? "" : string.Join(string.Empty, Name.Split(' ').Select(x => x[0]));
    public bool IsOnline { get; private set; }
    public AvatarColor? Color { get; private set; }
    public Language? PrimaryLanguage { get; private set; }
    public List<Language> Languages { get; private set; } = [];
    public string? Emoji { get; private set; }
    public string? SkinTone { get; private set; }
    public string? Pronouns { get; private set; }
    public string? Description { get; private set; }
    public Voice? Voice { get; private set; }
    public Dialect? Dialect { get; private set; }
    public string? Gender { get; private set; }
    public bool? HearOthers { get; private set; }
    public bool? MuteMe { get; private set; }
}
