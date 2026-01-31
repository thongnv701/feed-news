# 🚀 NEWS FEED SERVICE - IMPLEMENTATION ROADMAP

**Project Goal:** Fetch top 5 articles per category (Business, Technology, World) from Reuters & VNExpress, summarize via Google Gemini 1.5 Flash, send to Slack daily at **6:00 PM (18:00)**.

---

## 📋 Project Summary

| Aspect | Decision |
|--------|----------|
| **Fetch Schedule** | Daily at 18:00 (6 PM) |
| **Ranking** | By recency (newest first) per category |
| **Top Articles** | 5 per category (15 total to Slack) |
| **Summary Length** | 200-500 characters, context-dependent |
| **Database** | PostgreSQL, EF Core, store summary+link only |
| **Slack Format** | Text only (no Rich Blocks) |
| **Retry Strategy** | 3 attempts, exponential backoff, then stop |
| **Storage Strategy** | Summary + URL only (no full article content) |
| **Error Handling** | Log and continue, don't crash service |
| **Deployment** | Background service (purely automated) |

---

## 🏗️ Architecture Overview

```
┌─────────────────────────────────────────────────────┐
│                   FeedNews.API                      │
│  (Entry Point - Background Service Host)            │
└──────────────────┬──────────────────────────────────┘
                   │
         ┌─────────┴─────────┐
         │                   │
    ┌────▼──────────┐   ┌────▼──────────────┐
    │  Application  │   │  Infrastructure   │
    │  (MediatR)    │   │  (Services)       │
    │  - Commands   │   │  - ReutersService │
    │  - Handlers   │   │  - VNExpressServ. │
    │  - Queries    │   │  - GeminiService  │
    │               │   │  - SlackService   │
    └────┬──────────┘   └────┬──────────────┘
         │                   │
         └────────┬──────────┘
                  │
         ┌────────▼─────────┐
         │     Domain       │
         │  - News entity   │
         │  - Value objects │
         └─────────────────┘
```

---

## 📝 PHASE 1: Domain Layer & Configuration

**Status:** 🔲 Pending | **Priority:** HIGH

### 1.1 Create `News` Domain Entity
- **File:** `backend/src/FeedNews.Domain/Entities/News.cs`
- **Properties:**
  - `Id` (Guid, primary key)
  - `Title` (string, max 500 chars)
  - `Url` (string, max 2000 chars)
  - `Summary` (string, max 2000 chars)
  - `Source` (NewsSource enum: Reuters, VNExpress)
  - `Category` (NewsCategory enum: Business, Technology, World)
  - `PublishedDate` (DateTime)
  - `FetchedAt` (DateTime)
  - `RankingScore` (decimal, default 0)
  - `SlackSentAt` (DateTime?, nullable)
  - `CreatedAt` (DateTime, default now)
  - `UpdatedAt` (DateTime)

### 1.2 Create Domain Enums
- **File:** `backend/src/FeedNews.Domain/Enums/NewsSource.cs`
  ```csharp
  public enum NewsSource
  {
      Reuters = 0,
      VNExpress = 1
  }
  ```

- **File:** `backend/src/FeedNews.Domain/Enums/NewsCategory.cs`
  ```csharp
  public enum NewsCategory
  {
      Business = 0,
      Technology = 1,
      World = 2
  }
  ```

### 1.3 Create Configuration Classes

- **File:** `backend/src/FeedNews.Infrastructure/Configuration/NewsConfiguration.cs`
  - Properties: FetchTime (string "18:00"), Categories (list), TopNewsPerCategory (int 5), SummaryLengthMin (int 200), SummaryLengthMax (int 500)
  - Section name: "NewsFeed"

- **File:** `backend/src/FeedNews.Infrastructure/Configuration/GeminiSettings.cs`
  - Properties: ApiKey (string), Model (string), MaxTokens (int 500)
  - Section name: "Gemini"

- **File:** `backend/src/FeedNews.Infrastructure/Configuration/SlackSettings.cs`
  - Properties: BotToken (string), ChannelId (string)
  - Section name: "Slack"

- **File:** `backend/src/FeedNews.Infrastructure/Configuration/NewsFeedsConfiguration.cs`
  - ReutersUrls (Dictionary<string, string> for each category)
  - VNExpressUrls (Dictionary<string, string> for each category)
  - Section name: "NewsFeeds"

### 1.4 Update appsettings.json
- **File:** `backend/src/FeedNews.API/appsettings.json`
- Add sections: NewsFeed, NewsFeeds, Gemini, Slack with complete RSS URLs and settings

### 1.5 Create NewsRepository Interface
- **File:** `backend/src/FeedNews.Application/Contracts/Repositories/INewsRepository.cs`
- Methods:
  - `Task AddAsync(News news)`
  - `Task UpdateAsync(News news)`
  - `Task<List<News>> GetByCategoryAndRecentAsync(NewsCategory category, int topCount)`
  - `Task<bool> ExistsByUrlAsync(string url)`

---

## 🔌 PHASE 2: Infrastructure Services (External Integrations)

**Status:** 🔲 Pending | **Priority:** HIGH

### 2.1 Add NuGet Packages
**File:** `backend/src/FeedNews.Infrastructure/FeedNews.Infrastructure.csproj`

Add:
```xml
<PackageReference Include="Microsoft.SemanticKernel" Version="1.14.0"/>
<PackageReference Include="Microsoft.SemanticKernel.Connectors.Google" Version="1.14.0"/>
<PackageReference Include="SlackNet" Version="0.13.6"/>
<PackageReference Include="AngleSharp" Version="1.1.2"/>
<PackageReference Include="Polly" Version="8.4.1"/>
```

### 2.2 Create ReutersNewsService
- **File:** `backend/src/FeedNews.Infrastructure/Services/ReutersNewsService.cs`
- Class: `ReutersNewsService`
- Method: `Task<List<News>> FetchNewsByCategoryAsync(NewsCategory category)`
- Implementation:
  - Use `System.ServiceModel.Syndication` to parse Reuters RSS
  - Map RSS items to News domain objects
  - Use IHttpClientFactory for HTTP calls
  - Implement Polly retry policy (3 attempts, exponential backoff)
  - Handle parsing errors gracefully

### 2.3 Create VNExpressNewsService
- **File:** `backend/src/FeedNews.Infrastructure/Services/VNExpressNewsService.cs`
- Same interface as ReutersNewsService
- Similar RSS parsing and error handling

### 2.4 Create GeminiSummarizationService
- **File:** `backend/src/FeedNews.Infrastructure/Services/GeminiSummarizationService.cs`
- Integrate Semantic Kernel + Gemini connector
- Method: `Task<string> SummarizeArticleAsync(string title, string content)`
- Prompt engineering: "Summarize this news article in 200-500 words, focusing on key facts and implications."
- Token limit: 500 max
- Error handling: Return placeholder summary if API fails

### 2.5 Create SlackNotificationService
- **File:** `backend/src/FeedNews.Infrastructure/Services/SlackNotificationService.cs`
- Use SlackNet to send messages
- Method: `Task<bool> SendNewsToSlackAsync(List<News> articles)`
- Format: Text-only message with article title, summary, source, and link
- Retry logic: 3 attempts with exponential backoff
- Return success/failure flag

### 2.6 Create HttpClientConfiguration
- **File:** `backend/src/FeedNews.Infrastructure/Configuration/HttpClientConfiguration.cs`
- Register IHttpClientFactory
- Configure Polly resilience policies:
  - Retry policy: 3 attempts with exponential backoff (1s, 2s, 4s)
  - Circuit breaker: Fail after 5 consecutive failures, wait 30s before retry
  - Timeout: 30 seconds per request

---

## ⚙️ PHASE 3: Application Layer (CQRS)

**Status:** 🔲 Pending | **Priority:** HIGH

### 3.1 Create FetchNewsCommand
- **File:** `backend/src/FeedNews.Application/Features/News/Commands/FetchNewsCommand.cs`
- Properties: Category (NewsCategory)
- Handler returns: List<News>
- Orchestrates both Reuters and VNExpress fetch

### 3.2 Create RankAndSelectTopNewsQuery
- **File:** `backend/src/FeedNews.Application/Features/News/Queries/RankAndSelectTopNewsQuery.cs`
- Properties: Category (NewsCategory), AllNews (List<News>)
- Ranks by PublishedDate (newest first)
- Selects top 5 articles
- Handler returns: List<News>

### 3.3 Create GenerateSummaryCommand
- **File:** `backend/src/FeedNews.Application/Features/News/Commands/GenerateSummaryCommand.cs`
- Properties: News (News entity with title/content)
- Calls GeminiSummarizationService
- Updates News.Summary
- Handler returns: News entity
- Handle null/error gracefully

### 3.4 Create SendNewsToSlackCommand
- **File:** `backend/src/FeedNews.Application/Features/News/Commands/SendNewsToSlackCommand.cs`
- Properties: Articles (List<News>)
- Calls SlackNotificationService
- Logs sent/failed status
- Handler returns: bool (success)

### 3.5 Add FluentValidation
- Create validators for each command/query
- Validate enum values, required fields
- Files: `backend/src/FeedNews.Application/Features/News/Validators/`

---

## 🗄️ PHASE 4: Persistence Layer (Database)

**Status:** 🔲 Pending | **Priority:** HIGH

### 4.1 Create News EF Core Entity Mapping
- **File:** `backend/src/FeedNews.Infrastructure/Persistence/Configuration/NewsConfiguration.cs`
- Entity mapping:
  ```
  Table: news_feeds
  Columns:
    - id (UUID, PK)
    - title (varchar 500)
    - url (varchar 2000)
    - summary (varchar 2000)
    - source (varchar 50)
    - category (varchar 50)
    - published_date (timestamp)
    - fetched_at (timestamp)
    - ranking_score (numeric)
    - slack_sent_at (timestamp, nullable)
    - created_at (timestamp)
    - updated_at (timestamp)
  Indexes:
    - (category, published_date DESC)
    - (url) - unique
  ```

### 4.2 Create NewsRepository
- **File:** `backend/src/FeedNews.Infrastructure/Persistence/Repositories/NewsRepository.cs`
- Implement `INewsRepository`
- Methods: AddAsync, UpdateAsync, GetByCategoryAndRecentAsync, ExistsByUrlAsync

### 4.3 Create EF Core Migration
- Command: `dotnet ef migrations add CreateNewsTable --project FeedNews.Infrastructure --startup-project FeedNews.API`
- Apply: `dotnet ef database update --project FeedNews.Infrastructure --startup-project FeedNews.API`

### 4.4 Update IUnitOfWork
- **File:** `backend/src/FeedNews.Infrastructure/Persistence/IUnitOfWork.cs`
- Add property: `INewsRepository News { get; }`
- Add implementation in UnitOfWork class

---

## 🔄 PHASE 5: Background Service (Orchestration)

**Status:** 🔲 Pending | **Priority:** HIGH

### 5.1 Create NewsAggregationBackgroundService
- **File:** `backend/src/FeedNews.API/BackgroundServices/NewsAggregationBackgroundService.cs`
- Inherit from `BackgroundService`
- Implement end-of-day trigger at 18:00
- Use `PeriodicTimer` or Cron scheduling

### 5.2 Orchestration Flow
Daily at 6:00 PM (18:00):
```
1. Log: "Starting news aggregation..."
2. For each category (Business, Technology, World):
   a. FetchNewsCommand → get articles from Reuters + VNExpress
   b. RankAndSelectTopNewsQuery → get top 5 per category
   c. For each article: GenerateSummaryCommand → summarize
   d. Save to database
3. After all categories processed: SendNewsToSlackCommand (15 articles total)
4. Log completion or errors
5. Next run: tomorrow at 18:00
```

### 5.3 Error Handling & Retries
- Catch exceptions per category/article
- Log errors but continue processing
- Retry transient failures up to 3 times
- Update `slack_sent_at` only on successful send

### 5.4 Logging
- Log at each step: fetching, summarizing, sending
- Include timing information
- Log errors with full exception details

---

## ⚙️ PHASE 6: Dependency Injection & Registration

**Status:** 🔲 Pending | **Priority:** HIGH

### 6.1 Update Program.cs
- **File:** `backend/src/FeedNews.API/Program.cs`

Register settings:
```csharp
builder.Services.Configure<NewsConfiguration>(builder.Configuration.GetSection("NewsFeed"));
builder.Services.Configure<GeminiSettings>(builder.Configuration.GetSection("Gemini"));
builder.Services.Configure<SlackSettings>(builder.Configuration.GetSection("Slack"));
builder.Services.Configure<NewsFeedsConfiguration>(builder.Configuration.GetSection("NewsFeeds"));
```

Register services:
```csharp
builder.Services.AddScoped<IReutersNewsService, ReutersNewsService>();
builder.Services.AddScoped<IVNExpressNewsService, VNExpressNewsService>();
builder.Services.AddScoped<IGeminiSummarizationService, GeminiSummarizationService>();
builder.Services.AddScoped<ISlackNotificationService, SlackNotificationService>();
```

Register repositories & UnitOfWork:
```csharp
builder.Services.AddScoped<INewsRepository, NewsRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
```

Register MediatR handlers (auto-scan):
```csharp
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
```

Register background service:
```csharp
builder.Services.AddHostedService<NewsAggregationBackgroundService>();
```

Configure HTTP clients with resilience:
```csharp
ConfigureHttpClientWithPolly(builder.Services);
```

### 6.2 Update appsettings.json
- **File:** `backend/src/FeedNews.API/appsettings.json`

Add complete configuration:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "FeedNewsDb": "Host=localhost;Port=5432;Database=feedNews;Username=postgres;Password=<your-password>"
  },
  "NewsFeed": {
    "FetchTime": "18:00",
    "Categories": ["Business", "Technology", "World"],
    "TopNewsPerCategory": 5,
    "SummaryLengthMin": 200,
    "SummaryLengthMax": 500
  },
  "NewsFeeds": {
    "Reuters": {
      "RssFeedUrls": {
        "Business": "https://feeds.reuters.com/business",
        "Technology": "https://feeds.reuters.com/technologyNews",
        "World": "https://feeds.reuters.com/worldNews"
      }
    },
    "VNExpress": {
      "RssFeedUrls": {
        "Business": "https://vnexpress.net/rss/kinh-doanh.rss",
        "Technology": "https://vnexpress.net/rss/so-hoa.rss",
        "World": "https://vnexpress.net/rss/the-gioi.rss"
      }
    }
  },
  "Gemini": {
    "Model": "gemini-1.5-flash",
    "MaxTokens": 500
  },
  "Slack": {
    "ChannelId": ""
  }
}
```

### 6.3 Update appsettings.Development.json
- Add secrets (never commit):
```json
{
  "Gemini": {
    "ApiKey": "<your-gemini-api-key>"
  },
  "Slack": {
    "BotToken": "<your-slack-bot-token>"
  }
}
```

### 6.4 Create .env.example
- **File:** `backend/.env.example`
```
GEMINI_API_KEY=<your-gemini-api-key>
SLACK_BOT_TOKEN=<your-slack-bot-token>
DB_PASSWORD=<your-postgres-password>
```

---

## ✅ PHASE 7: Build & Verification

**Status:** 🔲 Pending | **Priority:** HIGH

### 7.1 Run Build
```bash
cd backend
dotnet build
# Expected: Build succeeded with 0 errors
```

### 7.2 Apply Database Migrations
```bash
dotnet ef database update --project FeedNews.Infrastructure --startup-project FeedNews.API
# Expected: news_feeds table created in PostgreSQL
```

### 7.3 Run Application
```bash
dotnet run --project FeedNews.API
# Expected: Service starts, background service initializes
```

### 7.4 Verification Checklist
- [ ] Build compiles without errors
- [ ] Database migrations applied successfully
- [ ] Application runs without crashes
- [ ] Background service logs startup message
- [ ] No missing dependencies or null reference errors
- [ ] PostgreSQL connection successful
- [ ] Scheduled job registered for 18:00 daily
- [ ] Logging output shows debug messages

### 7.5 Manual Testing (Optional)
- Check PostgreSQL: `SELECT * FROM news_feeds;`
- Monitor logs at 18:00 for automatic execution
- Verify Slack messages received (if credentials configured)

---

## 📚 File Structure Summary

```
backend/
├── src/
│   ├── FeedNews.API/
│   │   ├── Program.cs (UPDATED)
│   │   ├── appsettings.json (UPDATED)
│   │   ├── appsettings.Development.json (UPDATED)
│   │   └── BackgroundServices/
│   │       └── NewsAggregationBackgroundService.cs (NEW)
│   │
│   ├── FeedNews.Application/
│   │   ├── Features/News/
│   │   │   ├── Commands/
│   │   │   │   ├── FetchNewsCommand.cs (NEW)
│   │   │   │   ├── FetchNewsCommandHandler.cs (NEW)
│   │   │   │   ├── GenerateSummaryCommand.cs (NEW)
│   │   │   │   ├── GenerateSummaryCommandHandler.cs (NEW)
│   │   │   │   ├── SendNewsToSlackCommand.cs (NEW)
│   │   │   │   └── SendNewsToSlackCommandHandler.cs (NEW)
│   │   │   ├── Queries/
│   │   │   │   ├── RankAndSelectTopNewsQuery.cs (NEW)
│   │   │   │   └── RankAndSelectTopNewsQueryHandler.cs (NEW)
│   │   │   └── Validators/
│   │   │       └── NewsCommandValidator.cs (NEW)
│   │   └── Contracts/Repositories/
│   │       └── INewsRepository.cs (NEW)
│   │
│   ├── FeedNews.Domain/
│   │   ├── Entities/
│   │   │   └── News.cs (NEW)
│   │   └── Enums/
│   │       ├── NewsCategory.cs (NEW)
│   │       └── NewsSource.cs (NEW)
│   │
│   └── FeedNews.Infrastructure/
│       ├── Configuration/
│       │   ├── NewsConfiguration.cs (NEW)
│       │   ├── GeminiSettings.cs (NEW)
│       │   ├── SlackSettings.cs (NEW)
│       │   ├── NewsFeedsConfiguration.cs (NEW)
│       │   └── HttpClientConfiguration.cs (NEW)
│       ├── Services/
│       │   ├── ReutersNewsService.cs (NEW)
│       │   ├── VNExpressNewsService.cs (NEW)
│       │   ├── GeminiSummarizationService.cs (NEW)
│       │   └── SlackNotificationService.cs (NEW)
│       └── Persistence/
│           ├── Configuration/
│           │   └── NewsConfiguration.cs (NEW - EF mapping)
│           ├── Repositories/
│           │   └── NewsRepository.cs (NEW)
│           └── Migrations/
│               └── <timestamp>_CreateNewsTable.cs (NEW)
│
└── .env.example (NEW)
```

---

## 🎯 Implementation Timeline

| Phase | Duration | Status |
|-------|----------|--------|
| Phase 1: Domain & Configuration | 1-2 hours | 🔲 Pending |
| Phase 2: Infrastructure Services | 2-3 hours | 🔲 Pending |
| Phase 3: Application Layer (CQRS) | 1-2 hours | 🔲 Pending |
| Phase 4: Persistence Layer | 1 hour | 🔲 Pending |
| Phase 5: Background Service | 1-2 hours | 🔲 Pending |
| Phase 6: DI & Registration | 1 hour | 🔲 Pending |
| Phase 7: Build & Verification | 30 mins | 🔲 Pending |
| **TOTAL** | **8-11 hours** | 🔲 Pending |

---

## ✨ Success Criteria

✅ All code compiles without errors  
✅ Database migrations applied successfully  
✅ Background service starts and logs initialization  
✅ Scheduled job registered for 18:00 daily  
✅ No null reference or dependency injection errors  
✅ PostgreSQL connection verified  
✅ Logging output shows execution steps  
✅ (Optional) Slack message received with sample news  

---

## 🚀 Ready to Implement?

This roadmap is comprehensive and ready to execute. Each phase builds on the previous one, ensuring clean separation of concerns and testability.

**Next step:** Proceed to Phase 1 implementation!
