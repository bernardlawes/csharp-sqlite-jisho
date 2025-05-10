using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharp_sqlite_jisho.Models
{
    public class Word
    {
        public int Id { get; set; }
        public string Kanji { get; set; }
        public string Reading { get; set; }
        public string Meaning { get; set; }
        public int? JLPTLevel { get; set; }
        public int? GradeLevel { get; set; }
        public string Type { get; set; } // "kanji", "word", etc.
    }
}
