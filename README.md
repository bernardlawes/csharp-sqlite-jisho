# 📚 csharp-sqlite-jisho

A lightweight C# class library for managing Japanese flashcards, dictionary lookups, spaced repetition scoring, and local persistence using SQLite. Designed for integration into learning applications or as a standalone language-learning backend.

---

## 🧩 Key Features

- 🈶 **Flashcard Management**
  - Create, update, and delete vocabulary flashcards
  - Store reading (kana), meaning, and kanji variations

- 🧠 **Spaced Repetition Scoring**
  - Simple SRS-style algorithm to prioritize weaker cards
  - Track score, streak, next review date, and more

- 📘 **Dictionary Support**
  - Lookup words using pre-parsed JMdict or EDICT2 entries
  - Supports fuzzy search and kanji/kana breakdowns

- 💾 **SQLite Storage**
  - Efficient local database structure
  - Schema optimized for lookups and low memory overhead

---

## 🏗️ Project Structure

```plaintext
csharp-sqlite-jisho/
├── Models/
│   ├── Flashcard.cs
│   ├── ScoreEntry.cs
├── Data/
│   ├── JishoContext.cs         # SQLite interface layer
│   ├── JmdictLoader.cs         # JMdict XML parser and caching
├── Services/
│   ├── FlashcardService.cs     # Add/edit/delete flashcards
│   ├── ReviewEngine.cs         # SRS logic
├── Resources/
│   └── jmdict.xml              # (Optional) Raw dictionary file
├── JishoLib.csproj

```

# Add Library to your project
```bat
dotnet add reference ../csharp-sqlite-jisho/csharp-sqlite-jisho.csproj
```

# Initialize the Database
```csharp
var db = new JishoContext("Data Source=flashcards.db");
db.Database.EnsureCreated();
```

# Add a flashcard
```csharp
var service = new FlashcardService(db);
service.CreateCard("勉強", "べんきょう", "study, learning");
```

# Run a review session
```
var engine = new ReviewEngine(db);
var nextCard = engine.GetNextCard();
engine.SubmitReview(nextCard, correct: true);
```
