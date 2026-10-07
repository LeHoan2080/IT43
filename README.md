# StationeryWarehouse

ASP.NET Core MVC application for a stationery warehouse.

For end-user workflows and a module-by-module test checklist, see
[HUONG_DAN_SU_DUNG_VA_KIEM_THU.md](HUONG_DAN_SU_DUNG_VA_KIEM_THU.md).

## Run locally

1. Install the .NET 10 SDK and SQL Server.
2. Restore the database by running `Database/StationeryWarehouse.sql` in SQL Server Management Studio. The full snapshot contains account hashes and business/contact data, so the file is intentionally excluded from Git and must be transferred privately by the database owner.
3. Copy `appsettings.example.json` to `appsettings.json` and configure the SQL Server connection string and `JwtSettings` with values for your environment. Never commit real credentials or signing keys.
4. Restore, build, and run:

   ```sh
   dotnet restore
   dotnet build
   dotnet run
   ```

The application does not create or seed the database at startup. It checks database connectivity and required role data, then starts. For module workflows and test cases, see [HUONG_DAN_SU_DUNG_VA_KIEM_THU.md](HUONG_DAN_SU_DUNG_VA_KIEM_THU.md).