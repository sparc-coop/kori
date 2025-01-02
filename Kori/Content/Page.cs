using System.Diagnostics;
using System.Text;

namespace Kori;

public record SourceContent(string PageId, string ContentId);
public record TranslateContentRequest(Dictionary<string,string> ContentDictionary, bool AsHtml, string LanguageId);
public record TranslateContentResponse(string Domain, string Path, string Id, string Language, Dictionary<string, Content> Content);

public class Page : BlossomEntity<string>
{
    public string Domain { get; private set; }
    public string Path { get; private set; }
    public string Name { get; private set; }
    public SourceContent? SourceContent { get; private set; }
    public List<Language> Languages { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? LastActiveDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public AudioContent? Audio { get; private set; }
    public ICollection<Content> Contents { get; private set; } = [];

    private Page(string domain, string path)
    {
        Id = new Uri(new Uri(domain), path).ToString();
        Domain = domain;
        Path = path;
        Name = "New Page";
        Languages = [];
        StartDate = DateTime.UtcNow;
        LastActiveDate = DateTime.UtcNow;
    }

    public Page(string domain, string path, string name) : this(domain, path)
    {
        Name = name;
    }

    public Page(Uri uri, string name) : this(uri.Host, uri.AbsolutePath, name)
    {
    }

    internal Page(Page page, Content content) : this(page.Domain, page.Path, page.Name)
    {
        // Create a subpage from a message

        SourceContent = new(page.Id, content.Id);
        //Languages = page.Languages;
        //ActiveUsers = page.ActiveUsers;
        //Translations = page.Translations;
    }

    public void AddLanguage(Language language)
    {
        if (Languages.Any(x => x.Id == language.Id))
            return;

        Languages.Add(language);
    }

    public void TranslateContent(TranslateContentRequest request)
    {
        var newContentList = new List<Content>();
        
        if (request.ContentDictionary == null || request.ContentDictionary.Count == 0)
        {
            return;
        }

        foreach (var kvp in request.ContentDictionary) {
            var tag = kvp.Key?.Trim();
            var text = kvp.Value?.Trim();

            if (!string.IsNullOrEmpty(tag) || !string.IsNullOrEmpty(text))
            {
                if (Contents.Any(c => c.Tag == tag && c.Language == request.LanguageId)) {
                    continue;
                }

                var newContentEntry = new Content(Id, request.LanguageId, text, tag: tag);

                newContentEntry.SetTextAndHtml(new SetTextAndHtmlRequest(text));

                Contents.Add(newContentEntry);
            }
        }
    }

    public Dictionary<string,Content> GetAllContentAsDictionary()
    {
        var content = Contents.OrderBy(y => y.Timestamp);

        var defaultContentDictionary = content.ToDictionary(
            message => message.Tag!,
            message => message
        );

        return defaultContentDictionary;
    }

    internal async Task<List<Content>> TranslateAsync(Content content, KoriTranslator translator, bool forceRetranslation = false)
    {
        var languagesToTranslate = forceRetranslation
            ? Languages.Where(x => x.Id != content.Language).ToList()
            : Languages.Where(x => !content.HasTranslation(x.Id)).ToList();

        if (!languagesToTranslate.Any())
            return [];

        try
        {
            var translatedContents = await translator.TranslateAsync(content, languagesToTranslate);

            // Add reference to all the new translated contents
            foreach (var translatedContent in translatedContents)
                content.AddTranslation(translatedContent);

            return translatedContents;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            throw;
        }
    }

    internal async Task SpeakAsync(ISpeaker speaker, List<Content> contents)
    {
        Audio = await speaker.SpeakAsync(contents);
    }

    internal void Close()
    {
        EndDate = DateTime.UtcNow;
    }

    // Adopted from https://stackoverflow.com/a/25486
    static string UrlFriendly(string title)
    {
        if (title == null) return "";

        const int maxlen = 80;
        int len = title.Length;
        bool prevdash = false;
        var sb = new StringBuilder(len);
        char c;

        for (int i = 0; i < len; i++)
        {
            c = title[i];
            if (c >= 'a' && c <= 'z' || c >= '0' && c <= '9')
            {
                sb.Append(c);
                prevdash = false;
            }
            else if (c >= 'A' && c <= 'Z')
            {
                // tricky way to convert to lowercase
                sb.Append((char)(c | 32));
                prevdash = false;
            }
            else if (c == ' ' || c == ',' || c == '.' || c == '/' ||
                c == '\\' || c == '-' || c == '_' || c == '=')
            {
                if (!prevdash && sb.Length > 0)
                {
                    sb.Append('-');
                    prevdash = true;
                }
            }
            else if (c >= 128)
            {
                int prevlen = sb.Length;
                sb.Append(RemapInternationalCharToAscii(c));
                if (prevlen != sb.Length) prevdash = false;
            }
            if (i == maxlen) break;
        }

        if (prevdash)
            return sb.ToString()[..(sb.Length - 1)];
        else
            return sb.ToString();
    }

    public static string RemapInternationalCharToAscii(char c)
    {
        string s = c.ToString().ToLowerInvariant();
        if ("àåáâäãåą".Contains(s))
        {
            return "a";
        }
        else if ("èéêëę".Contains(s))
        {
            return "e";
        }
        else if ("ìíîïı".Contains(s))
        {
            return "i";
        }
        else if ("òóôõöøőð".Contains(s))
        {
            return "o";
        }
        else if ("ùúûüŭů".Contains(s))
        {
            return "u";
        }
        else if ("çćčĉ".Contains(s))
        {
            return "c";
        }
        else if ("żźž".Contains(s))
        {
            return "z";
        }
        else if ("śşšŝ".Contains(s))
        {
            return "s";
        }
        else if ("ñń".Contains(s))
        {
            return "n";
        }
        else if ("ýÿ".Contains(s))
        {
            return "y";
        }
        else if ("ğĝ".Contains(s))
        {
            return "g";
        }
        else if (c == 'ř')
        {
            return "r";
        }
        else if (c == 'ł')
        {
            return "l";
        }
        else if (c == 'đ')
        {
            return "d";
        }
        else if (c == 'ß')
        {
            return "ss";
        }
        else if (c == 'Þ')
        {
            return "th";
        }
        else if (c == 'ĥ')
        {
            return "h";
        }
        else if (c == 'ĵ')
        {
            return "j";
        }
        else
        {
            return "";
        }
    }

    internal void UpdateName(string name) => Name = name;
}

