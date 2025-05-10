using Microsoft.Data.Sqlite;

namespace csharp_sqlite_jisho.Database
{
    public static class DatabaseInitializer
    {
        public static void Initialize()
        {
            using var connection = SQLiteConnectionFactory.CreateConnection();

            // ---------------------------------------------------------------------------
            var createWordsCommand = connection.CreateCommand();
            createWordsCommand.CommandText = @"
                CREATE TABLE IF NOT EXISTS Words (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    kanji TEXT,
                    reading TEXT,
                    meaning TEXT,
                    jlpt_level INTEGER,
                    grade_level INTEGER,
                    type TEXT,
                    UNIQUE(kanji, reading)
                );
            ";
            createWordsCommand.ExecuteNonQuery();

            // ---------------------------------------------------------------------------
            var createSRCommand = connection.CreateCommand();
            createSRCommand.CommandText = @"
                CREATE TABLE IF NOT EXISTS SpacedRepetition (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    word_id INTEGER,
                    last_reviewed_at TEXT,
                    times_correct INTEGER DEFAULT 0,
                    times_incorrect INTEGER DEFAULT 0,
                    ease_factor REAL DEFAULT 2.5,
                    FOREIGN KEY (word_id) REFERENCES Words(id)
                );
            ";
            createSRCommand.ExecuteNonQuery();

            // ---------------------------------------------------------------------------
            var createSessionStatsCommand = connection.CreateCommand();
            createSessionStatsCommand.CommandText = @"
                CREATE TABLE IF NOT EXISTS SessionStats (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    started_at TEXT,
                    ended_at TEXT,
                    total_questions INTEGER,
                    total_correct INTEGER,
                    total_incorrect INTEGER,
                    collection_id INTEGER,
                    FOREIGN KEY (collection_id) REFERENCES Collections(id)
                );
            ";
            createSessionStatsCommand.ExecuteNonQuery();

            // This creates the table to store Collection decks.
            // -----------------------------------------------------------------------------------------
            var createCollectionsCommand = connection.CreateCommand();
            createCollectionsCommand.CommandText = @"

                DROP TABLE IF EXISTS Collections;

                CREATE TABLE IF NOT EXISTS Collections (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL UNIQUE,
                    description TEXT
                );
            ";
            createCollectionsCommand.ExecuteNonQuery();

            // This creates the linking table (many-to-many relationship) between Collections and Words.
            // -----------------------------------------------------------------------------------------
            var createCollectionWordCommand = connection.CreateCommand();
            createCollectionWordCommand.CommandText = @"
                CREATE TABLE IF NOT EXISTS CollectionWord (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    collection_id INTEGER NOT NULL,
                    word_id INTEGER NOT NULL,
                    added_at TEXT,
                    FOREIGN KEY (collection_id) REFERENCES Collections(id) ON DELETE CASCADE,
                    FOREIGN KEY (word_id) REFERENCES Words(id) ON DELETE CASCADE
                );
            ";
            createCollectionWordCommand.ExecuteNonQuery();


        }


    }
}
