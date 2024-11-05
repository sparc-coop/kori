using Microsoft.EntityFrameworkCore;
using Sparc.Kori.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparc.Kori._Plugins
{
    public class KoriContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Page> Pages => Set<Page>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Page>().ToContainer("Pages").HasPartitionKey(x => x.PageId);
        }
    }
}
