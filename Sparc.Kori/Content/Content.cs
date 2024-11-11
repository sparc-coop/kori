using Microsoft.Azure.Cosmos;
using Sparc.Blossom.Data;
using Sparc.Kori.Users;
using Sparc.Kori.Page;

namespace Sparc.Kori.Content;

public record Word(long Offset, long Duration, string Text);
public record EditHistory(DateTime Timestamp, string Text);
public record AudioContent(string? Url, long Duration, string Voice, List<Word>? Subtitles = null);
public record ContentTag(string Key, string Value, bool Translate);
public record ContentTranslation(string Id, string LanguageId, string SourceMessageId);

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
        User = new Users.User().Avatar;
        Language = string.Empty;
        Translations = new();
        EditHistory = new();
        Tags = new();
        ContentType = "Text";
    }

    public Content(string pageId, string pageName, Users.User user, string text, string? tag = null, string? language = null, string contentType = "Text") : this()
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

    public Content(Content sourceContent, Page.Language toLanguage, string text, List<ContentTag> translatedTags) : this()
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
        SetTags(sourceContent.Tags);
        SetTags(translatedTags, false);
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

        Broadcast(new ContentTextChanged(this));
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
            Broadcast(new ContentAudioChanged(this));
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

    internal void SetTags(List<ContentTag> tags, bool fullReplace = true)
    {
        var keys = tags.Select(x => x.Key).ToList();
        if (fullReplace)
            Tags.RemoveAll(x => !keys.Contains(x.Key));

        foreach (var tag in tags)
        {
            var existing = Tags.FirstOrDefault(x => x.Key == tag.Key);
            if (existing != null)
                existing.Value = tag.Value;
            else
                Tags.Add(new(tag.Key, tag.Value, SourceContentId == null && tag.Translate));
        }

        if (tags.Any(x => x.Translate))
            Broadcast(new ContentTextChanged(this));
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
        Broadcast(new ContentDeleted(this));
    }

    public void SetHtmlFromMarkdown()
    {
        Html = MarkdownExtensions.ToHtml(Text ?? string.Empty);
    }
}
