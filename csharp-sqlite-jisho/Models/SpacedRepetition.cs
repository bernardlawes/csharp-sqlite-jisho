using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharp_sqlite_jisho.Models
{
    public class SpacedRepetition
    {
        public int Id { get; set; }
        public int WordId { get; set; }
        public DateTime? LastReviewedAt { get; set; }
        public DateTime? NextReviewDue { get; set; }
        public double EaseFactor { get; set; } = 2.5;
        public int IntervalDays { get; set; } = 1;
        public int RepetitionCount { get; set; } = 0;
    }
}
