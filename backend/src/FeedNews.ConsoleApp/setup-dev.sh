#!/bin/bash

# FeedNews Console App - Development Setup Script
# This script helps set up the development environment

set -e

echo "╔═══════════════════════════════════════════════════════════════════════╗"
echo "║                                                                       ║"
echo "║      FeedNews Console App - Development Setup Script                 ║"
echo "║                                                                       ║"
echo "╚═══════════════════════════════════════════════════════════════════════╝"
echo ""

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
CONSOLE_APP_DIR="$SCRIPT_DIR"

echo -e "${BLUE}ℹ️  ConsoleApp Directory: $CONSOLE_APP_DIR${NC}"
echo ""

# Check if appsettings.Development.json exists
if [ -f "$CONSOLE_APP_DIR/appsettings.Development.json" ]; then
    echo -e "${GREEN}✅ appsettings.Development.json already exists${NC}"
else
    echo -e "${YELLOW}⚠️  Creating appsettings.Development.json from template${NC}"
    cp "$CONSOLE_APP_DIR/appsettings.json" "$CONSOLE_APP_DIR/appsettings.Development.json"
    echo -e "${GREEN}✅ Created appsettings.Development.json${NC}"
fi

echo ""
echo "📝 Configuration Steps:"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""

# Function to read user input
prompt_for_input() {
    local prompt_text=$1
    local default_value=$2
    local variable_name=$3
    
    if [ -z "$default_value" ]; then
        read -p "$(echo -e ${BLUE}$prompt_text${NC}): " value
    else
        read -p "$(echo -e ${BLUE}$prompt_text${NC}) [default: $default_value]: " value
        value=${value:-$default_value}
    fi
    
    eval "$variable_name='$value'"
}

# Prompt for configuration values
echo "1. PostgreSQL Database Configuration"
echo "   (Typically: Host=localhost;Port=5432;Database=FeedNews;Username=postgres;Password=<password>)"
prompt_for_input "Enter database connection string" \
    "Host=localhost;Port=5432;Database=FeedNews;Username=postgres;Password=postgres" \
    "db_connection"

echo ""
echo "2. Gemini API Configuration"
echo "   (Get your API key from: https://ai.google.dev/)"
prompt_for_input "Enter Gemini API Key" "" "gemini_key"

echo ""
echo "3. Slack Configuration"
echo "   (Set up webhook at: https://api.slack.com/apps)"
prompt_for_input "Enter Slack Webhook URL" "" "slack_webhook"

echo ""
echo "4. Logging Configuration"
echo "   (Options: Debug, Information, Warning, Error, Critical)"
prompt_for_input "Enter default log level" "Debug" "log_level"

# Update appsettings.Development.json with user input
echo ""
echo -e "${BLUE}📝 Updating appsettings.Development.json...${NC}"

# Use Python or sed to update JSON (Python is more reliable)
if command -v python3 &> /dev/null; then
    python3 << EOF
import json

config_file = "$CONSOLE_APP_DIR/appsettings.Development.json"

with open(config_file, 'r') as f:
    config = json.load(f)

# Update configuration
config['ConnectionStrings']['FeedNewsDb'] = "$db_connection"
config['Gemini']['ApiKey'] = "$gemini_key"
config['Slack']['WebhookUrl'] = "$slack_webhook"
config['Logging']['LogLevel']['Default'] = "$log_level"

with open(config_file, 'w') as f:
    json.dump(config, f, indent=2)

print("✅ Configuration updated successfully")
EOF
else
    echo -e "${YELLOW}⚠️  Python3 not found. Manual update required.${NC}"
    echo ""
    echo "Please manually edit: $CONSOLE_APP_DIR/appsettings.Development.json"
    echo ""
    echo "Replace these values:"
    echo "  - ConnectionStrings.FeedNewsDb: $db_connection"
    echo "  - Gemini.ApiKey: $gemini_key"
    echo "  - Slack.WebhookUrl: $slack_webhook"
    echo "  - Logging.LogLevel.Default: $log_level"
fi

echo ""
echo "🔐 Security Check"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

# Verify appsettings.Development.json is in .gitignore
if grep -q "appsettings.Development.json" "$SCRIPT_DIR/../../.gitignore"; then
    echo -e "${GREEN}✅ appsettings.Development.json is properly ignored by Git${NC}"
else
    echo -e "${RED}❌ WARNING: appsettings.Development.json might not be ignored by Git!${NC}"
    echo "   Add to .gitignore: appsettings.Development.json"
fi

echo ""
echo "✅ Environment Setup"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

# Check for ASPNETCORE_ENVIRONMENT
if [ -z "$ASPNETCORE_ENVIRONMENT" ]; then
    echo -e "${YELLOW}⚠️  ASPNETCORE_ENVIRONMENT not set${NC}"
    echo "   Setting to: Development"
    export ASPNETCORE_ENVIRONMENT=Development
else
    echo -e "${GREEN}✅ ASPNETCORE_ENVIRONMENT=$ASPNETCORE_ENVIRONMENT${NC}"
fi

echo ""
echo "🚀 Ready to Run!"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""
echo "To run the console app:"
echo ""
echo -e "${BLUE}  export ASPNETCORE_ENVIRONMENT=Development${NC}"
echo -e "${BLUE}  cd backend${NC}"
echo -e "${BLUE}  dotnet run --project ./src/FeedNews.ConsoleApp/FeedNews.ConsoleApp.csproj${NC}"
echo ""
echo "OR:"
echo ""
echo -e "${BLUE}  cd backend/src/FeedNews.ConsoleApp${NC}"
echo -e "${BLUE}  dotnet run${NC}"
echo ""

echo -e "${GREEN}✅ Setup Complete!${NC}"
echo ""
echo "For more information, see: CONFIGURATION.md"
echo ""
