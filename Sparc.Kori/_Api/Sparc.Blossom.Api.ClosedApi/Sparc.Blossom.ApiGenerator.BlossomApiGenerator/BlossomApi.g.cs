namespace Sparc.Blossom.Api;
public class BlossomApi(Contents contents, Pages pages) : BlossomApiProxy
{
    public Contents Contents { get; } = contents;
public Pages Pages { get; } = pages;

}