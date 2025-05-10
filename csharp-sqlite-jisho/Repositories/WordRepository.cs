using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using csharp_sqlite_jisho.Models;
using csharp_sqlite_jisho.Database;

namespace csharp_sqlite_jisho.Repositories
{
    public class WordRepository
    {
        // If exists → UPDATE the existing record's meaning, level, type
        public void InsertOrUpdateWord(Word word, SqliteConnection? externalConnection = null)
        {
            var connection = externalConnection ?? SQLiteConnectionFactory.CreateConnection();

            // Check if word already exists
            using (var checkCommand = connection.CreateCommand())
            {
                checkCommand.CommandText = @"
            SELECT id FROM Words
            WHERE kanji = @kanji AND reading = @reading;
        ";
                checkCommand.Parameters.AddWithValue("@kanji", word.Kanji);
                checkCommand.Parameters.AddWithValue("@reading", word.Reading);

                var existingId = checkCommand.ExecuteScalar() as long?;

                if (existingId.HasValue)
                {
                    // Word exists — Update it
                    using var updateCommand = connection.CreateCommand();
                    updateCommand.CommandText = @"
                UPDATE Words
                SET meaning = @meaning,
                    jlpt_level = @jlpt_level,
                    grade_level = @grade_level,
                    type = @type
                WHERE id = @id;
            ";
                    updateCommand.Parameters.AddWithValue("@meaning", word.Meaning);
                    updateCommand.Parameters.AddWithValue("@jlpt_level", (object?)word.JLPTLevel ?? DBNull.Value);
                    updateCommand.Parameters.AddWithValue("@grade_level", (object?)word.GradeLevel ?? DBNull.Value);
                    updateCommand.Parameters.AddWithValue("@type", word.Type);
                    updateCommand.Parameters.AddWithValue("@id", existingId.Value);

                    updateCommand.ExecuteNonQuery();
                    Console.WriteLine($"Updated existing word: {word.Kanji} ({word.Reading})");
                }
                else
                {
                    // Word does not exist — Insert it
                    using var insertCommand = connection.CreateCommand();
                    insertCommand.CommandText = @"
                INSERT INTO Words (kanji, reading, meaning, jlpt_level, grade_level, type)
                VALUES (@kanji, @reading, @meaning, @jlpt_level, @grade_level, @type);
            ";
                    insertCommand.Parameters.AddWithValue("@kanji", word.Kanji);
                    insertCommand.Parameters.AddWithValue("@reading", word.Reading);
                    insertCommand.Parameters.AddWithValue("@meaning", word.Meaning);
                    insertCommand.Parameters.AddWithValue("@jlpt_level", (object?)word.JLPTLevel ?? DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@grade_level", (object?)word.GradeLevel ?? DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@type", word.Type);

                    insertCommand.ExecuteNonQuery();
                    Console.WriteLine($"Inserted new word: {word.Kanji} ({word.Reading})");
                }
            }

            // ❗ Only dispose the connection if we created it inside
            if (externalConnection == null)
            {
                connection.Dispose();
            }
        }


        public void BulkInsertWords(List<Word> words, int collectionId)
        {
            using var connection = SQLiteConnectionFactory.CreateConnection();
            using var transaction = connection.BeginTransaction();

            foreach (var word in words)
            {
                // Insert into Words table
                using (var insertCommand = connection.CreateCommand())
                {
                    insertCommand.CommandText = @"
                INSERT OR IGNORE INTO Words (kanji, reading, meaning, jlpt_level, grade_level, type)
                VALUES (@kanji, @reading, @meaning, @jlpt_level, @grade_level, @type);
            ";
                    insertCommand.Parameters.AddWithValue("@kanji", word.Kanji);
                    insertCommand.Parameters.AddWithValue("@reading", word.Reading);
                    insertCommand.Parameters.AddWithValue("@meaning", word.Meaning);
                    insertCommand.Parameters.AddWithValue("@jlpt_level", (object?)word.JLPTLevel ?? DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@grade_level", (object?)word.GradeLevel ?? DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@type", word.Type);

                    insertCommand.ExecuteNonQuery();
                }

                // Now link to Collection
                using (var linkCommand = connection.CreateCommand())
                {
                    linkCommand.CommandText = @"
                INSERT INTO CollectionWord (collection_id, word_id, added_at)
                SELECT @collectionId, id, CURRENT_TIMESTAMP
                FROM Words
                WHERE kanji = @kanji AND reading = @reading;
            ";
                    linkCommand.Parameters.AddWithValue("@collectionId", collectionId);
                    linkCommand.Parameters.AddWithValue("@kanji", word.Kanji);
                    linkCommand.Parameters.AddWithValue("@reading", word.Reading);

                    linkCommand.ExecuteNonQuery();
                }
            }

            transaction.Commit();
            Console.WriteLine($"✅ Bulk inserted {words.Count} words into Collection ID {collectionId}.");
        }



        //If exists → do nothing
        public void InsertWord(Word word)
        {
            using var connection = SQLiteConnectionFactory.CreateConnection();

            // First, check if the word already exists
            using (var checkCommand = connection.CreateCommand())
            {
                checkCommand.CommandText = @"
                SELECT COUNT(*) FROM Words
                WHERE kanji = @kanji AND reading = @reading;";
                checkCommand.Parameters.AddWithValue("@kanji", word.Kanji);
                checkCommand.Parameters.AddWithValue("@reading", word.Reading);

                var existingCount = Convert.ToInt32(checkCommand.ExecuteScalar());
                if (existingCount > 0)
                {
                    Console.WriteLine($"Word already exists: {word.Kanji} ({word.Reading}) — Skipping insert.");
                    return; // Word already exists, don't insert
                }
            }

            // Otherwise, insert it
            using (var insertCommand = connection.CreateCommand())
            {
                insertCommand.CommandText = @"
                INSERT INTO Words (kanji, reading, meaning, jlpt_level, grade_level, type)
                VALUES (@kanji, @reading, @meaning, @jlpt_level, @grade_level, @type);";
                insertCommand.Parameters.AddWithValue("@kanji", word.Kanji);
                insertCommand.Parameters.AddWithValue("@reading", word.Reading);
                insertCommand.Parameters.AddWithValue("@meaning", word.Meaning);
                insertCommand.Parameters.AddWithValue("@jlpt_level", (object?)word.JLPTLevel ?? DBNull.Value);
                insertCommand.Parameters.AddWithValue("@grade_level", (object?)word.GradeLevel ?? DBNull.Value);
                insertCommand.Parameters.AddWithValue("@type", word.Type);
                insertCommand.ExecuteNonQuery();
            }
        }


        public Word? FindWordByKanji(string kanji)
        {
            using var connection = SQLiteConnectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM Words
                WHERE TRIM(kanji) = TRIM(@kanji)
                COLLATE BINARY
                LIMIT 1;
            ";
            command.Parameters.AddWithValue("@kanji", kanji.Trim());

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Word
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Kanji = reader["kanji"].ToString(),
                    Reading = reader["reading"].ToString(),
                    Meaning = reader["meaning"].ToString(),
                    JLPTLevel = reader["jlpt_level"] as int?,
                    GradeLevel = reader["grade_level"] as int?,
                    Type = reader["type"].ToString()
                };
            }

            return null;
        }




        public List<Word> SearchWords(string query)
        {
            var words = new List<Word>();
            using var connection = SQLiteConnectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM Words
                WHERE TRIM(kanji) LIKE @kanjiQuery
                   OR reading LIKE @readingQuery
                   OR meaning LIKE @meaningQuery
                ORDER BY id;
            ";

            command.Parameters.AddWithValue("@kanjiQuery", "%" + query.Trim() + "%");
            //command.Parameters.AddWithValue("@kanjiQuery", $"%{query}%");
            command.Parameters.AddWithValue("@readingQuery", $"%{query}%");
            command.Parameters.AddWithValue("@meaningQuery", $"%{query}%");

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                words.Add(new Word
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Kanji = reader["kanji"].ToString(),
                    Reading = reader["reading"].ToString(),
                    Meaning = reader["meaning"].ToString(),
                    JLPTLevel = reader["jlpt_level"] as int?,
                    GradeLevel = reader["grade_level"] as int?,
                    Type = reader["type"].ToString()
                });
            }
            return words;
        }



        public List<Word> GetRandomWords(int count)
        {
            var words = new List<Word>();
            using var connection = SQLiteConnectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = $@"
                    SELECT * FROM Words
                    ORDER BY RANDOM()
                    LIMIT {count};
                ";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                words.Add(new Word
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Kanji = reader["kanji"].ToString(),
                    Reading = reader["reading"].ToString(),
                    Meaning = reader["meaning"].ToString(),
                    JLPTLevel = reader["jlpt_level"] as int?,
                    GradeLevel = reader["grade_level"] as int?,
                    Type = reader["type"].ToString()
                });
            }
            return words;
        }


        public List<Word> GetRandomWordsByJLPT(int jlptLevel, int count)
        {
            var words = new List<Word>();
            using var connection = SQLiteConnectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM Words
                WHERE jlpt_level = @jlptLevel
                ORDER BY RANDOM()
                LIMIT @count;
            ";
            command.Parameters.AddWithValue("@jlptLevel", jlptLevel);
            command.Parameters.AddWithValue("@count", count);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                words.Add(new Word
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Kanji = reader["kanji"].ToString(),
                    Reading = reader["reading"].ToString(),
                    Meaning = reader["meaning"].ToString(),
                    JLPTLevel = reader["jlpt_level"] as int?,
                    GradeLevel = reader["grade_level"] as int?,
                    Type = reader["type"].ToString()
                });
            }
            return words;
        }


        public List<Word> GetWordsByCollectionId(int collectionId, int count)
        {
            var words = new List<Word>();
            using var connection = SQLiteConnectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT w.*
                FROM Words w
                INNER JOIN CollectionWord cw ON w.id = cw.word_id
                WHERE cw.collection_id = @collectionId
                ORDER BY RANDOM()
                LIMIT @count;
            ";
            command.Parameters.AddWithValue("@collectionId", collectionId);
            command.Parameters.AddWithValue("@count", count);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                words.Add(new Word
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Kanji = reader["kanji"].ToString(),
                    Reading = reader["reading"].ToString(),
                    Meaning = reader["meaning"].ToString(),
                    JLPTLevel = reader["jlpt_level"] as int?,
                    GradeLevel = reader["grade_level"] as int?,
                    Type = reader["type"].ToString()
                });
            }
            return words;
        }



        public Word? GetWordById(int id)
        {
            using var connection = SQLiteConnectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Words WHERE id = @id;";
            command.Parameters.AddWithValue("@id", id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Word
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Kanji = reader["kanji"].ToString(),
                    Reading = reader["reading"].ToString(),
                    Meaning = reader["meaning"].ToString(),
                    JLPTLevel = reader["jlpt_level"] as int?,
                    GradeLevel = reader["grade_level"] as int?,
                    Type = reader["type"].ToString()
                };
            }

            return null; // If not found
        }



        public List<Word> GetAllWords()
        {
            var words = new List<Word>();
            using var connection = SQLiteConnectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Words;";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                words.Add(new Word
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Kanji = reader["kanji"].ToString(),
                    Reading = reader["reading"].ToString(),
                    Meaning = reader["meaning"].ToString(),
                    JLPTLevel = reader["jlpt_level"] as int?,
                    GradeLevel = reader["grade_level"] as int?,
                    Type = reader["type"].ToString()
                });
            }
            return words;
        }
    }
}
