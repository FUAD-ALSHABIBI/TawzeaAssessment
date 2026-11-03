# 01. Getting Started

## Prerequisites

| Tool                                                              | Used for                                |
| ----------------------------------------------------------------- | --------------------------------------- |
| [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)    | Building and running the API            |
| SQL Server (2019+)                                                | Database                                |
| `dotnet-ef` CLI                                                   | Applying migrations                     |
| Python 3, or any static file server (such as VS Code Live Server) | Serving the frontend                    |
| A modern browser                                                  | The frontend uses `<dialog>` and ES2020 |

Install the EF Core CLI once:

```bash
dotnet tool install --global dotnet-ef
```

## 1. Configure the database connection

Set `ConnectionStrings:Default` in `TawzeaITAssetAPI/appsettings.json`.

**Remote SQL Server with SQL authentication:**

```json
"ConnectionStrings": {
  "Default": "Server=<host>,1433;Database=TawzeaITAsset;User Id=<user>;Password=<password>;TrustServerCertificate=True;"
}
```

**Local SQL Server with Windows authentication:**

```json
"ConnectionStrings": {
  "Default": "Server=localhost;Database=TawzeaITAsset;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
}
```

## 2. Create the database

```bash
cd TawzeaITAssetAPI
dotnet ef database update
```

This applies every migration in order. It creates the `TawzeaITAsset` database (if it doesn't exist yet), the `ITAssets` table with its unique index on `SerialNumber`, and the `sp_ITAsset_*` and `sp_Dashboard_*` stored procedures. See [Database](04-database.md).

## 3. Run the API

```bash
cd TawzeaITAssetAPI
dotnet run
```

| URL                              | What                     |
| -------------------------------- | ------------------------ |
| http://localhost:5295            | API base URL             |
| http://localhost:5295/swagger    | Swagger UI (Development only) |

To also listen on HTTPS (https://localhost:7033), run `dotnet run --launch-profile https`.

## 4. Run the frontend

```bash
cd Web
python -m http.server 5500
```

Then open **http://localhost:5500**.

The frontend **must** run on port `5500`, from `localhost` or `127.0.0.1`. Requests from any other origin are blocked by the API's CORS policy. See [Operations](07-operations.md#cors).

## 5. Check that it works

1. The Dashboard loads with no red error message.
2. On **Manage**, click **+ Add Asset**, fill in the form and save. The new row appears in the grid.
3. Add a second asset with the same serial number. The form shows "An asset with this serial number already exists."
4. Back on **Dashboard**, the totals and charts include the new asset.

If something fails, see [Troubleshooting](07-operations.md#troubleshooting).
