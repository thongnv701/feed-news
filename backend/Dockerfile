FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS build
WORKDIR /src
COPY ["src/LevelUpAccounts.API/LevelUpAccounts.API.csproj", "src/LevelUpAccounts.API/"]
COPY ["src/LevelUpAccounts.Application/LevelUpAccounts.Application.csproj", "src/LevelUpAccounts.Application/"]
COPY ["src/LevelUpAccounts.Domain/LevelUpAccounts.Domain.csproj", "src/LevelUpAccounts.Domain/"]
COPY ["src/LevelUpAccounts.Infrastructure/LevelUpAccounts.Infrastructure.csproj", "src/LevelUpAccounts.Infrastructure/"]
COPY ["src/LevelUpAccounts.Share/LevelUpAccounts.Share.csproj", "src/LevelUpAccounts.Share/"]
RUN dotnet restore "src/LevelUpAccounts.API/LevelUpAccounts.API.csproj"
COPY . .
WORKDIR "/src/src/LevelUpAccounts.API"
RUN dotnet build "LevelUpAccounts.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LevelUpAccounts.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0-preview AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LevelUpAccounts.API.dll"]