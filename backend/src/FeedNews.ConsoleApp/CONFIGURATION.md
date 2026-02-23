# FeedNews Console App - Configuration Guide

## Development Setup

The FeedNews.ConsoleApp uses two configuration files for different environments:

### 1. **appsettings.json** (Production Template)
- **Location**: `backend/src/FeedNews.ConsoleApp/appsettings.json`
- **Committed to Git**: ✅ YES
- **Contains**: Template with placeholder values for production
- **Purpose**: Provides schema and documentation
- **Usage**: Should be customized per environment

### 2. **appsettings.Development.json** (Local Development)
- **Location**: `backend/src/FeedNews.ConsoleApp/appsettings.Development.json`
- **Committed to Git**: ❌ NO (Ignored by .gitignore)
- **Contains**: Actual development credentials and settings
- **Purpose**: Local testing and development
- **Usage**: Automatically loaded when `ASPNETCORE_ENVIRONMENT=Development`

## Environment Configuration

### Development Setup

1. **Create appsettings.Development.json**
   ```bash
   cp appsettings.json appsettings.Development.json
   ```

2. **Update with local credentials**
   ```json
   {
     "ConnectionStrings": {
       "FeedNewsDb": "Host=localhost;Port=5432;Database=FeedNews;Username=postgres;Password=yourLocalPassword"
     },
     "Gemini": {
       "ApiKey": "your-local-gemini-api-key"
     },
     "Slack": {
       "WebhookUrl": "https://hooks.slack.com/services/YOUR/LOCAL/WEBHOOK"
     }
   }
   ```

3. **Run in Development**
   ```bash
   export ASPNETCORE_ENVIRONMENT=Development
   dotnet run
   ```

### Production Setup

1. **Use appsettings.json with environment variables**
   ```bash
   export ASPNETCORE_ENVIRONMENT=Production
   export ConnectionStrings__FeedNewsDb=postgresql://prod-host:5432/...
   export Gemini__ApiKey=prod-api-key
   export Slack__WebhookUrl=prod-webhook-url
   dotnet run
   ```

2. **Or use .env file** (ignored by git)
   ```bash
   # .env (NOT committed)
   ASPNETCORE_ENVIRONMENT=Production
   ConnectionStrings__FeedNewsDb=...
   Gemini__ApiKey=...
   Slack__WebhookUrl=...
   ```

## Configuration Priority

The configuration system uses this priority order:

1. **Environment Variables** (Highest priority)
   ```
   ASPNETCORE_ENVIRONMENT=Development
   ConnectionStrings__FeedNewsDb=postgresql://...
   ```

2. **appsettings.Development.json** (If Environment=Development)
   ```json
   {
     "ConnectionStrings": { "FeedNewsDb": "..." }
   }
   ```

3. **appsettings.json** (Base template)
   ```json
   {
     "ConnectionStrings": { "FeedNewsDb": "Host=YOUR_HOST;..." }
   }
   ```

## Configuration Options

### Logging Configuration

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug|Information|Warning|Error|Critical",
      "Microsoft": "Warning",
      "FeedNews": "Debug"
    }
  }
}
```

**Log Levels:**
- `Debug`: Detailed diagnostic information (development)
- `Information`: Informational messages (production default)
- `Warning`: Warning messages
- `Error`: Error messages
- `Critical`: Critical errors

### News Feed Configuration

```json
{
  "NewsFeed": {
    "FetchTime": "18:00",                    // When to run (for scheduling)
    "Categories": ["Business", "Technology", "World"],
    "TopNewsPerCategory": 5,                 // Articles per category
    "SummaryLengthMin": 200,                 // Min summary length
    "SummaryLengthMax": 500                  // Max summary length
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
  }
}
```

### AI & External Services Configuration

```json
{
  "Gemini": {
    "ApiKey": "SET_VIA_ENVIRONMENT_VARIABLE_OR_SECRETS",
    "Model": "gemini-pro",
    "MaxTokens": 1500
  },
  "Slack": {
    "ChannelId": "C01234567890",
    "WebhookUrl": "SET_VIA_ENVIRONMENT_VARIABLE_OR_SECRETS",
    "MinimumLogLevel": "Warning",
    "IncludeException": true,
    "IncludeTimestamp": true
  }
}
```

## Secrets Management

### ✅ Recommended Approaches

#### 1. **Environment Variables** (Simple & Portable)
```bash
export ConnectionStrings__FeedNewsDb="postgresql://user:pass@host:5432/db"
export Gemini__ApiKey="your-gemini-key"
export Slack__WebhookUrl="https://hooks.slack.com/..."
dotnet run
```

#### 2. **User Secrets** (Development)
```bash
cd backend/src/FeedNews.ConsoleApp
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:FeedNewsDb" "postgresql://..."
dotnet user-secrets set "Gemini:ApiKey" "your-key"
dotnet user-secrets set "Slack:WebhookUrl" "your-webhook"
dotnet run
```

#### 3. **Azure Key Vault** (Production)
```csharp
// In Program.cs - can be integrated
if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
{
    var keyVaultUrl = new Uri(configuration["KeyVault:VaultUri"]);
    configBuilder.AddAzureKeyVault(keyVaultUrl, new DefaultAzureCredential());
}
```

#### 4. **.env File** (Local Development Only)
```bash
# .env (NOT committed to git - .gitignore blocks it)
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__FeedNewsDb=postgresql://localhost:5432/FeedNews?username=postgres&password=postgres
Gemini__ApiKey=your-local-api-key
Slack__WebhookUrl=https://hooks.slack.com/services/YOUR/LOCAL/WEBHOOK
```

### ❌ Never Do This
- ❌ Commit credentials to git
- ❌ Hardcode API keys in code
- ❌ Share secrets in version control
- ❌ Use appsettings.Production.json with secrets

## .gitignore Configuration

The project's `.gitignore` already includes:

```gitignore
# Environment and configuration files
.env
.env.local
.env.*.local
appsettings.Development.json
appsettings.*.Development.json
GreenHeart.API/appsettings.Development.json
backend/src/**/appsettings.Development.json
```

This ensures development configurations with local credentials are never committed.

## Development Workflow

### First Time Setup

```bash
# 1. Clone repository
git clone <repo-url>
cd backend/src/FeedNews.ConsoleApp

# 2. Create local development config
cp appsettings.json appsettings.Development.json

# 3. Edit with your local settings
nano appsettings.Development.json
# Update: ConnectionStrings, Gemini__ApiKey, Slack__WebhookUrl

# 4. Set environment
export ASPNETCORE_ENVIRONMENT=Development

# 5. Run
dotnet run
```

### Daily Development

```bash
export ASPNETCORE_ENVIRONMENT=Development
dotnet run

# Should load appsettings.json + appsettings.Development.json
# Environment variables override both
```

### Before Committing

```bash
# Verify appsettings.Development.json is NOT staged
git status

# Only these should be staged:
# - Code changes
# - appsettings.json (template)
# - Not: appsettings.Development.json
```

## Troubleshooting

### Configuration Not Loading

**Issue**: Settings not being applied
```
Solution:
1. Verify ASPNETCORE_ENVIRONMENT is set correctly
2. Ensure appsettings.Development.json exists in project root
3. Check file permissions are readable
4. Verify JSON syntax is valid (use https://jsonlint.com/)
```

### Database Connection Failed

**Issue**: "Unable to connect to database"
```
Solution:
1. Verify PostgreSQL is running: psql -U postgres
2. Check connection string in appsettings.Development.json
3. Format: Host=localhost;Port=5432;Database=FeedNews;Username=postgres;Password=<password>
4. Test connection: psql -h localhost -U postgres -d FeedNews
```

### API Keys Not Found

**Issue**: "Gemini API key is null"
```
Solution:
1. Verify environment variable is set: echo $Gemini__ApiKey
2. Or check appsettings.Development.json has the key
3. Environment variables use double underscores: Gemini__ApiKey
4. Not: Gemini.ApiKey or Gemini_ApiKey
```

### Secrets Committed Accidentally

**Issue**: "Oh no, I committed a secret!"
```
Solution:
1. Revoke the exposed key/token immediately
2. Run: git rm --cached backend/src/FeedNews.ConsoleApp/appsettings.Development.json
3. Commit: git commit -m "Remove accidentally committed secrets"
4. Force push: git push --force-with-lease
5. Future: Never do this. Verify .gitignore before committing.
```

## Configuration Examples

### Development (Local)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information"
    }
  },
  "ConnectionStrings": {
    "FeedNewsDb": "Host=localhost;Port=5432;Database=FeedNews;Username=postgres;Password=localpassword"
  },
  "Gemini": {
    "ApiKey": "AIzaSyD1234567890abcdefghijk"
  },
  "Slack": {
    "WebhookUrl": "https://hooks.slack.com/services/T123456/B123456/ABCD1234567890"
  }
}
```

### Staging (Shared)

```bash
# Use environment variables only
export ASPNETCORE_ENVIRONMENT=Staging
export ConnectionStrings__FeedNewsDb="postgresql://staging-user:staging-pass@staging-db:5432/feednews"
export Gemini__ApiKey="staging-gemini-key"
export Slack__WebhookUrl="https://hooks.slack.com/services/staging-webhook"
```

### Production (Secure)

```bash
# Use Azure Key Vault or similar
export ASPNETCORE_ENVIRONMENT=Production
export KeyVault__VaultUri="https://your-keyvault.vault.azure.net/"
export KeyVault__TenantId="your-tenant-id"
export KeyVault__ClientId="your-client-id"
export KeyVault__ClientSecret="your-client-secret"
```

## Summary

| Aspect | Development | Production |
|--------|-------------|-----------|
| **Config File** | appsettings.Development.json | appsettings.json + env vars |
| **In Git?** | ❌ No (ignored) | ✅ Yes (template only) |
| **Secrets** | Plaintext OK | Use env vars only |
| **Environment Var** | ASPNETCORE_ENVIRONMENT=Development | ASPNETCORE_ENVIRONMENT=Production |
| **Credentials** | Local database/keys | Secure via secrets manager |

---

**Remember**: Never commit appsettings.Development.json. It's automatically ignored by .gitignore.
