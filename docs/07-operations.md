# 07. Operations

## Configuration

| Setting                     | Where                                         | Default                         |
| --------------------------- | --------------------------------------------- | ------------------------------- |
| Database connection string  | `appsettings.json` → `ConnectionStrings:Default` (or user secrets / environment variable `ConnectionStrings__Default`) | — |
| API URL / port              | `Properties/launchSettings.json` (dev) or `ASPNETCORE_URLS` | `http://localhost:5295` |
| Environment                 | `ASPNETCORE_ENVIRONMENT`                      | `Development` (via launch profile) |
| Log levels                  | `appsettings.json` → `Logging:LogLevel`       | `Information`, ASP.NET Core `Warning` |
| Allowed frontend origins    | `Program.cs` → CORS policy `WebFrontend`      | `http://localhost:5500`, `http://127.0.0.1:5500` |
| API URL used by frontend    | `Web/js/config.js` → `API_Link`               | `http://localhost:5295`         |

Configuration is read in this order, with later sources overriding earlier ones: `appsettings.json` → `appsettings.{Environment}.json` → user secrets (Development only) → environment variables → command line.

### Secrets

Keep connection strings with real credentials **out of git**:

- **Development:** `dotnet user-secrets set "ConnectionStrings:Default" "..."`
- **Server:** the environment variable `ConnectionStrings__Default`

## CORS

The API only accepts browser requests from the origins listed in `Program.cs`:

```csharp
policy.WithOrigins("http://127.0.0.1:5500", "http://localhost:5500")
```

If the frontend is served from anywhere else (another port, or a real domain), add that origin there and redeploy the API.

## Environments

| Behaviour        | Development | Production |
| ---------------- | ----------- | ---------- |
| Swagger UI       | On          | Off        |
| Exception detail | Shown       | Hidden     |
| User secrets     | Loaded      | Not loaded |

## Deployment (outline)

The project currently runs only locally. A minimal deployment would be:

1. **Database:** create the database on the target server and apply the schema with `dotnet ef migrations script --idempotent -o migrate.sql`, run by a DBA. Use a dedicated SQL login with only the permissions it needs, never `sa`. The API only runs stored procedures, so its runtime login needs only `GRANT EXECUTE ON SCHEMA::dbo TO <api_user>`, with no direct table access. Apply migrations with a separate, more privileged login.
2. **API:** run `dotnet publish -c Release -o ./publish`, then host it on IIS (with the ASP.NET Core Hosting Bundle) or as a service. Set `ConnectionStrings__Default` and `ASPNETCORE_ENVIRONMENT=Production`.
3. **Frontend:** copy `Web/` to any static host (IIS, nginx, a storage bucket). Set `API_Link` in `js/config.js` to the API's public URL.
4. **CORS:** add the frontend's public origin to the `WebFrontend` policy.
5. Serve both over **HTTPS**.

## Backups

All state is in the `TawzeaITAsset` database. Back it up with standard SQL Server backups:

```sql
BACKUP DATABASE TawzeaITAsset TO DISK = 'D:\Backups\TawzeaITAsset.bak' WITH INIT;
```

## Logs

The API logs to the console (the default ASP.NET Core providers). To see the SQL that EF Core generates, set:

```json
"Logging": { "LogLevel": { "Microsoft.EntityFrameworkCore.Database.Command": "Information" } }
```

## Troubleshooting

| Symptom                                                                                 | Likely cause / fix                                                                                                                                                                                      |
| --------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Dashboard shows "Could not load dashboard data"                                         | The API isn't running, or `API_Link` in `js/config.js` is wrong                                                                                                                                         |
| Browser console: *blocked by CORS policy*                                               | The frontend isn't on `localhost:5500` / `127.0.0.1:5500`. Use that port or update CORS                                                                                                                 |
| Header and footer are empty                                                             | The page was opened as `file://`. Serve `Web/` over HTTP                                                                                                                                                |
| `dotnet ef` is not recognized                                                           | Run `dotnet tool install --global dotnet-ef`, then open a new terminal                                                                                                                                  |
| `A network-related or instance-specific error…` on startup or on the first request      | SQL Server isn't reachable. Check the host and port, that TCP/IP is enabled, and the firewall                                                                                                           |
| `Login failed for user`                                                                 | Wrong credentials in the connection string                                                                                                                                                              |
| `Cannot open database "TawzeaITAsset"`                                                  | Run `dotnet ef database update`                                                                                                                                                                         |
| Saving an edit returns 404                                                              | The asset was deleted by someone else. Reload the page                                                                                                                                                  |
| Form shows "An asset with this serial number already exists."                           | Another asset already uses that serial number (case-insensitive). Use a different one or edit the existing asset                                                                                        |
| `dotnet ef database update` fails on `CREATE UNIQUE INDEX ... IX_ITAssets_SerialNumber` | The table already has duplicate serial numbers. Find them with `SELECT SerialNumber, COUNT(*) FROM ITAssets GROUP BY SerialNumber HAVING COUNT(*) > 1`, fix or delete the extra rows, then run it again |
| `dotnet build` fails with *file is locked by TawzeaITAssetAPI*                          | The API is still running. Stop it first, or pass `--configuration Release` to `dotnet ef`                                                                                                               |
| Port 5295 or 5500 already in use                                                        | Stop the other process, or change the port (and update CORS / `config.js`)                                                                                                                              |
