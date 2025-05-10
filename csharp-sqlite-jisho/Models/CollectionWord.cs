using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharp_sqlite_jisho.Models
{
    public class CollectionWord
    {
        public int Id { get; set; }
        public int CollectionId { get; set; }
        public int WordId { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
