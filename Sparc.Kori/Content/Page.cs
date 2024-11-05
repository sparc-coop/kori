using Sparc.Blossom.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparc.Kori.Content
{
    public class Page : BlossomEntity<string>
    {
        public string PageId { get; private set; }
        public string PageType { get; private set; }
        public string Name { get; private set; }
        public string Slug { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime? LastActiveDate { get; private set; }
        public DateTime? EndDate { get; private set; }
    }
}
