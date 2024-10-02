using Microsoft.EntityFrameworkCore;

namespace Kori;

public class KoriContext(BlossomContextOptions options) : BlossomContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Content>().ToContainer("content").HasPartitionKey(x => new { x.ContainerUri, x.Language, x.Path });
        builder.Entity<User>().ToContainer("users").HasPartitionKey(x => x.TenantUri);
    }
}
