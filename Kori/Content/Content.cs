namespace Kori;

public class Content(string containerUri, string language, string path, string id, string type) 
    : BlossomEntity<string>(id)
{
    public Content(Content sourceContent, Language language, string translatedText)
        : this(sourceContent.ContainerUri, language.Id, sourceContent.Path, sourceContent.Id, sourceContent.Type)
    {
        Text = translatedText;
        Author = sourceContent.Author;
    }


    public string ContainerUri { get; private set; } = containerUri;
    public string Language { get; private set; } = language;
    public string Path { get; private set; } = path;
    // BlossomEntity.Id is the tag for the content
    public string Type { get; private set; } = type;
    public string? Text { get; private set; }
    public Audio? Audio { get; private set; }
    public string? Html { get; private set; }
    public User? Author { get; private set; }
    internal DateTime Timestamp { get; private set; } = DateTime.UtcNow;
}
