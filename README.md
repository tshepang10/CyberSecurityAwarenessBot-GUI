# Cybersecurity Awareness Chatbot

A **Cybersecurity Awareness Assistant** for South African citizens — a C# WinForms application covering Parts 1–3 of the PROG6221 portfolio project.

## Run the GUI (Part 2 & 3 — default)

```powershell
dotnet run --project CyberSecurityAwarenessBot/CyberSecurityAwarenessBot.csproj
```

## Run the console (Part 1)

```powershell
dotnet run --project CyberSecurityAwarenessBot/CyberSecurityAwarenessBot.csproj -- --console
```

## Build

```powershell
dotnet build CyberSecurityAwarenessBot.sln -c Release
```

## Features

### Part 1 — Console
- Voice greeting (`Assets/greetings.wav`) via `System.Media`
- ASCII art header with colored console UI and typing effect
- Name personalization, cybersecurity responses, input validation
- Modular code structure (services, models, UI)

### Part 2 — GUI
- WinForms chat interface with welcome screen
- Keyword recognition: password, scam, privacy, phishing
- Random varied responses and conversation follow-ups ("tell me more")
- Memory (remembers favorite topics) and sentiment detection
- Error handling with fallback responses

### Part 3 — Advanced
- **Task assistant** with MySQL storage (JSON fallback when DB unavailable)
- **Cybersecurity quiz** (12 questions, multiple-choice & true/false)
- **NLP simulation** via keyword/regex intent detection
- **Activity log** with recent summary and "Show More"

## MySQL setup (optional)

1. Install MySQL and create a database:

```sql
CREATE DATABASE cybersecurity_bot;
```

2. Update `CyberSecurityAwarenessBot/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Port=3306;Database=cybersecurity_bot;User=root;Password=yourpassword;"
  }
}
```

The app auto-creates the `tasks` table on first run. If MySQL is unavailable, tasks are stored locally in `tasks-fallback.json`.

## Project structure

| Folder | Purpose |
|--------|---------|
| `Models/` | User profile, tasks, quiz, enums |
| `Services/` | Chat logic, NLP, tasks, quiz, activity log |
| `UI/` | WinForms, console UI, quiz & task forms |
| `Assets/` | Voice greeting WAV file |
| `.github/workflows/` | GitHub Actions CI |

## GitHub & CI

- Minimum 6 meaningful commits required for submission
- CI workflow builds the project on every push (see `.github/workflows/ci.yml`)
- Include a screenshot of a successful CI run in your README before submitting

  ## YouTube Video Link
  https://youtu.be/D67usWLLDts


- .NET 10 SDK (Windows)
- Windows (WinForms)
- MySQL (optional, for Part 3 task persistence)
