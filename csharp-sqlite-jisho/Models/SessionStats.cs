using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// represents one quiz session (start/end time, totals, accuracy).
namespace csharp_sqlite_jisho.Models
{
    public class SessionStat
    {
        public int Id { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime EndedAt { get; set; }
        public int TotalQuestions { get; set; }
        public int TotalCorrect { get; set; }
        public int TotalIncorrect { get; set; }
        public int? CollectionId { get; set; } // nullable
        public string? CollectionName { get; set; }
        public double Accuracy => TotalQuestions > 0
            ? (double)TotalCorrect / TotalQuestions * 100
            : 0;
    }
}
