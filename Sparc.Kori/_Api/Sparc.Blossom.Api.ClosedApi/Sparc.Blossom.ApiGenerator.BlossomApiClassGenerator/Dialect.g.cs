namespace Sparc.Blossom.Api;
#nullable disable
        
public partial class Dialect 
{
    public string Language { get; set; } = default!;
public string Locale { get; set; } = default!;
public string DisplayName { get; set; } = default!;
public string NativeName { get; set; } = default!;
public List<Voice> Voices { get; set; } = default!;

    
}