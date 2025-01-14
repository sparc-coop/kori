namespace Sparc.Blossom.Api;
#nullable enable
        
public partial class Page  : BlossomEntityProxy<Page, string>
{
    string _domain = default!;
public string Domain { get => _domain; private set => _set(ref _domain, value); }
string _path = default!;
public string Path { get => _path; private set => _set(ref _path, value); }
string _name = default!;
public string Name { get => _name; private set => _set(ref _name, value); }
SourceContent? _sourceContent;
public SourceContent? SourceContent { get => _sourceContent; private set => _set(ref _sourceContent, value); }
List<Language> _languages = default!;
public List<Language> Languages { get => _languages; private set => _set(ref _languages, value); }
DateTime _startDate = default!;
public DateTime StartDate { get => _startDate; private set => _set(ref _startDate, value); }
DateTime? _lastActiveDate;
public DateTime? LastActiveDate { get => _lastActiveDate; private set => _set(ref _lastActiveDate, value); }
DateTime? _endDate;
public DateTime? EndDate { get => _endDate; private set => _set(ref _endDate, value); }
AudioContent? _audio;
public AudioContent? Audio { get => _audio; private set => _set(ref _audio, value); }

    public async Task AddLanguage(Language language) => await Runner.Execute(Id, "AddLanguage", language);

}