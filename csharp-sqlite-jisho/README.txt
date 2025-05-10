Multi-mode Japanese Flashcard System
User-Custom Collections
├── Add your own words
├── Add your own collections
├── Add your own kanji
├── Add individually or in bulk
├── Insert new collections ✅
├── View/select collections ✅
├── Study specific collections ✅
├── Delete Collections
├── Users can create new collections and assign words to them
├── Track study sessions per collection ✅
├── Import CSV into Word Repository and Collections
Metrics-based Spaced Repetition
├── Lesser known words showsn more frequently
Dynamic, user driven quiz options
├── By JLPT level
├── By Collection
├── By Grade
├── By Kanji
Statistics and Performance tracking
├── Total Questions Answere - Measure volume
├── Total Correct - See how much you remember
├── Total Incorrect - See where you struggle
├── Accuracy % - Motivation boost when accuracy rises
├── Total Time - See how long you spend studying
├── Track long-term learning history,




csharp-sqlite-jisho/
├── Database/
├── Models/
├── Repositories/
├── Services/
└── csharp-sqlite-jisho.csproj



csharp-sqlite-jisho/
├── Database/
│   ├── SQLiteConnectionFactory.cs
│   ├── DatabaseInitializer.cs (optional - creates tables if needed)
│
├── Models/
│   ├── Word.cs
│   ├── Collection.cs
│   ├── CollectionWord.cs
│   ├── SpacedRepetition.cs
│
├── Repositories/
│   ├── WordRepository.cs
│   ├── CollectionRepository.cs
│   ├── SpacedRepetitionRepository.cs
│
├── Services/
│   ├── SpacedRepetitionService.cs
│
└── csharp-sqlite-jisho.csproj