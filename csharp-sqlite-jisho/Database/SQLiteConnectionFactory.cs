using Microsoft.Data.Sqlite; // instead of System.Data.SQLite

namespace csharp_sqlite_jisho.Database
{
    public static class SQLiteConnectionFactory
    {
        private static readonly string _connectionString = "Data Source=jisho.db;";

        public static SqliteConnection CreateConnection()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return connection;
        }
    }
}

