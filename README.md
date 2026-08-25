# Tweet Audit

Tweet Audit is a .NET console application that analyzes a user's X (formerly Twitter) archive using Google's Gemini AI and identifies tweets that may not align with user-defined criteria.

The application processes tweets in batches, sends them to Gemini for analysis, validates the AI responses, retries malformed or incomplete batches, tracks failed batches, and generates a CSV containing tweets flagged for manual review.

> **Important:** Tweet Audit does not automatically delete tweets. It produces a list of flagged tweets and their URLs so the user can review and manually delete them if necessary.

---

## Features

- Parse downloaded X/Twitter `tweets.js` archives
- Process tweets in batches of 25
- Define custom tweet-alignment criteria
- Analyze tweets using Google Gemini
- Receive structured JSON verdicts from Gemini
- Validate Gemini responses
- Detect malformed JSON responses
- Detect incomplete batch responses
- Validate returned tweet IDs
- Automatically retry failed batches
- Track permanently failed batches
- Identify failed batches using unique `BatchId`s
- Generate X/Twitter URLs for flagged tweets
- Export flagged tweets to CSV
- Dependency Injection
- Options Pattern configuration
- Clean Architecture
- Asynchronous batch processing
- Configurable concurrency
- Strongly typed C# models

---

# How It Works

The application follows this workflow:

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
   Chunk into 25
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
       ├───────────────┐
       │               │
    Valid           Invalid
       │               │
       ▼               ▼
 TweetVerdicts       Retry
                       │
                       ▼
                 Max Retries
                       │
                       ▼
                 FailedBatch
       │
       ▼
  Audit Results
       │
       ▼
   CSV Output
