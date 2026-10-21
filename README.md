# StationeryWarehouse

ASP.NET Core MVC application for a stationery warehouse.

For end-user workflows and a module-by-module test checklist

## Run locally

1. Install the .NET 10 SDK and SQL Server.
2. Restore the database by running `Database/StationeryWarehouse.sql` in SQL Server Management Studio. The script contains the anonymized demo database snapshot.
3. Copy `appsettings.example.json` to `appsettings.json` and configure the SQL Server connection string and `JwtSettings` with values for your environment. Never commit real credentials or signing keys.
4. Restore, build, and run:

   ```sh
   dotnet restore
   dotnet build
   dotnet run
   ```
