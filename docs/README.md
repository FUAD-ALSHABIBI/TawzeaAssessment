# IT Asset Management: Documentation

A web application for tracking a company's IT assets: brand, serial number, status and warranty expiry.

- **Backend:** ASP.NET Core 8 Web API with Entity Framework Core 8 on SQL Server
- **Frontend:** static HTML/CSS/JavaScript, with AG Grid (data grid) and Highcharts (charts)

```
Browser (Web/, port 5500) ──HTTP/JSON──▶ TawzeaITAssetAPI (port 5295) ──EF Core, stored procedures──▶ SQL Server (TawzeaITAsset)
```

## Quick start

```bash
# 1. Set ConnectionStrings:Default in TawzeaITAssetAPI/appsettings.json
cd TawzeaITAssetAPI
dotnet ef database update      # create the database, table and stored procedures
dotnet run                     # API on http://localhost:5295

# 2. In a second terminal
cd Web
python -m http.server 5500     # open http://localhost:5500
```

See [Getting Started](01-getting-started.md) for the full setup.

## Contents

| #   | Document                                 | Read it when you want to…                                            |
| --- | ---------------------------------------- | -------------------------------------------------------------------- |
| 01  | [Getting Started](01-getting-started.md) | Install, configure and run the project                               |
| 02  | [Architecture](02-architecture.md)       | Understand how the parts fit together                                |
| 03  | [Domain Rules](03-domain-rules.md)       | **Know what an asset is and how its data behaves (read this first)** |
| 04  | [Database](04-database.md)               | See the schema, migrations and how to change them                    |
| 05  | [API](05-api.md)                         | Call or change an endpoint                                           |
| 06  | [Frontend](06-frontend.md)               | Work on the pages, grid or charts                                    |
| 07  | [Operations](07-operations.md)           | Configure, deploy or troubleshoot                                    |


## Requirements coverage

| Requirement (test project section)              | Status | Where                                      |
| ----------------------------------------------- | ------ | ------------------------------------------ |
| 3. ITAsset table with all required fields       | Done   | [Database](04-database.md)                 |
| 4.1 Add / Edit / View forms, data grid (AG Grid)| Done   | [Frontend](06-frontend.md)                 |
| 4.1.5 Delete asset (optional)                   | Done   | [API](05-api.md)                           |
| 4.2 Charts by brand and by status (Highcharts)  | Done   | [Frontend](06-frontend.md)                 |
| 5. Two pages, navigation, header, footer        | Done   | [Frontend](06-frontend.md)                 |
| 6.1 / 6.2 HTML, CSS, JS, .NET, MSSQL            | Done   | [Architecture](02-architecture.md)         |
| 6.3 Stored procedures for all CRUD operations   | Done (dashboard reads too) | [Database](04-database.md#stored-procedures) |
| 7. Documentation                                | Done   | This folder                                |
