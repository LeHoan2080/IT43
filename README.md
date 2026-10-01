# StationeryWarehouse

ASP.NET Core MVC application for a stationery warehouse.

## Run locally

1. Install the .NET 10 SDK and SQL Server.
2. Copy `appsettings.example.json` to `appsettings.json` and set your SQL Server connection string and a private JWT signing key.
3. Create and apply the Entity Framework Core migrations for your database:

   ```sh
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

4. Start the application:

   ```sh
   dotnet run
   ```

Local settings, build output, and migrations are excluded from this repository. Do not commit real passwords or signing keys.