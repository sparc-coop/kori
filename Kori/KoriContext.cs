using Microsoft.EntityFrameworkCore;

namespace Kori;

public class KoriContext(BlossomContextOptions options) : BlossomContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Page>().HasMany(x => x.Contents).WithOne().HasForeignKey(x => new { x.Domain, x.Path });
        builder.Entity<Content>();
    }
}
