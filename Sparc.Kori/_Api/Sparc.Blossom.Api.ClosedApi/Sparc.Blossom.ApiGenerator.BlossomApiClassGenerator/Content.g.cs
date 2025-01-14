namespace Sparc.Blossom.Api;
#nullable enable
        
public partial class Content  : BlossomEntityProxy<Content, string>
{
    string _domain = default!;
public string Domain { get => _domain; private set => _set(ref _domain, value); }
string _path = default!;
public string Path { get => _path; private set => _set(ref _path, value); }
string? _sourceContentId;
public string? SourceContentId { get => _sourceContentId; private set => _set(ref _sourceContentId, value); }
Language _language = default!;
public Language Language { get => _language; protected set => _set(ref _language, value); }
string _contentType = default!;
public string ContentType { get => _contentType; private set => _set(ref _contentType, value); }
DateTime _timestamp = default!;
public DateTime Timestamp { get => _timestamp; private set => _set(ref _timestamp, value); }
DateTime? _lastModified;
public DateTime? LastModified { get => _lastModified; private set => _set(ref _lastModified, value); }
DateTime? _deletedDate;
public DateTime? DeletedDate { get => _deletedDate; private set => _set(ref _deletedDate, value); }
UserAvatar? _user;
public UserAvatar? User { get => _user; private set => _set(ref _user, value); }
AudioContent? _audio;
public AudioContent? Audio { get => _audio; private set => _set(ref _audio, value); }
string? _text;
public string? Text { get => _text; private set => _set(ref _text, value); }
List<ContentTranslation> _translations = default!;
public List<ContentTranslation> Translations { get => _translations; private set => _set(ref _translations, value); }
string _originalText = default!;
public string OriginalText { get => _originalText;  set => _set(ref _originalText, value); }
string _html = default!;
public string Html { get => _html;  set => _set(ref _html, value); }
string _pageId = default!;
public string PageId { get => _pageId; internal set => _set(ref _pageId, value); }

    public async Task SetHtmlFromMarkdown() => await Runner.Execute(Id, "SetHtmlFromMarkdown");
public async Task SetTextAndHtml(string text) => await Runner.Execute(Id, "SetTextAndHtml", text);

}