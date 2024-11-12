namespace Kori;

public interface ISpeaker
{
    Task<AudioContent?> SpeakAsync(Content message, string? voiceId = null);
    Task<List<Voice>> GetVoicesAsync(string? language = null, string? dialect = null, string? gender = null);
    Task<AudioContent> SpeakAsync(List<Content> messages);
    Task<string?> GetClosestVoiceAsync(string language, string? gender, string deterministicId);
}