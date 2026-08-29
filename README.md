# Tweet Audit

Tweet Audit is a .NET console application that analyzes a user's X (formerly Twitter) archive using Google's Gemini AI and identifies tweets that may not align with user-defined criteria.

The application parses a downloaded `tweets.js` archive, processes tweets in batches of 100, sends each batch to Gemini for analysis, validates the AI's responses, retries batches that fail due to transient errors, tracks batches that fail permanently, and exports the flagged tweets (with direct links) to a CSV for manual review.

> **Important:** Tweet Audit does not automatically delete tweets. It produces a list of flagged tweets and their URLs so the user can review and manually delete them if necessary.

---

## Features

- Parse downloaded X/Twitter `tweets.js` archives
- Process tweets in batches of 100
- Define custom tweet-alignment criteria via configuration
- Analyze tweets using Google Gemini
- Receive and deserialize structured JSON verdicts from Gemini
- Validate Gemini responses:
  - Detect malformed/empty JSON responses
  - Detect incomplete batch responses (verdict count mismatch)
  - Validate that returned tweet IDs match the batch sent
- Classify failures by type and respond accordingly:
  - Retry transient errors (HTTP 429 rate limits, 500/503/504 server errors) with exponential backoff
  - Halt the entire run on non-retryable client errors (e.g. invalid API key, bad request) rather than silently failing batch by batch
  - Record permanently failed batches (malformed responses, validation mismatches, exhausted retries) without stopping the rest of the run
- Identify failed batches using unique `BatchId`s
- Process batches concurrently with configurable parallelism
- Report live progress to the console as batches complete
- Generate X/Twitter URLs for flagged tweets
- Export flagged tweets to CSV
- Dependency Injection
- Options pattern configuration (alignment criteria, Gemini API key, X username)
- Layered architecture (Domain / Application / Infrastructure)
- Strongly typed C# models throughout

---

## How It Works

```text
X/Twitter Archive
       │
       ▼
    tweets.js
       │
       ▼
  ArchiveParser
       │
       ▼
   List<Tweet>
       │
       ▼
  Chunk into 100
       │
       ▼
   BatchContext
       │
       ▼
 BatchAuditService
       │
       ▼
   PromptBuilder
       │
       ▼
    Gemini AI
       │
       ▼
 JSON Response
       │
       ▼
Deserialize + Validate
       │
       ├────────────────────┬───────────────────┐
       │                    │                    │
    Valid            Transient error       Fatal error
       │             (429 / 5xx)          (bad key, 4xx)
       ▼                    │                    │
 TweetVerdicts         Retry with               ▼
       │              backoff, up to      Halt entire run
       │               MaxRetries               │
       │                    │                    ▼
       │              ┌─────┴─────┐        Error shown
       │           Succeeds    Exhausted     to user
       │              │             │
       │              ▼             ▼
       │         TweetVerdicts  FailedBatch
       │                              │
       └──────────────┬───────────────┘
                       ▼
                Aggregated Results
                       │
                       ▼
              Flagged Tweet URLs
                       │
                       ▼
                  CSV Output
```

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- A Google Gemini API key
- Your downloaded X/Twitter data archive

### Clone the repository

```bash
git clone https://github.com/InI-OLU/Tweet-Audit.git
cd Tweet-Audit
```

### Set up your archive

Tweet Audit expects your X/Twitter archive to live at a fixed location, so there's no path to configure:

```text
%USERPROFILE%\Downloads\ArchiveInput\data\tweets.js
```

(On Windows, that's typically `C:\Users\<you>\Downloads\ArchiveInput\data\tweets.js`.)

To set this up:

1. Download and unzip your X/Twitter data archive.
2. Rename the unzipped folder to `ArchiveInput`.
3. Move it into your **Downloads** folder, so `tweets.js` ends up at `Downloads\ArchiveInput\data\tweets.js`.

If the archive isn't found at that location, the app will tell you so directly when it runs.

### Configure

The repo ships with `appsettings.example.json` rather than a real `appsettings.json`, so your API key and personal settings aren't committed to source control. Copy it and fill in your own values:

```bash
cp appsettings.example.json appsettings.json
```

Then edit `appsettings.json` with:

- `criteria` — your alignment criteria for Gemini to judge tweets against
- `GeminiApiKey.ApiKey` — your Gemini API key
- `UserName.Name` — your X/Twitter username (used to build tweet URLs)

### Run

```bash
dotnet run
```

The app will parse your archive, process tweets in batches, print live progress to the console, and produce a CSV of flagged tweets when finished.

---

## Configuration

Settings are supplied via `appsettings.json` and bound with the Options pattern:

| Section          | Purpose                                      |
|------------------|-----------------------------------------------|
| `criteria`       | Alignment criteria used to judge tweets       |
| `GeminiApiKey`   | API key for Gemini requests                   |
| `UserName`       | X/Twitter username, used to build tweet URLs  |

The archive path is **not** configured via `appsettings.json`. It's resolved automatically at startup from `Downloads\ArchiveInput\data\tweets.js` — see [Set up your archive](#set-up-your-archive) above.

---

## Architecture

The project is organized into three layers:

- **Domain** — core models (`Tweet`, `TweetVerdict`, `BatchContext`, `FailedBatch`, configuration option types, domain exceptions)
- **Application** — orchestration and business logic (`TweetAuditService`, `BatchAuditService`, `PromptBuilder`, `TweetUrlBuilder`)
- **Infrastructure** — external concerns (`ArchiveParser`, `GeminiClient`)

Failure handling is deliberately split into three categories, each with different consequences:

1. **Transient** (rate limits, server errors) — retried automatically with exponential backoff.
2. **Permanent, batch-scoped** (malformed AI response, ID/count mismatch, exhausted retries) — the batch is recorded as failed and the run continues.
3. **Fatal** (invalid API key, rejected request) — the run halts immediately, since the same failure would recur for every remaining batch.