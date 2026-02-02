```
dotnet ef migrations add init table --project src/FeedNews.Infrastructure/FeedNews.Infrastructure.csproj --startup-project src/FeedNews.API/FeedNews.API.csproj --context FeedNewsContext
````

``````
dotnet ef database update --project src/FeedNews.Infrastructure/FeedNews.Infrastructure.csproj --startup-project src/FeedNews.API/FeedNews.API.csproj --context FeedNewsContext