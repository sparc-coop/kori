namespace Sparc.Blossom.Api;
#nullable enable

public record Language(string Id, string? DialectId, string? VoiceId, string DisplayName, string NativeName, bool? IsRightToLeft, List<Dialect> Dialects);