using Sparc.Blossom.Data;

namespace Sparc.Kori;

public record EditHistory(DateTime Timestamp, string Text);
public record AudioContent(string? Url, long Duration, string Voice, List<Word>? Subtitles = null);
public record ContentTag(string Key, string Value, bool Translate);
public record ContentTranslation(string Id, string LanguageId, string? SourceContentId = null);

public class Content : BlossomEntity<string>
{
    public string PageId { get; private set; }
    public string PageName { get; private set; }
    public string? SourceContentId { get; private set; }
    public string Language { get; protected set; }
    public string ContentType { get; private set; }
    public bool? LanguageIsRTL { get; protected set; }
    public DateTime Timestamp { get; private set; }
    public DateTime? LastModified { get; private set; }
    public DateTime? DeletedDate { get; private set; }
    public UserAvatar User { get; private set; }
    public AudioContent? Audio { get; private set; }
    public string? Text { get; private set; }
    public List<ContentTranslation> Translations { get; private set; }
    public long Charge { get; private set; }
    public decimal Cost { get; private set; }
    public string? Tag { get; set; }
    public List<ContentTag> Tags { get; set; }
    public List<EditHistory> EditHistory { get; private set; }
    public string Html { get; set; }

    protected Content()
    {
        Id = Guid.NewGuid().ToString();
        PageId = string.Empty;
        User = new User().Avatar;
        Language = string.Empty;
        Translations = new();
        EditHistory = new();
        Tags = new();
        ContentType = "Text";
    }

    public Content(string pageId, string pageName, User user, string text, string? tag = null, string? language = null, string contentType = "Text") : this()
    {
        PageId = pageId;
        PageName = pageName;
        User = user.Avatar;
        Language = user.Avatar.Language ?? language ?? string.Empty;
        LanguageIsRTL = user.Avatar.LanguageIsRTL;
        Audio = user.Avatar.Voice == null ? null : new(null, 0, user.Avatar.Voice);
        Timestamp = DateTime.UtcNow;
        Tag = tag;
        ContentType = contentType;
        SetText(text);
        SetHtmlFromMarkdown();
    }

    public Content(Content sourceContent, Language toLanguage, string text) : this()
    {
        PageId = sourceContent.PageId;
        PageName = sourceContent.PageName;
        SourceContentId = sourceContent.Id;
        User = new(sourceContent.User);
        Audio = sourceContent.Audio?.Voice == null ? null : new(null, 0, new(sourceContent.Audio.Voice));
        Language = toLanguage.Id;
        LanguageIsRTL = toLanguage.IsRightToLeft;
        Timestamp = sourceContent.Timestamp;
        Tag = sourceContent.Tag;
        SetText(text);
        SetHtmlFromMarkdown();
    }

    public void SetText(string text)
    {
        if (Text == text)
            return;

        if (Text != null)
            EditHistory.Add(new(LastModified ?? Timestamp, Text));

        Text = text;
        LastModified = DateTime.UtcNow;
    }

    internal async Task<(string?, Content?)> TranslateAsync(Translator translator, string languageId)
    {
        if (HasTranslation(languageId))
            return (Translations.First(x => x.LanguageId == languageId).SourceContentId, null);

        var language = await translator.GetLanguageAsync(languageId);
        var translatedContent = (await translator.TranslateAsync(this, new List<Language> { language! })).FirstOrDefault();

        if (translatedContent != null)
            AddTranslation(translatedContent);

        return (translatedContent?.Id, translatedContent);
    }

    internal async Task<AudioContent?> SpeakAsync(ISpeaker engine, string? voiceId = null)
    {
        if (voiceId == null && (Audio?.Voice == null || !Audio.Voice.StartsWith(Language)))
        {
            voiceId = await engine.GetClosestVoiceAsync(Language, User.Gender, User.Id);
        }

        var audio = await engine.SpeakAsync(this, voiceId);

        if (audio != null)
        {
            Audio = audio;
        }

        return audio;
    }

    internal bool HasTranslation(string languageId)
    {
        return Language == languageId
            || Translations != null && Translations.Any(x => x.LanguageId == languageId);
    }

    internal void AddTranslation(Content translatedContent)
    {
        if (HasTranslation(translatedContent.Language))
        {
            // Set the newly translated content's ID to the existing translation so that it is updated in the repository
            var translation = Translations.FirstOrDefault(x => x.LanguageId == translatedContent.Language);
            if (translation != null)
                translatedContent.Id = translation.SourceContentId;
        }
        else
        {
            Translations.Add(new(translatedContent.Language, translatedContent.Id));
        }
    }

    internal void AddCharge(long ticks, decimal cost, string description)
    {
        Charge += ticks;
        Cost -= cost;
        //if (ticks > 0)
        //Broadcast(new CostIncurred(this, description, ticks));
    }

    internal void Delete()
    {
        DeletedDate = DateTime.UtcNow;
    }

    public void SetHtmlFromMarkdown()
    {
        Html = MarkdownExtensions.ToHtml(Text ?? string.Empty);
    }
}
