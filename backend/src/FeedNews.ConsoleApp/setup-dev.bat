@echo off
REM FeedNews Console App - Development Setup Script for Windows
REM This script helps set up the development environment

cls
echo.
echo ╔═══════════════════════════════════════════════════════════════════════╗
echo ║                                                                       ║
echo ║      FeedNews Console App - Development Setup Script (Windows)       ║
echo ║                                                                       ║
echo ╚═══════════════════════════════════════════════════════════════════════╝
echo.

setlocal enabledelayedexpansion

REM Script directory
set "SCRIPT_DIR=%~dp0"
set "CONSOLE_APP_DIR=%SCRIPT_DIR%"

echo [INFO] ConsoleApp Directory: %CONSOLE_APP_DIR%
echo.

REM Check if appsettings.Development.json exists
if exist "%CONSOLE_APP_DIR%appsettings.Development.json" (
    echo [OK] appsettings.Development.json already exists
) else (
    echo [WARN] Creating appsettings.Development.json from template
    copy "%CONSOLE_APP_DIR%appsettings.json" "%CONSOLE_APP_DIR%appsettings.Development.json"
    echo [OK] Created appsettings.Development.json
)

echo.
echo Configuration Steps:
echo ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
echo.

echo 1. PostgreSQL Database Configuration
echo    ^(Typically: Host=localhost;Port=5432;Database=FeedNews;Username=postgres;Password=^<password^>^)
set /p db_connection=Enter database connection string [default: Host=localhost;Port=5432;Database=FeedNews;Username=postgres;Password=postgres]: 
if "%db_connection%"=="" set db_connection=Host=localhost;Port=5432;Database=FeedNews;Username=postgres;Password=postgres

echo.
echo 2. Gemini API Configuration
echo    ^(Get your API key from: https://ai.google.dev/^)
set /p gemini_key=Enter Gemini API Key: 

echo.
echo 3. Slack Configuration
echo    ^(Set up webhook at: https://api.slack.com/apps^)
set /p slack_webhook=Enter Slack Webhook URL: 

echo.
echo 4. Logging Configuration
echo    ^(Options: Debug, Information, Warning, Error, Critical^)
set /p log_level=Enter default log level [default: Debug]: 
if "%log_level%"=="" set log_level=Debug

echo.
echo [INFO] Updating appsettings.Development.json...

REM PowerShell command to update JSON
powershell -Command ^
  "$content = Get-Content '%CONSOLE_APP_DIR%appsettings.Development.json' | ConvertFrom-Json; " ^
  "$content.ConnectionStrings.FeedNewsDb = '%db_connection%'; " ^
  "$content.Gemini.ApiKey = '%gemini_key%'; " ^
  "$content.Slack.WebhookUrl = '%slack_webhook%'; " ^
  "$content.Logging.LogLevel.Default = '%log_level%'; " ^
  "$content | ConvertTo-Json -Depth 10 | Set-Content '%CONSOLE_APP_DIR%appsettings.Development.json'"

if %errorlevel% equ 0 (
    echo [OK] Configuration updated successfully
) else (
    echo [WARN] PowerShell update failed. Please manually edit the file.
    echo        File: %CONSOLE_APP_DIR%appsettings.Development.json
)

echo.
echo Security Check
echo ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

findstr /M "appsettings.Development.json" "%SCRIPT_DIR%..\..\..\.gitignore" >nul
if %errorlevel% equ 0 (
    echo [OK] appsettings.Development.json is properly ignored by Git
) else (
    echo [WARN] appsettings.Development.json might not be ignored by Git!
    echo        Add to .gitignore: appsettings.Development.json
)

echo.
echo Environment Setup
echo ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

if "%ASPNETCORE_ENVIRONMENT%"=="" (
    echo [WARN] ASPNETCORE_ENVIRONMENT not set
    echo        Setting to: Development
    set ASPNETCORE_ENVIRONMENT=Development
) else (
    echo [OK] ASPNETCORE_ENVIRONMENT=%ASPNETCORE_ENVIRONMENT%
)

echo.
echo Ready to Run!
echo ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
echo.
echo To run the console app:
echo.
echo   set ASPNETCORE_ENVIRONMENT=Development
echo   cd backend
echo   dotnet run --project ./src/FeedNews.ConsoleApp/FeedNews.ConsoleApp.csproj
echo.
echo OR:
echo.
echo   cd backend\src\FeedNews.ConsoleApp
echo   dotnet run
echo.
echo.
echo [OK] Setup Complete!
echo.
echo For more information, see: CONFIGURATION.md
echo.

endlocal
pause
