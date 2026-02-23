```
dotnet-ef migrations add inittable2 --project src/FeedNews.Infrastructure/FeedNews.Infrastructure.csproj --startup-project src/FeedNews.API/FeedNews.API.csproj --context FeedNewsContext
````

``````
dotnet-ef database update --project src/FeedNews.Infrastructure/FeedNews.Infrastructure.csproj --startup-project src/FeedNews.API/FeedNews.API.csproj --context FeedNewsContext
