using Microsoft.EntityFrameworkCore;

namespace Kori;

public class KoriContext(DbContextOptions options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Page>().HasKey(x => new { x.Domain, x.Path });
        builder.Entity<Page>().HasPartitionKey(x => new { x.Domain, x.Path }).ToContainer("content");
        builder.Entity<Content>().HasPartitionKey(x => new { x.Domain, x.Path }).ToContainer("content");
    }
}
