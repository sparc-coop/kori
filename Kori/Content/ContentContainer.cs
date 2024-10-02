namespace Kori;

public class ContentContainer(string uri, string type, string createdByUserId) : BlossomEntity<string>
{
    public string Uri { get; private set; } = uri;
    public string Type { get; private set; } = type;
    public string? Name { get; private set; }
    public string CreatedByUserId { get; private set; } = createdByUserId;
    public List<string> AdminUsers { get; private set; } = [];
    public List<Language> Languages { get; private set; } = [];
}
