# 02. Architecture

## Overview

The system has three parts that run independently:

```
┌──────────────────────────┐        ┌────────────────────────────┐        ┌──────────────────┐
│  Web/  (static files)    │  HTTP  │  TawzeaITAssetAPI          │  EF    │  SQL Server      │
│  index.html  manage.html │ ─────▶ │  ASP.NET Core 8 Web API    │ ─────▶ │  DB: TawzeaITAsset│
│  AG Grid, Highcharts     │  JSON  │  Controllers → AppDbContext│  Core  │  Table: ITAssets │
│  :5500                   │ ◀───── │  :5295                     │ ◀───── │  :1433           │
└──────────────────────────┘        └────────────────────────────┘        └──────────────────┘
```

- The **frontend** is plain static files with no build step. It calls the API with `fetch`.
- The **API** is stateless. Each request opens a scoped `AppDbContext` and calls stored procedures through EF Core for every database operation: `sp_ITAsset_*` for asset CRUD and `sp_Dashboard_*` for the dashboard figures.
- The **database** has a single table, `ITAssets`, and eight stored procedures (five `sp_ITAsset_*`, three `sp_Dashboard_*`). All of them are created by EF Core code-first migrations.

The frontend and API run on different origins, so the API enables CORS for the frontend's origin.

## Repository layout

```
TawzeaAssessment/
├── TawzeaITAssetAPI/                 # Backend
│   ├── Program.cs                    # DI, JSON options, CORS, Swagger, pipeline
│   ├── Controllers/
│   │   ├── ITAssetController.cs      # CRUD endpoints
│   │   └── DashboardController.cs    # Aggregated statistics
│   ├── Data/AppDbContext.cs          # EF Core context and model config
│   ├── Models/
│   │   ├── ITAsset.cs                # Entity
│   │   ├── ITAssetRequest.cs         # Create/update request body
│   │   ├── DashboardCounts.cs        # BrandCount / StatusCount rows
│   │   └── AssetStatus.cs            # Status enum
│   ├── Migrations/                   # EF Core migrations
│   ├── Properties/launchSettings.json
│   └── appsettings*.json
├── Web/                              # Frontend
│   ├── index.html                    # Dashboard page
│   ├── manage.html                   # Asset management page
│   ├── components/                   # header.html, footer.html (shared)
│   ├── css/style.css
│   └── js/
│       ├── config.js                 # API base URL
│       ├── layout.js                 # Injects header/footer
│       ├── dashboard.js              # Dashboard page logic
│       └── manage.js                 # Manage page logic
└── docs/                             # This documentation
```

## Backend components

| Component             | Responsibility                                                                  |
| --------------------- | ------------------------------------------------------------------------------- |
| `Program.cs`          | Registers controllers, the string-enum JSON converter, `AppDbContext` (SQL Server), the CORS policy `WebFrontend` and Swagger |
| `ITAssetController`   | List, get, create, update and delete assets through the `sp_ITAsset_*` stored procedures |
| `DashboardController` | Counts by brand and status, total count, and warranties expiring this month, through the `sp_Dashboard_*` stored procedures |
| `AppDbContext`        | `DbSet<ITAsset> ITAssets`; stores `Status` as a string (max 20 characters)      |

The controllers use `AppDbContext` directly. There is no service or repository layer, because the domain is a single entity.

## Request flow examples

**Loading the Manage page**

1. The browser loads `manage.html`, which loads `config.js`, `layout.js`, AG Grid (CDN) and `manage.js`.
2. `layout.js` fetches `components/header.html` and `components/footer.html` and injects them into the page.
3. `manage.js` calls `GET /ITAsset/List` and passes the result to AG Grid as `rowData`.

**Saving an asset**

1. The form is submitted. `manage.js` sends `POST /ITAsset/create` or `PUT /ITAsset/update/{id}` with an `ITAssetRequest` JSON body.
2. The controller validates the body, then runs `sp_ITAsset_Create` or `sp_ITAsset_Update`. If the serial number already exists it returns **409**, and if the asset is missing it returns **404**. Otherwise it returns the **full asset list** from `sp_ITAsset_List`.
3. `manage.js` replaces the grid's data with the returned list, or shows an error in the form.

**Loading the Dashboard**

1. `dashboard.js` calls `GET /Dashboard`.
2. The API runs `sp_Dashboard_AssetsByBrand`, `sp_Dashboard_AssetsByStatus` and `sp_Dashboard_WarrantyExpiringThisMonth`, then returns one JSON object. `totalAssets` is the sum of the status counts.
3. `dashboard.js` renders the stat cards, two Highcharts charts and the expiring-warranty table.

## Technology choices

| Concern        | Choice                     | Why                                                   |
| -------------- | -------------------------- | ----------------------------------------------------- |
| Backend        | .NET 8 (LTS)               | Allowed by the brief (section 6.2); long-term support |
| Database       | SQL Server (MSSQL)         | Allowed by the brief                                  |
| Data access    | EF Core 8, code-first      | Versioned schema via migrations, type-safe queries |
| Frontend       | Vanilla HTML/CSS/JS        | Required by the brief (section 6.1)                   |
| Data grid      | AG Grid Community          | Required by the brief (section 4.1)                   |
| Charts         | Highcharts                 | Required by the brief (section 4.2)                   |
| API docs       | Swagger (Swashbuckle)      | Built-in interactive testing during development       |
