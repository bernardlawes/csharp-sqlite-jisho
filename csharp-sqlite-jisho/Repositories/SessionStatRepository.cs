using System.IO;
using Microsoft.Data.Sqlite;
using csharp_sqlite_jisho.Models;
using csharp_sqlite_jisho.Database;

namespace csharp_sqlite_jisho.Repositories
{
    public class SessionStatRepository
    {
        public void InsertSessionStat(SessionStat stat)
        {
            using var connection = SQLiteConnectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO SessionStats (started_at, ended_at, total_questions, total_correct, total_incorrect, collection_id)
                VALUES (@startedAt, @endedAt, @totalQuestions, @totalCorrect, @totalIncorrect, @collectionId);
            ";
            command.Parameters.AddWithValue("@startedAt", stat.StartedAt.ToString("o"));
            command.Parameters.AddWithValue("@endedAt", stat.EndedAt.ToString("o"));
            command.Parameters.AddWithValue("@totalQuestions", stat.TotalQuestions);
            command.Parameters.AddWithValue("@totalCorrect", stat.TotalCorrect);
            command.Parameters.AddWithValue("@totalIncorrect", stat.TotalIncorrect);
            command.Parameters.AddWithValue("@collectionId", (object?)stat.CollectionId ?? DBNull.Value);

            command.ExecuteNonQuery();
        }

        public List<SessionStat> GetAllSessionStatsWithCollection()
        {
            var sessions = new List<SessionStat>();
            using var connection = SQLiteConnectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT 
                    s.id, s.started_at, s.ended_at, s.total_questions, s.total_correct, s.total_incorrect,
                    s.collection_id,
                    c.name AS collection_name
                FROM SessionStats s
                LEFT JOIN Collections c ON s.collection_id = c.id
                ORDER BY s.started_at DESC;
            ";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                sessions.Add(new SessionStat
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    StartedAt = DateTime.Parse(reader["started_at"].ToString()),
                    EndedAt = DateTime.Parse(reader["ended_at"].ToString()),
                    TotalQuestions = reader.GetInt32(reader.GetOrdinal("total_questions")),
                    TotalCorrect = reader.GetInt32(reader.GetOrdinal("total_correct")),
                    TotalIncorrect = reader.GetInt32(reader.GetOrdinal("total_incorrect")),
                    CollectionId = reader["collection_id"] as int?,
                    CollectionName = reader["collection_name"]?.ToString()
                });
            }
            return sessions;
        }


        public List<SessionStat> GetAllSessionStats()
        {
            var sessions = new List<SessionStat>();
            using var connection = SQLiteConnectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT id, started_at, ended_at, total_questions, total_correct, total_incorrect
                FROM SessionStats
                ORDER BY started_at DESC;
            ";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                sessions.Add(new SessionStat
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    StartedAt = DateTime.Parse(reader["started_at"].ToString()),
                    EndedAt = DateTime.Parse(reader["ended_at"].ToString()),
                    TotalQuestions = reader.GetInt32(reader.GetOrdinal("total_questions")),
                    TotalCorrect = reader.GetInt32(reader.GetOrdinal("total_correct")),
                    TotalIncorrect = reader.GetInt32(reader.GetOrdinal("total_incorrect")),
                });
            }
            return sessions;
        }

        public void ExportSessionStatsToCsv(string filePath)
        {
            //var sessions = GetAllSessionStats();

            var sessions = GetAllSessionStatsWithCollection();

            using var writer = new StreamWriter(filePath);

            // Includes Collection Name
            writer.WriteLine("Id,StartedAt,EndedAt,TotalQuestions,TotalCorrect,TotalIncorrect,Accuracy,CollectionName");

            // Basic / No Collection Name
            //writer.WriteLine("Id,StartedAt,EndedAt,TotalQuestions,TotalCorrect,TotalIncorrect,Accuracy");

            foreach (var session in sessions)
            {
                var safeCollectionName = session.CollectionName?.Replace(",", ";") ?? "";
                // Includes Collection Name
                writer.WriteLine($"{session.Id},{session.StartedAt:o},{session.EndedAt:o},{session.TotalQuestions},{session.TotalCorrect},{session.TotalIncorrect},{session.Accuracy:F1},{safeCollectionName}");
                // Basic / No Collection Name
                //writer.WriteLine($"{session.Id},{session.StartedAt:o},{session.EndedAt:o},{session.TotalQuestions},{session.TotalCorrect},{session.TotalIncorrect},{session.Accuracy:F1}");
            }
        }

    }

}
