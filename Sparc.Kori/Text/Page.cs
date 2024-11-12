using Sparc.Blossom.Data;
using Sparc.Kori.Users;
using System.Diagnostics;
using System.Text;


namespace Sparc.Kori;

public record SourceContent(string PageId, string ContentId);
public record UserJoined(string PageId, UserAvatar User) : Notification(PageId);
public record UserLeft(string PageId, UserAvatar User) : Notification(PageId);

public class Page : BlossomEntity<string>
{
    public string PageId { get; private set; }
    public string PageType { get; private set; }
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public UserAvatar HostUser { get; private set; }
    public List<UserAvatar> Users { get; private set; }
    public SourceContent? SourceContent { get; private set; }
    public List<Language> Languages { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? LastActiveDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public AudioContent? Audio { get; private set; }

    private Page()
    {
        Id = Guid.NewGuid().ToString();
        PageId = Id;
        PageType = "Chat";
        Name = "";
        Slug = "";
        SetName("New Page");
        HostUser = new Users.User().Avatar;
        Languages = new();
        StartDate = DateTime.UtcNow;
        LastActiveDate = DateTime.UtcNow;
        Users = new();
    }

    public Page(string name, string type, Users.User hostUser) : this()
    {
        SetName(name);
        PageType = type;
        HostUser = hostUser.Avatar;
    }

    public Page(Page page, Content content) : this()
    {
        // Create a subpage from a message

        SetName(page.Name);
        PageType = page.PageType;
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

    public void AddActiveUser(Users.User user)
    {
        var activeUser = Users.FirstOrDefault(x => x.Id == user.Id);
        if (activeUser == null)
        {
            activeUser = user.Avatar;
            Users.Add(activeUser);
        }

        if (user.PrimaryLanguage != null)
            AddLanguage(user.PrimaryLanguage);
    }

    public void RemoveActiveUser(Users.User user)
    {
        var activeUser = Users.FirstOrDefault(x => x.Id == user.Id);
    }

    internal void InviteUser(UserAvatar user)
    {
        if (!Users.Any(x => x.Id == user.Id))
            Users.Add(user);
    }

    internal async Task<List<Content>> TranslateAsync(Content content, Translator translator, bool forceRetranslation = false)
    {
        var languagesToTranslate = forceRetranslation
            ? Languages.Where(x => x.Id != content.Language).ToList()
            : Languages.Where(x => !content.HasTranslation(x.Id)).ToList();

        if (!languagesToTranslate.Any())
            return new();

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

    internal void SetName(string title)
    {
        Name = title;
        Slug = UrlFriendly(Name);
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
}

