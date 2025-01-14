namespace Sparc.Blossom.Api;
#nullable enable
        
public partial class UserAvatar 
{
    public string Id { get; set; } = default!;
public string Name { get; set; } = default!;
public string Initials { get; set; } = default!;
public bool IsOnline { get; set; } = default!;
public string BackgroundColor { get; set; } = default!;
public string ForegroundColor { get; set; } = default!;
public Language? Language { get; set; }
public string? Emoji { get; set; }
public string? SkinTone { get; set; }
public string? Pronouns { get; set; }
public string? Description { get; set; }
public string? Gender { get; set; }
public bool? HearOthers { get; set; }
public bool? MuteMe { get; set; }

    
}