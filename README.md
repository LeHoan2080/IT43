# StationeryWarehouse

ASP.NET Core MVC application for a stationery warehouse.

For end-user workflows and a module-by-module test checklist, see
[HUONG_DAN_SU_DUNG_VA_KIEM_THU.md](HUONG_DAN_SU_DUNG_VA_KIEM_THU.md).

## Run locally

1. Install the .NET 10 SDK and SQL Server.
2. Configure `ConnectionStrings:DefaultConnection` and `JwtSettings:Key`, `JwtSettings:Issuer`, `JwtSettings:Audience`, and `JwtSettings:ExpirationMinutes` using local user secrets or environment variables. Do not commit credentials or signing keys.
3. Restore dependencies and start the application:

   ```sh
   dotnet restore
   dotnet run
   ```

On startup, the application applies available Entity Framework Core migrations and seeds required demo data. The database account must have permission to apply migrations. For module workflows, default development accounts, and test cases, see [HUONG_DAN_SU_DUNG_VA_KIEM_THU.md](HUONG_DAN_SU_DUNG_VA_KIEM_THU.md).