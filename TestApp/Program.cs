using System;
using csharp_sqlite_jisho.Database;
using csharp_sqlite_jisho.Models;
using csharp_sqlite_jisho.Repositories;

namespace csharp_sqlite_jisho.TestApp
{
    class Program
    {
        public class SessionStats_CurrentOnly
        {
            public int TotalQuestions { get; set; } = 0;
            public int TotalCorrect { get; set; } = 0;
            public int TotalIncorrect { get; set; } = 0;

            public double Accuracy => TotalQuestions > 0
                ? (double)TotalCorrect / TotalQuestions * 100
                : 0;

            public void PrintSummary()
            {
                Console.WriteLine("\n📊 Session Summary:");
                Console.WriteLine($"Total Questions: {TotalQuestions}");
                Console.WriteLine($"Correct Answers: {TotalCorrect}");
                Console.WriteLine($"Incorrect Answers: {TotalIncorrect}");
                Console.WriteLine($"Accuracy: {Accuracy:F1}%");
            }
        }

        static void StartFlashcardQuizWithSession_CurrentOnly(WordRepository repo)
        {
            var spacedRepo = new SpacedRepetitionRepository();
            var stats = new SessionStats_CurrentOnly();

            Console.WriteLine("Starting Random Flashcard Quiz!");
            Console.WriteLine("How many cards do you want to study?");
            if (!int.TryParse(Console.ReadLine(), out int count) || count <= 0)
            {
                Console.WriteLine("Invalid number. Exiting quiz.");
                return;
            }

            var flashcards = repo.GetRandomWords(count);

            foreach (var card in flashcards)
            {
                Console.WriteLine($"\n🔵 Kanji: {card.Kanji}");
                Console.WriteLine("(Press Enter to reveal reading and meaning...)");
                Console.ReadLine();

                Console.WriteLine($"📖 Reading: {card.Reading}");
                Console.WriteLine($"📚 Meaning: {card.Meaning}");
                Console.WriteLine("\nDid you remember it correctly? (y/n)");

                var response = Console.ReadLine();
                if (response?.ToLower() == "y")
                {
                    Console.WriteLine("✅ Great!");
                    stats.TotalCorrect++;
                    spacedRepo.RecordQuizResult(card.Id, true);
                }
                else
                {
                    Console.WriteLine("📝 No worries — keep practicing!");
                    stats.TotalIncorrect++;
                    spacedRepo.RecordQuizResult(card.Id, false);
                }

                stats.TotalQuestions++;
            }

            stats.PrintSummary(); // 📈 Show the report after the quiz!
        }

        static void ExportCollectionToCsv()
        {
            var collectionRepo = new CollectionRepository();
            var wordRepo = new WordRepository();

            var collections = collectionRepo.GetAllCollections();
            if (collections.Count == 0)
            {
                Console.WriteLine("❗ No collections exist.");
                return;
            }

            Console.WriteLine("\nAvailable Collections:");
            foreach (var collection in collections)
            {
                Console.WriteLine($"{collection.Id}: {collection.Name} - {collection.Description}");
            }

            Console.WriteLine("\nEnter the Collection ID to export:");
            if (!int.TryParse(Console.ReadLine(), out int collectionId) || !collections.Any(c => c.Id == collectionId))
            {
                Console.WriteLine("❗ Invalid Collection ID.");
                return;
            }

            var words = wordRepo.GetWordsByCollectionId(collectionId, 1000);

            if (words.Count == 0)
            {
                Console.WriteLine("\n📭 No words to export in this collection.");
                return;
            }

            Console.WriteLine("Enter the filename to export to (e.g., basic_verbs.csv):");
            var filename = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(filename))
            {
                Console.WriteLine("❗ Invalid filename.");
                return;
            }

            try
            {
                using var writer = new StreamWriter(filename);
                writer.WriteLine("Kanji,Reading,Meaning,Type,JLPTLevel,GradeLevel,Id");

                foreach (var word in words)
                {
                    writer.WriteLine($"{EscapeCsv(word.Kanji)},{EscapeCsv(word.Reading)},{EscapeCsv(word.Meaning)},{word.Type},{word.JLPTLevel},{word.GradeLevel},{word.Id}");
                }

                Console.WriteLine($"✅ Collection exported successfully to '{filename}'!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❗ Export failed: {ex.Message}");
            }
        }

        static string EscapeCsv(string? field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            return field;
        }


        static void ExportSessionHistoryToCsv()
        {
            var sessionRepo = new SessionStatRepository();

            Console.WriteLine("Enter the filename to export (e.g., session_history.csv):");
            var filename = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(filename))
            {
                Console.WriteLine("❗Invalid filename. Export cancelled.");
                return;
            }

            try
            {
                sessionRepo.ExportSessionStatsToCsv(filename);
                Console.WriteLine($"✅ Session history successfully exported to '{filename}'!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❗ Error exporting: {ex.Message}");
            }
        }


        static void ViewSessionHistory()
        {
            var sessionRepo = new SessionStatRepository();
            //var sessions = sessionRepo.GetAllSessionStats();

            var sessions = sessionRepo.GetAllSessionStatsWithCollection();
            if (sessions.Count == 0)
            {
                Console.WriteLine("\n📭 No session history found.");
                return;
            }

            Console.WriteLine("\n📚 Session History:");
            foreach (var session in sessions)
            {
                // With Collection Info Name
                var collectionInfo = !string.IsNullOrEmpty(session.CollectionName) ? $" (Collection: {session.CollectionName})" : "";
                Console.WriteLine($"[{session.StartedAt:G}]{collectionInfo} - {session.TotalQuestions} questions, {session.TotalCorrect} correct, {session.TotalIncorrect} incorrect, Accuracy: {session.Accuracy:F1}%");

                // With Collection ID
                //var collectionInfo = session.CollectionId.HasValue ? $" (Collection ID {session.CollectionId.Value})" : "";
                //Console.WriteLine($"[{session.StartedAt:G}]{collectionInfo} - {session.TotalQuestions} questions, {session.TotalCorrect} correct, {session.TotalIncorrect} incorrect, Accuracy: {session.Accuracy:F1}%");
                
                
                //Console.WriteLine($"[{session.StartedAt:G}] - {session.TotalQuestions} questions, {session.TotalCorrect} correct, {session.TotalIncorrect} incorrect, Accuracy: {session.Accuracy:F1}%");
            }

            // (Optional) Show overall stats
            var totalSessions = sessions.Count;
            var totalQuestions = sessions.Sum(s => s.TotalQuestions);
            var totalCorrect = sessions.Sum(s => s.TotalCorrect);
            var overallAccuracy = totalQuestions > 0 ? (double)totalCorrect / totalQuestions * 100 : 0;


            Console.WriteLine($"\n📈 Overall:");
            Console.WriteLine($"Total Sessions: {totalSessions}");
            Console.WriteLine($"Total Questions Answered: {totalQuestions}");
            Console.WriteLine($"Overall Accuracy: {overallAccuracy:F1}%");
        }


        static void StartFlashcardQuizWithSession_Persistent(WordRepository repo)
        {
            var spacedRepo = new SpacedRepetitionRepository();
            var sessionRepo = new SessionStatRepository();
            var stat = new SessionStat { StartedAt = DateTime.UtcNow };

            Console.WriteLine("Starting Random Flashcard Quiz!");
            Console.WriteLine("How many cards do you want to study?");
            if (!int.TryParse(Console.ReadLine(), out int count) || count <= 0)
            {
                Console.WriteLine("Invalid number. Exiting quiz.");
                return;
            }

            var flashcards = repo.GetRandomWords(count);

            foreach (var card in flashcards)
            {
                Console.WriteLine($"\n🔵 Kanji: {card.Kanji}");
                Console.WriteLine("(Press Enter to reveal reading and meaning...)");
                Console.ReadLine();

                Console.WriteLine($"📖 Reading: {card.Reading}");
                Console.WriteLine($"📚 Meaning: {card.Meaning}");
                Console.WriteLine("\nDid you remember it correctly? (y/n)");

                var response = Console.ReadLine();
                if (response?.ToLower() == "y")
                {
                    Console.WriteLine("✅ Great!");
                    stat.TotalCorrect++;
                    spacedRepo.RecordQuizResult(card.Id, true);
                }
                else
                {
                    Console.WriteLine("📝 No worries — keep practicing!");
                    stat.TotalIncorrect++;
                    spacedRepo.RecordQuizResult(card.Id, false);
                }

                stat.TotalQuestions++;
            }

            stat.EndedAt = DateTime.UtcNow;
            sessionRepo.InsertSessionStat(stat);

            Console.WriteLine($"\n📊 Session Completed: {stat.TotalQuestions} questions, {stat.TotalCorrect} correct, {stat.TotalIncorrect} incorrect.");
            Console.WriteLine($"✅ Accuracy: {stat.Accuracy:F1}%");
        }



        static void StartPriorityFlashcardQuiz(WordRepository repo, SpacedRepetitionRepository spacedRepo)
        {
            Console.WriteLine("Starting Priority Flashcard Quiz Mode!");
            Console.WriteLine("How many cards do you want to study?");

            if (!int.TryParse(Console.ReadLine(), out int count) || count <= 0)
            {
                Console.WriteLine("Invalid number. Exiting quiz.");
                return;
            }

            var priorityIds = spacedRepo.GetPriorityWordIds(count);
            if (priorityIds.Count == 0)
            {
                Console.WriteLine("No priority words found — great job!");
                return;
            }

            var flashcards = new List<Word>();
            foreach (var id in priorityIds)
            {
                var word = repo.GetWordById(id);
                if (word != null)
                    flashcards.Add(word);
            }

            int correctCount = 0;
            int incorrectCount = 0;

            foreach (var card in flashcards)
            {
                Console.WriteLine($"\n🔵 Kanji: {card.Kanji}");
                Console.WriteLine("(Press Enter to reveal reading and meaning...)");
                Console.ReadLine();

                Console.WriteLine($"📖 Reading: {card.Reading}");
                Console.WriteLine($"📚 Meaning: {card.Meaning}");
                Console.WriteLine("\nDid you remember it correctly? (y/n)");

                var response = Console.ReadLine();
                if (response?.ToLower() == "y")
                {
                    Console.WriteLine("✅ Great!");
                    correctCount++;
                    spacedRepo.RecordQuizResult(card.Id, true);
                }
                else
                {
                    Console.WriteLine("📝 No worries — keep practicing!");
                    incorrectCount++;
                    spacedRepo.RecordQuizResult(card.Id, false);
                }
            }

            Console.WriteLine($"\n🎯 Priority Quiz Completed! You got {correctCount} correct and {incorrectCount} wrong.");
            Console.WriteLine($"✅ Accuracy: {(correctCount + incorrectCount > 0 ? (correctCount * 100 / (correctCount + incorrectCount)) : 0)}%");
        }


        static void InsertDemoWords(WordRepository repo)
        {
            var demoWords = new List<Word>
            {
                new Word { Kanji = "日", Reading = "にち", Meaning = "day, sun", JLPTLevel = 5, GradeLevel = 1, Type = "kanji" },
                new Word { Kanji = "本", Reading = "ほん", Meaning = "book, origin", JLPTLevel = 5, GradeLevel = 1, Type = "kanji" },
                new Word { Kanji = "学生", Reading = "がくせい", Meaning = "student", JLPTLevel = 5, GradeLevel = 2, Type = "word" },
                new Word { Kanji = "食べる", Reading = "たべる", Meaning = "to eat", JLPTLevel = 5, GradeLevel = 2, Type = "word" },
                new Word { Kanji = "走る", Reading = "はしる", Meaning = "to run", JLPTLevel = 4, GradeLevel = 3, Type = "word" }
            };

            foreach (var word in demoWords)
            {
                repo.InsertOrUpdateWord(word);
            }

            Console.WriteLine("✅ Demo words inserted successfully.");
        }

        static void ImportFromCsvBulk()
        {
            var collectionRepo = new CollectionRepository();
            var wordRepo = new WordRepository();

            Console.WriteLine("Enter the path to the CSV file to import:");
            var filepath = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(filepath) || !File.Exists(filepath))
            {
                Console.WriteLine("❗ File not found. Import cancelled.");
                return;
            }

            Console.WriteLine("\nChoose a Collection:");
            var collections = collectionRepo.GetAllCollections();
            foreach (var collection in collections)
            {
                Console.WriteLine($"{collection.Id}: {collection.Name} - {collection.Description}");
            }
            Console.WriteLine("Enter an existing Collection ID, or leave blank to create a new Collection:");
            var collectionInput = Console.ReadLine()?.Trim();

            int collectionId;
            if (string.IsNullOrWhiteSpace(collectionInput))
            {
                Console.WriteLine("Enter a name for the new collection:");
                var name = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(name))
                {
                    Console.WriteLine("❗ Invalid name. Import cancelled.");
                    return;
                }

                Console.WriteLine("Enter a description (optional):");
                var description = Console.ReadLine()?.Trim();

                var newCollection = new Collection { Name = name, Description = description };
                collectionRepo.InsertCollection(newCollection);

                // Get the new collection ID
                collections = collectionRepo.GetAllCollections();
                collectionId = collections.OrderByDescending(c => c.Id).First().Id;
            }
            else
            {
                if (!int.TryParse(collectionInput, out collectionId) || !collections.Any(c => c.Id == collectionId))
                {
                    Console.WriteLine("❗ Invalid Collection ID.");
                    return;
                }
            }

            var wordsToImport = new List<Word>();

            using var reader = new StreamReader(filepath);
            string? line;
            bool firstLine = true;

            while ((line = reader.ReadLine()) != null)
            {
                if (firstLine)
                {
                    firstLine = false; // Skip header
                    continue;
                }

                var fields = ParseCsvLine(line);
                if (fields.Length < 4)
                {
                    Console.WriteLine($"⚠️ Skipping invalid line: {line}");
                    continue;
                }

                var word = new Word
                {
                    Kanji = fields[0].Trim(),
                    Reading = fields[1].Trim(),
                    Meaning = fields[2].Trim(),
                    Type = fields[3].Trim(),
                    JLPTLevel = fields.Length > 4 ? ParseNullableInt(fields[4]) : null,
                    GradeLevel = fields.Length > 5 ? ParseNullableInt(fields[5]) : null
                };

                wordsToImport.Add(word);
            }

            if (wordsToImport.Count == 0)
            {
                Console.WriteLine("❗ No valid words to import. Import cancelled.");
                return;
            }

            // 🚀 BULK INSERT
            wordRepo.BulkInsertWords(wordsToImport, collectionId);

            Console.WriteLine($"✅ Bulk import complete! {wordsToImport.Count} words processed.");
        }

        static string[] ParseCsvLine(string line)
        {
            return line.Split(',');
        }

        static int? ParseNullableInt(string value)
        {
            if (int.TryParse(value, out int result))
                return result;
            return null;
        }


        static void ListAllCollections()
        {
            var collectionRepo = new CollectionRepository();
            var collections = collectionRepo.GetAllCollections();

            Console.Clear();
            Console.WriteLine("📚 Available Collections:");

            if (collections.Count == 0)
            {
                Console.WriteLine("❗ No collections found.");
            }
            else
            {
                foreach (var collection in collections)
                {
                    Console.WriteLine($"[{collection.Id}] {collection.Name} - {collection.Description}");
                }
            }

            Console.WriteLine("\nPress Enter to return to the main menu...");
            Console.ReadLine();
        }




        static void ImportCollectionFromCsv()
        {
            var collectionRepo = new CollectionRepository();
            var wordRepo = new WordRepository();

            Console.WriteLine("Enter the path to the CSV file to import:");
            var filepath = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(filepath) || !File.Exists(filepath))
            {
                Console.WriteLine("❗ File not found. Import cancelled.");
                return;
            }

            Console.WriteLine("\nChoose a Collection:");
            var collections = collectionRepo.GetAllCollections();
            foreach (var collection in collections)
            {
                Console.WriteLine($"{collection.Id}: {collection.Name} - {collection.Description}");
            }
            Console.WriteLine("Enter an existing Collection ID, or leave blank to create a new Collection:");
            var collectionInput = Console.ReadLine()?.Trim();

            int collectionId;
            if (string.IsNullOrWhiteSpace(collectionInput))
            {
                Console.WriteLine("Enter a name for the new collection:");
                var name = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(name))
                {
                    Console.WriteLine("❗ Invalid name. Import cancelled.");
                    return;
                }

                Console.WriteLine("Enter a description (optional):");
                var description = Console.ReadLine()?.Trim();

                var newCollection = new Collection { Name = name, Description = description };
                collectionRepo.InsertCollection(newCollection);

                // Get the new collection ID
                collections = collectionRepo.GetAllCollections();
                collectionId = collections.OrderByDescending(c => c.Id).First().Id;
            }
            else
            {
                if (!int.TryParse(collectionInput, out collectionId) || !collections.Any(c => c.Id == collectionId))
                {
                    Console.WriteLine("❗ Invalid Collection ID.");
                    return;
                }
            }

            int importedCount = 0;

            using var connection = SQLiteConnectionFactory.CreateConnection();
            using var transaction = connection.BeginTransaction();

            using var reader = new StreamReader(filepath);
            string? line;
            bool firstLine = true;

            while ((line = reader.ReadLine()) != null)
            {
                if (firstLine)
                {
                    firstLine = false; // Skip header
                    continue;
                }

                var fields = ParseCsvLine(line);
                if (fields.Length < 4)
                {
                    Console.WriteLine($"⚠️ Skipping invalid line: {line}");
                    continue;
                }

                var kanji = fields[0].Trim();
                var reading = fields[1].Trim();
                var meaning = fields[2].Trim();
                var type = fields[3].Trim();
                int? jlptLevel = fields.Length > 4 ? ParseNullableInt(fields[4]) : null;
                int? gradeLevel = fields.Length > 5 ? ParseNullableInt(fields[5]) : null;

                var word = new Word
                {
                    Kanji = kanji,
                    Reading = reading,
                    Meaning = meaning,
                    Type = type,
                    JLPTLevel = jlptLevel,
                    GradeLevel = gradeLevel
                };

                var existingWord = wordRepo.FindWordByKanji(kanji);
                if (existingWord == null)
                {
                    //wordRepo.InsertOrUpdateWord(word);
                    wordRepo.InsertOrUpdateWord(word, connection);
                    existingWord = wordRepo.FindWordByKanji(kanji);
                }

                if (existingWord != null)
                {
                    using var cmd = connection.CreateCommand();
                    cmd.CommandText = @"
                INSERT INTO CollectionWord (collection_id, word_id, added_at)
                VALUES (@collectionId, @wordId, CURRENT_TIMESTAMP);
            ";
                    cmd.Parameters.AddWithValue("@collectionId", collectionId);
                    cmd.Parameters.AddWithValue("@wordId", existingWord.Id);
                    cmd.ExecuteNonQuery();
                    importedCount++;
                }
            }

            transaction.Commit();
            Console.WriteLine($"✅ Import complete! {importedCount} words added to collection.");
        }



        static void CreateNewCollection()
        {
            var collectionRepo = new CollectionRepository();

            Console.WriteLine("\nEnter a name for the new collection:");
            var name = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("❗ Invalid name. Collection creation cancelled.");
                return;
            }

            Console.WriteLine("Enter a description (optional):");
            var description = Console.ReadLine()?.Trim();

            var newCollection = new Collection
            {
                Name = name,
                Description = description
            };

            collectionRepo.InsertCollection(newCollection);

            Console.WriteLine($"✅ Collection '{name}' created successfully!");
        }

        static void ViewItemsInCollection()
        {
            var collectionRepo = new CollectionRepository();
            var wordRepo = new WordRepository();

            var collections = collectionRepo.GetAllCollections();
            if (collections.Count == 0)
            {
                Console.WriteLine("❗ No collections exist.");
                return;
            }

            Console.WriteLine("\nAvailable Collections:");
            foreach (var collection in collections)
            {
                Console.WriteLine($"{collection.Id}: {collection.Name} - {collection.Description}");
            }

            Console.WriteLine("\nEnter the Collection ID to view:");
            if (!int.TryParse(Console.ReadLine(), out int collectionId) || !collections.Any(c => c.Id == collectionId))
            {
                Console.WriteLine("❗ Invalid Collection ID.");
                return;
            }

            var words = wordRepo.GetWordsByCollectionId(collectionId, 1000); // Fetch up to 1000 words

            if (words.Count == 0)
            {
                Console.WriteLine("\n📭 No words linked to this collection yet.");
                return;
            }

            Console.WriteLine($"\n📚 Words in Collection:");
            foreach (var word in words)
            {
                Console.WriteLine($"[{word.Id}] {word.Kanji} ({word.Reading}) - {word.Meaning}");
            }
        }



        static void AddWordsToCollection_Flexible()
        {
            var collectionRepo = new CollectionRepository();
            var wordRepo = new WordRepository();

            var collections = collectionRepo.GetAllCollections();
            if (collections.Count == 0)
            {
                Console.WriteLine("❗ No collections exist. Please create one first.");
                return;
            }

            Console.WriteLine("\nAvailable Collections:");
            foreach (var collection in collections)
            {
                Console.WriteLine($"{collection.Id}: {collection.Name} - {collection.Description}");
            }

            Console.WriteLine("\nEnter the Collection ID you want to add words to:");
            if (!int.TryParse(Console.ReadLine(), out int collectionId) || !collections.Any(c => c.Id == collectionId))
            {
                Console.WriteLine("❗ Invalid Collection ID.");
                return;
            }

            Console.WriteLine("Enter part of the Kanji, Reading, or Meaning to search for:");

            var input = Console.ReadLine();
            var query = input.Trim();
            Console.WriteLine($"You entered '{input}' (Length: {input.Length})");
            Console.WriteLine($"Trimmed Val '{query}' (Length: {query.Length})");
            foreach (char c in query)
            {
                Console.WriteLine($"Char: '{c}' Unicode: {(int)c}");
            }



            if (string.IsNullOrWhiteSpace(query))
            {
                Console.WriteLine("❗ Invalid input.");
                return;
            }

            var searchResults = wordRepo.SearchWords(query);

            if (searchResults.Count == 0)
            {
                Console.WriteLine("❗ No matching words found.");
                return;
            }

            Console.WriteLine("\nMatching Words:");
            foreach (var word in searchResults)
            {
                Console.WriteLine($"{word.Id}: {word.Kanji} ({word.Reading}) - {word.Meaning}");
            }

            Console.WriteLine("\nEnter the ID of the word you want to add:");
            if (!int.TryParse(Console.ReadLine(), out int selectedId))
            {
                Console.WriteLine("❗ Invalid selection.");
                return;
            }

            var selectedWord = searchResults.FirstOrDefault(w => w.Id == selectedId);
            if (selectedWord == null)
            {
                Console.WriteLine("❗ Selected ID not found in search results.");
                return;
            }

            using var connection = SQLiteConnectionFactory.CreateConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO CollectionWord (collection_id, word_id, added_at)
                VALUES (@collectionId, @wordId, CURRENT_TIMESTAMP);
            ";
            cmd.Parameters.AddWithValue("@collectionId", collectionId);
            cmd.Parameters.AddWithValue("@wordId", selectedWord.Id);
            cmd.ExecuteNonQuery();

            Console.WriteLine($"✅ Word '{selectedWord.Kanji}' successfully added to collection!");
        }



        static void AddWordsToCollection_ByKanji()
        {
            var collectionRepo = new CollectionRepository();
            var wordRepo = new WordRepository();

            var collections = collectionRepo.GetAllCollections();
            if (collections.Count == 0)
            {
                Console.WriteLine("❗ No collections exist. Create one first!");
                return;
            }

            Console.WriteLine("\nAvailable Collections:");
            foreach (var collection in collections)
            {
                Console.WriteLine($"{collection.Id}: {collection.Name} - {collection.Description}");
            }

            Console.WriteLine("\nEnter the Collection ID you want to add words to:");
            if (!int.TryParse(Console.ReadLine(), out int collectionId) || !collections.Any(c => c.Id == collectionId))
            {
                Console.WriteLine("❗ Invalid Collection ID.");
                return;
            }

            Console.WriteLine("Enter the Kanji of the word you want to add (case-sensitive):");

            var input = Console.ReadLine();
            var kanji = input.Trim();
            Console.WriteLine($"You entered '{input}' (Length: {input.Length})");
            Console.WriteLine($"Trimmed Val '{kanji}' (Length: {kanji.Length})");
            foreach (char c in kanji)
            {
                Console.WriteLine($"Char: '{c}' Unicode: {(int)c}");
            }

            if (string.IsNullOrWhiteSpace(kanji))
            {
                Console.WriteLine("❗ Invalid Kanji.");
                return;
            }

            var word = wordRepo.FindWordByKanji(kanji);

            if (word == null)
            {
                Console.WriteLine("❗ Word not found in the database.");
                return;
            }

            using var connection = SQLiteConnectionFactory.CreateConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO CollectionWord (collection_id, word_id, added_at)
                VALUES (@collectionId, @wordId, CURRENT_TIMESTAMP);
            ";
            cmd.Parameters.AddWithValue("@collectionId", collectionId);
            cmd.Parameters.AddWithValue("@wordId", word.Id);

            cmd.ExecuteNonQuery();

            Console.WriteLine($"✅ Word '{word.Kanji}' added to collection successfully!");
        }


        static void ViewItemsInCollectionPaginated()
        {
            var collectionRepo = new CollectionRepository();
            var wordRepo = new WordRepository();

            var collections = collectionRepo.GetAllCollections();
            if (collections.Count == 0)
            {
                Console.WriteLine("❗ No collections exist.");
                return;
            }

            Console.WriteLine("\nAvailable Collections:");
            foreach (var collection in collections)
            {
                Console.WriteLine($"{collection.Id}: {collection.Name} - {collection.Description}");
            }

            Console.WriteLine("\nEnter the Collection ID to view:");
            if (!int.TryParse(Console.ReadLine(), out int collectionId) || !collections.Any(c => c.Id == collectionId))
            {
                Console.WriteLine("❗ Invalid Collection ID.");
                return;
            }

            var words = wordRepo.GetWordsByCollectionId(collectionId, 1000); // Fetch up to 1000 words

            if (words.Count == 0)
            {
                Console.WriteLine("\n📭 No words linked to this collection yet.");
                return;
            }

            const int pageSize = 3;
            int totalPages = (int)Math.Ceiling(words.Count / (double)pageSize);
            int currentPage = 1;

            while (true)
            {
                Console.Clear();
                Console.WriteLine($"📚 Words in Collection (Page {currentPage}/{totalPages})");

                var pageWords = words.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
                foreach (var word in pageWords)
                {
                    Console.WriteLine($"[{word.Id}] {word.Kanji} ({word.Reading}) - {word.Meaning}");
                }

                Console.WriteLine("\nNavigation: (N)ext | (P)revious | (Q)uit");
                var input = Console.ReadLine()?.Trim().ToLower();

                if (input == "n" && currentPage < totalPages)
                {
                    currentPage++;
                }
                else if (input == "p" && currentPage > 1)
                {
                    currentPage--;
                }
                else if (input == "q")
                {
                    break;
                }
            }
        }



        static void InsertDemoCollections(WordRepository wordRepo)
        {
            var collectionRepo = new CollectionRepository();

            // Create "Basic Verbs" collection
            var verbsCollection = new Collection { Name = "Basic Verbs", Description = "Common everyday action verbs" };
            collectionRepo.InsertCollection(verbsCollection);

            // Create "Basic Nouns" collection
            var nounsCollection = new Collection { Name = "Basic Nouns", Description = "Useful basic nouns" };
            collectionRepo.InsertCollection(nounsCollection);

            Console.WriteLine("✅ Demo collections created. Now inserting demo words...");

            // Insert Demo Words
            var demoWords = new List<Word>
            {
                new Word { Kanji = "食べる", Reading = "たべる", Meaning = "to eat", JLPTLevel = 5, GradeLevel = 2, Type = "word" },
                new Word { Kanji = "走る", Reading = "はしる", Meaning = "to run", JLPTLevel = 4, GradeLevel = 3, Type = "word" },
                new Word { Kanji = "見る", Reading = "みる", Meaning = "to see", JLPTLevel = 5, GradeLevel = 2, Type = "word" },
                new Word { Kanji = "聞く", Reading = "きく", Meaning = "to hear", JLPTLevel = 5, GradeLevel = 2, Type = "word" },
                new Word { Kanji = "話す", Reading = "はなす", Meaning = "to speak", JLPTLevel = 5, GradeLevel = 2, Type = "word" },
                new Word { Kanji = "本", Reading = "ほん", Meaning = "book", JLPTLevel = 5, GradeLevel = 1, Type = "kanji" },
                new Word { Kanji = "水", Reading = "みず", Meaning = "water", JLPTLevel = 5, GradeLevel = 1, Type = "kanji" },
                new Word { Kanji = "学校", Reading = "がっこう", Meaning = "school", JLPTLevel = 5, GradeLevel = 2, Type = "word" },
                new Word { Kanji = "先生", Reading = "せんせい", Meaning = "teacher", JLPTLevel = 5, GradeLevel = 2, Type = "word" },
                new Word { Kanji = "犬", Reading = "いぬ", Meaning = "dog", JLPTLevel = 5, GradeLevel = 1, Type = "word" }
            };

            foreach (var word in demoWords)
            {
                wordRepo.InsertOrUpdateWord(word);
            }

            Console.WriteLine("✅ Demo words inserted. Now linking words to collections...");

            // Now link words into collections
            InsertDemoCollectionWords();
        }

        static void InsertDemoCollectionWords()
        {
            var connection = SQLiteConnectionFactory.CreateConnection();

            using var cmd = connection.CreateCommand();

            // We assume Collections and Words were inserted first

            // Link Basic Verbs
            cmd.CommandText = @"
                INSERT INTO CollectionWord (collection_id, word_id, added_at)
                SELECT (SELECT id FROM Collections WHERE name = 'Basic Verbs'), id, CURRENT_TIMESTAMP
                FROM Words
                WHERE kanji IN ('食べる', '走る', '見る', '聞く', '話す');
            ";
            cmd.ExecuteNonQuery();

            // Link Basic Nouns
            cmd.CommandText = @"
                INSERT INTO CollectionWord (collection_id, word_id, added_at)
                SELECT (SELECT id FROM Collections WHERE name = 'Basic Nouns'), id, CURRENT_TIMESTAMP
                FROM Words
                WHERE kanji IN ('本', '水', '学校', '先生', '犬');
            ";
            cmd.ExecuteNonQuery();

            Console.WriteLine("✅ Words linked to demo collections successfully.");
        }




        static void StartFlashcardQuizByJLPT(WordRepository repo)
        {
            var spacedRepo = new SpacedRepetitionRepository();

            int correctCount = 0;
            int incorrectCount = 0;

            Console.WriteLine("Starting JLPT Flashcard Quiz Mode!");
            Console.WriteLine("Which JLPT Level? (5 = easiest, 1 = hardest)");

            if (!int.TryParse(Console.ReadLine(), out int jlptLevel) || jlptLevel < 1 || jlptLevel > 5)
            {
                Console.WriteLine("Invalid JLPT Level. Exiting quiz.");
                return;
            }

            Console.WriteLine("How many cards do you want to study?");
            if (!int.TryParse(Console.ReadLine(), out int count) || count <= 0)
            {
                Console.WriteLine("Invalid number. Exiting quiz.");
                return;
            }

            var flashcards = repo.GetRandomWordsByJLPT(jlptLevel, count);

            if (flashcards.Count == 0)
            {
                Console.WriteLine($"No words found for JLPT Level {jlptLevel}!");
                return;
            }

            foreach (var card in flashcards)
            {
                Console.WriteLine($"\n🔵 Kanji: {card.Kanji}");
                Console.WriteLine("(Press Enter to reveal reading and meaning...)");
                Console.ReadLine();

                Console.WriteLine($"📖 Reading: {card.Reading}");
                Console.WriteLine($"📚 Meaning: {card.Meaning}");
                Console.WriteLine("\nDid you remember it correctly? (y/n)");

                var response = Console.ReadLine();
                if (response?.ToLower() == "y")
                {
                    Console.WriteLine("✅ Great!");
                    correctCount++;
                    spacedRepo.RecordQuizResult(card.Id, true); // user got it right
                }
                else
                {
                    Console.WriteLine("📝 No worries — keep practicing!");
                    incorrectCount++;
                    spacedRepo.RecordQuizResult(card.Id, false); // user got it right
                }
            }

            Console.WriteLine("\n🎉 JLPT Quiz session complete!");
            Console.WriteLine($"\n🎯 Quiz Completed! You got {correctCount} correct and {incorrectCount} wrong.");
            Console.WriteLine($"✅ Accuracy: {(correctCount + incorrectCount > 0 ? (correctCount * 100 / (correctCount + incorrectCount)) : 0)}%");
        }



        static void StartFlashcardQuiz(WordRepository repo)
        {
            var spacedRepo = new SpacedRepetitionRepository();

            int correctCount = 0;
            int incorrectCount = 0;

            Console.WriteLine("Starting Flashcard Quiz Mode!");
            Console.WriteLine("How many cards do you want to study?");

            if (!int.TryParse(Console.ReadLine(), out int count) || count <= 0)
            {
                Console.WriteLine("Invalid number. Exiting quiz.");
                return;
            }

            var flashcards = repo.GetRandomWords(count);

            if (flashcards.Count == 0)
            {
                Console.WriteLine("No words found in database!");
                return;
            }

            foreach (var card in flashcards)
            {
                Console.WriteLine($"\n🔵 Kanji: {card.Kanji}");
                Console.WriteLine("(Press Enter to reveal reading and meaning...)");
                Console.ReadLine();

                Console.WriteLine($"📖 Reading: {card.Reading}");
                Console.WriteLine($"📚 Meaning: {card.Meaning}");
                Console.WriteLine("\nDid you remember it correctly? (y/n)");

                var response = Console.ReadLine();
                if (response?.ToLower() == "y")
                {
                    Console.WriteLine("✅ Great!");
                    correctCount++;
                    spacedRepo.RecordQuizResult(card.Id, true); // user got it right
                }
                else
                {
                    Console.WriteLine("📝 No worries — keep practicing!");
                    incorrectCount++;
                    spacedRepo.RecordQuizResult(card.Id, false); // user got it right
                }
            }

            Console.WriteLine("\n🎉 Quiz session complete!");
            Console.WriteLine($"\n🎯 Quiz Completed! You got {correctCount} correct and {incorrectCount} wrong.");
            Console.WriteLine($"✅ Accuracy: {(correctCount + incorrectCount > 0 ? (correctCount * 100 / (correctCount + incorrectCount)) : 0)}%");
        }

        static void StudyCollectionFlashcards(WordRepository repo)
        {
            var spacedRepo = new SpacedRepetitionRepository();
            var sessionRepo = new SessionStatRepository();

            var collectionId = SelectCollection();
            if (collectionId == null)
                return;

            Console.WriteLine("How many cards do you want to study?");
            if (!int.TryParse(Console.ReadLine(), out int count) || count <= 0)
            {
                Console.WriteLine("Invalid number. Exiting quiz.");
                return;
            }

            var flashcards = repo.GetWordsByCollectionId(collectionId.Value, count);

            if (flashcards.Count == 0)
            {
                Console.WriteLine("No words found in this collection.");
                return;
            }

            var stat = new SessionStat { StartedAt = DateTime.UtcNow, CollectionId = collectionId.Value };

            foreach (var card in flashcards)
            {
                Console.WriteLine($"\n🔵 Kanji: {card.Kanji}");
                Console.WriteLine("(Press Enter to reveal reading and meaning...)");
                Console.ReadLine();

                Console.WriteLine($"📖 Reading: {card.Reading}");
                Console.WriteLine($"📚 Meaning: {card.Meaning}");
                Console.WriteLine("\nDid you remember it correctly? (y/n)");

                var response = Console.ReadLine();
                if (response?.ToLower() == "y")
                {
                    Console.WriteLine("✅ Great!");
                    stat.TotalCorrect++;
                    spacedRepo.RecordQuizResult(card.Id, true);
                }
                else
                {
                    Console.WriteLine("📝 No worries — keep practicing!");
                    stat.TotalIncorrect++;
                    spacedRepo.RecordQuizResult(card.Id, false);
                }

                stat.TotalQuestions++;
            }

            stat.EndedAt = DateTime.UtcNow;
            sessionRepo.InsertSessionStat(stat);

            Console.WriteLine($"\n📚 Collection Study Session Complete!");
            Console.WriteLine($"✅ {stat.TotalCorrect}/{stat.TotalQuestions} correct. Accuracy: {stat.Accuracy:F1}%");
        }


        static int? SelectCollection()
        {
            var collectionRepo = new CollectionRepository();
            var collections = collectionRepo.GetAllCollections();

            if (collections.Count == 0)
            {
                Console.WriteLine("No collections available. Please create collections first.");
                return null;
            }

            Console.WriteLine("\nAvailable Collections:");
            foreach (var collection in collections)
            {
                Console.WriteLine($"{collection.Id}: {collection.Name} - {collection.Description}");
            }

            Console.WriteLine("\nEnter the Collection ID you want to study:");
            if (int.TryParse(Console.ReadLine(), out int collectionId))
            {
                if (collections.Any(c => c.Id == collectionId))
                    return collectionId;
            }

            Console.WriteLine("Invalid selection.");
            return null;
        }

        static void DumpAllWordsRaw()
        {
            var repo = new WordRepository();
            var words = repo.GetRandomWords(1000);

            Console.WriteLine("\nAll words from DB:");
            foreach (var word in words)
            {
                Console.WriteLine($"[ID: {word.Id}] Kanji: '{word.Kanji}' | Reading: '{word.Reading}' | Meaning: '{word.Meaning}'");
            }
        }

        static void DumpAllWordsRawVerbose()
        {
            var repo = new WordRepository();
            var words = repo.GetRandomWords(1000);

            Console.WriteLine("\nAll words with raw data:");
            foreach (var word in words)
            {
                Console.WriteLine($"[ID: {word.Id}] Kanji: '{word.Kanji}' (Length: {word.Kanji?.Length}) | Reading: '{word.Reading}' | Meaning: '{word.Meaning}'");
                if (word.Kanji != null)
                {
                    foreach (char c in word.Kanji)
                    {
                        Console.WriteLine($"Char: '{c}' Unicode: {(int)c}");
                    }
                }
                Console.WriteLine();
            }
        }

        static void Main(string[] args)
        {
            // Ensure Both Input and Output Encoding can render Japanese Characters..  However, know that working inside terminal is more reliable than working inside Powershell or Console due to fonts
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("Initializing database...");
            DatabaseInitializer.Initialize();
            Console.WriteLine("Database ready!");

            var repo = new WordRepository();
            var spacedRepo = new SpacedRepetitionRepository();

            // Insert a word
            var newWord = new Word
            {
                Kanji = "日",
                Reading = "にち",
                Meaning = "day, sun",
                JLPTLevel = 5,
                GradeLevel = 1,
                Type = "kanji"
            };

            //repo.InsertWord(newWord);
            repo.InsertOrUpdateWord(newWord);
            Console.WriteLine("Inserted word!");


            // Insert demo words (optional)
            InsertDemoWords(repo);

            // Insert demo Collections (optional)
            InsertDemoCollections(repo);

            //DumpAllWordsRaw();
            DumpAllWordsRawVerbose();
            //return;

            // Fetch and display all words
            var words = repo.GetAllWords();
            foreach (var word in words)
            {
                Console.WriteLine($"Inserted: {word.Id}: {word.Kanji} ({word.Reading}) - {word.Meaning}");
            }

            // Search for words - Query kanji, reading, and meaning fields
            var searchResults = repo.SearchWords("日");

            foreach (var word in searchResults)
            {
                //Console.WriteLine($"Found: {word.Id}: {word.Kanji} ({word.Reading}) - {word.Meaning}");
            }

            // Output all words in repository to console
            searchResults = repo.GetAllWords();
            foreach (var word in searchResults)
            {
                //Console.WriteLine($"All: {word.Id}: {word.Kanji} ({word.Reading}) - {word.Meaning}");
            }



            // User Menu to start flashcard quizes
            while (true)
            {
                Console.WriteLine("\n🎌 Welcome to Japanese Dictionary Flashcards!");
                Console.WriteLine("Choose a mode:");
                Console.WriteLine("1. Random Flashcard Quiz");
                Console.WriteLine("2. JLPT Level Flashcard Quiz");
                Console.WriteLine("3. Priority Flashcard Quiz (Spaced Repetition)");
                Console.WriteLine("4. Random Flashcard Quiz with Current Session Statistics (Spaced Repetition)");
                Console.WriteLine("5. Random Flashcard Quiz with Persistent Session Statistics (Spaced Repetition)");
                Console.WriteLine("6. View Session History");
                Console.WriteLine("7. Export Session History to CSV");
                Console.WriteLine("8. Study a Specific Collection");
                Console.WriteLine("9. Create New Collection");
                Console.WriteLine("10. Add Word to Existing Collection (Kanji)");
                Console.WriteLine("11. Add Word to Existing Collection (Flexible)");
                Console.WriteLine("12. View Items in a Collection");
                Console.WriteLine("13. View Items in a Collection (Paginated)");
                Console.WriteLine("14. Export a Collection to CSV");
                Console.WriteLine("15. Import a Collection from CSV");
                Console.WriteLine("16. Import a Collection from CSV (Bulk Mode)");
                Console.WriteLine("17. List All Collections");

                Console.WriteLine("0. Exit");
                Console.Write("\nEnter your choice: ");

                var input = Console.ReadLine();

                if (input == "1")
                {
                    StartFlashcardQuiz(repo);
                    Console.WriteLine("===================================================================================");
                }
                else if (input == "2")
                {
                    StartFlashcardQuizByJLPT(repo);
                    Console.WriteLine("===================================================================================");
                }
                else if (input == "3")
                {
                    StartPriorityFlashcardQuiz(repo, spacedRepo);
                    Console.WriteLine("===================================================================================");
                }

                // Options with Temporary Session Statistics
                else if (input == "4")
                {
                    StartFlashcardQuizWithSession_CurrentOnly(repo);
                    Console.WriteLine("===================================================================================");
                }
                // Options with Historical Persistent Session Statistics
                else if (input == "5")
                {
                    StartFlashcardQuizWithSession_Persistent(repo);
                    Console.WriteLine("===================================================================================");
                }
                // View Session Statistics History
                else if (input == "6")
                {
                    ViewSessionHistory();
                    Console.WriteLine("===================================================================================");
                }
                // View Session Statistics History
                else if (input == "7")
                {
                    ExportSessionHistoryToCsv();
                    Console.WriteLine("===================================================================================");
                }
                // Study Specifc Collection
                else if (input == "8")
                {
                    StudyCollectionFlashcards(repo);
                    Console.WriteLine("===================================================================================");
                }
                // Create New Collection
                else if (input == "9")
                {
                    CreateNewCollection();
                    Console.WriteLine("===================================================================================");
                }
                // Add to Collection by Kanji
                else if (input == "10")
                {
                    AddWordsToCollection_ByKanji();
                    //AddWordsToCollection_Flexible();  // Flexible Search
                    Console.WriteLine("===================================================================================");
                }
                // Add to Collection by Flexible Search
                else if (input == "11")
                {
                    AddWordsToCollection_Flexible();
                    Console.WriteLine("===================================================================================");
                }
                // View Contents of a Collection
                else if (input == "12")
                {
                    ViewItemsInCollection();
                    Console.WriteLine("===================================================================================");
                }
                // View Contents of a Collection paginated
                else if (input == "13")
                {
                    ViewItemsInCollectionPaginated();
                    //AddWordsToCollection_Flexible();  // Flexible Search
                }
                // Export Collection to CSV
                else if (input == "14")
                {
                    ExportCollectionToCsv();
                    //AddWordsToCollection_Flexible();  // Flexible Search
                }
                // Import Collection to CSV
                else if (input == "15")
                {
                    ImportCollectionFromCsv();
                    //AddWordsToCollection_Flexible();  // Flexible Search
                }
                // Import Collection to CSV
                else if (input == "16")
                {
                    ImportFromCsvBulk();
                    //AddWordsToCollection_Flexible();  // Flexible Search
                }
                // Import Collection to CSV
                else if (input == "17")
                {
                    ListAllCollections();
                    //AddWordsToCollection_Flexible();  // Flexible Search
                }
                else if (input == "0")
                {
                    Console.WriteLine("Goodbye! 🎯");
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid choice. Please try again.");
                    Console.WriteLine("===================================================================================");
                }
            }
        }

    }
}
