```
dotnet ef migrations add UpdateRoleTable --project src/GreenHeart.Infrastructure/GreenHeart.Infrastructure.csproj --startup-project src/GreenHeart.API/GreenHeart.API.csproj --context GreenHeartContext
````

``````
dotnet ef database update --project src/GreenHeart.Infrastructure/GreenHeart.Infrastructure.csproj --startup-project src/GreenHeart.API/GreenHeart.API.csproj --context GreenHeartContext