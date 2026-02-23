# Set Development Environment Variables for Windows PowerShell
$env:ASPNETCORE_ENVIRONMENT = "Development"

Write-Host "✅ Environment variables set for Development" -ForegroundColor Green
Write-Host "ASPNETCORE_ENVIRONMENT = $($env:ASPNETCORE_ENVIRONMENT)" -ForegroundColor Cyan

Write-Host ""
Write-Host "Now you can run: dotnet run" -ForegroundColor Yellow
