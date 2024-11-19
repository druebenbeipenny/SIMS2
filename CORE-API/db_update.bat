dotnet tool install --global dotnet-ef
dotnet tool update --global dotnet-ef
dotnet ef migrations add "Update" --context UserContext
dotnet ef migrations add "Update" --context IncidentContext
dotnet ef database update --context UserContext
dotnet ef database update --context IncidentContext