using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using csharp_sqlite_jisho.Database;
using csharp_sqlite_jisho.Models;

namespace csharp_sqlite_jisho.Repositories
{
    public class CollectionRepository
    {
        public List<Collection> GetAllCollections()
        {
            var collections = new List<Collection>();

            using var connection = SQLiteConnectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT id, name, description FROM Collections ORDER BY name;";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                collections.Add(new Collection
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Name = reader["name"].ToString(),
                    Description = reader["description"].ToString()
                });
            }

            return collections;
        }

        public void InsertCollectionIfNotExists(Collection collection)
        {
            using var connection = SQLiteConnectionFactory.CreateConnection();

            using (var checkCommand = connection.CreateCommand())
            {
                checkCommand.CommandText = @"
            SELECT id FROM Collections
            WHERE name = @name;
        ";
                checkCommand.Parameters.AddWithValue("@name", collection.Name);

                var existingId = checkCommand.ExecuteScalar() as long?;

                if (existingId.HasValue)
                {
                    Console.WriteLine($"⚠️ Collection '{collection.Name}' already exists. Skipping.");
                    return; // Skip insertion
                }
            }

            using (var insertCommand = connection.CreateCommand())
            {
                insertCommand.CommandText = @"
            INSERT INTO Collections (name, description)
            VALUES (@name, @description);
        ";
                insertCommand.Parameters.AddWithValue("@name", collection.Name);
                insertCommand.Parameters.AddWithValue("@description", collection.Description ?? "");

                insertCommand.ExecuteNonQuery();
                Console.WriteLine($"✅ Inserted new collection: {collection.Name}");
            }
        }


        public void InsertCollection(Collection collection)
        {
            using var connection = SQLiteConnectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
        INSERT OR IGNORE INTO Collections (name, description)
        VALUES (@name, @description);
    ";
            command.Parameters.AddWithValue("@name", collection.Name);
            command.Parameters.AddWithValue("@description", collection.Description ?? "");

            command.ExecuteNonQuery();
        }


        public void DeleteCollection(int collectionId)
        {
            using var connection = SQLiteConnectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Collections WHERE id = @id;";
            command.Parameters.AddWithValue("@id", collectionId);
            command.ExecuteNonQuery();
        }
    }
}
