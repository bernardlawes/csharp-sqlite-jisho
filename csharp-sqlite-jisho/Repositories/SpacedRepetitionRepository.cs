using System;
using Microsoft.Data.Sqlite;
using csharp_sqlite_jisho.Database;

namespace csharp_sqlite_jisho.Repositories
{
    public class SpacedRepetitionRepository
    {

        public List<int> GetPriorityWordIds(int count)
        {
            var ids = new List<int>();

            using var connection = SQLiteConnectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT word_id
                FROM SpacedRepetition
                ORDER BY 
                    (times_incorrect - times_correct) DESC, -- prioritize mistakes
                    last_reviewed_at ASC NULLS FIRST -- prioritize older review
                LIMIT @count;
            ";
            command.Parameters.AddWithValue("@count", count);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                ids.Add(reader.GetInt32(0));
            }

            return ids;
        }

        public void RecordQuizResult(int wordId, bool correct)
        {
            using var connection = SQLiteConnectionFactory.CreateConnection();

            // Check if spaced repetition record exists for this word
            using var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "SELECT id FROM SpacedRepetition WHERE word_id = @wordId;";
            checkCommand.Parameters.AddWithValue("@wordId", wordId);

            var existingId = checkCommand.ExecuteScalar() as long?;

            double newEaseFactor = correct ? 0.15 : -0.25; // 🛠 Calculate ease factor delta based on correct/incorrect

            if (existingId.HasValue)
            {
                // Word exists — Update it
                using var updateCommand = connection.CreateCommand();
                updateCommand.CommandText = @"
                        UPDATE SpacedRepetition
                        SET last_reviewed_at = @now,
                            times_correct = times_correct + @correctDelta,
                            times_incorrect = times_incorrect + @incorrectDelta,
                            ease_factor = MAX(1.3, ease_factor + @easeDelta)
                        WHERE word_id = @wordId;
                    ";
                updateCommand.Parameters.AddWithValue("@now", DateTime.UtcNow.ToString("o"));
                updateCommand.Parameters.AddWithValue("@correctDelta", correct ? 1 : 0);
                updateCommand.Parameters.AddWithValue("@incorrectDelta", correct ? 0 : 1);
                updateCommand.Parameters.AddWithValue("@easeDelta", newEaseFactor);
                updateCommand.Parameters.AddWithValue("@wordId", wordId);
                updateCommand.ExecuteNonQuery();
            }
            else
            {
                // No record — Insert new
                using var insertCommand = connection.CreateCommand();
                insertCommand.CommandText = @"
                        INSERT INTO SpacedRepetition (word_id, last_reviewed_at, times_correct, times_incorrect, ease_factor)
                        VALUES (@wordId, @now, @correctInitial, @incorrectInitial, 2.5);
                    ";
                insertCommand.Parameters.AddWithValue("@wordId", wordId);
                insertCommand.Parameters.AddWithValue("@now", DateTime.UtcNow.ToString("o"));
                insertCommand.Parameters.AddWithValue("@correctInitial", correct ? 1 : 0);
                insertCommand.Parameters.AddWithValue("@incorrectInitial", correct ? 0 : 1);
                insertCommand.ExecuteNonQuery();
            }
        }

    }
}
