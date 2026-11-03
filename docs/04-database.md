# 04. Database

- **Engine:** Microsoft SQL Server
- **Database:** `TawzeaITAsset`
- **Managed by:** EF Core 8 code-first migrations

## Schema

### Table `ITAssets`

| Column                   | SQL type           | Null | Notes                                       |
| ------------------------ | ------------------ | ---- | ------------------------------------------- |
| `Id`                     | `uniqueidentifier` | No   | **Primary key** (`PK_ITAssets`)             |
| `Brand`                  | `nvarchar(100)`    | No   |                                             |
| `SerialNumber`           | `nvarchar(100)`    | No   | **Unique** (`IX_ITAssets_SerialNumber`)     |
| `WarrantyExpirationDate` | `datetime2`        | No   | Stored as a date at midnight                |
| `Status`                 | `nvarchar(20)`     | No   | One of `New`, `InUse`, `Damaged`, `Disposed` |
| `CreatedDate`            | `datetime2`        | No   | DB server local time at insert (`GETDATE()`) |

Equivalent DDL:

```sql
CREATE TABLE [ITAssets] (
    [Id]                     uniqueidentifier NOT NULL,
    [Brand]                  nvarchar(100)    NOT NULL,
    [SerialNumber]           nvarchar(100)    NOT NULL,
    [WarrantyExpirationDate] datetime2        NOT NULL,
    [Status]                 nvarchar(20)     NOT NULL,
    [CreatedDate]            datetime2        NOT NULL,
    CONSTRAINT [PK_ITAssets] PRIMARY KEY ([Id])
);

CREATE UNIQUE INDEX [IX_ITAssets_SerialNumber] ON [ITAssets] ([SerialNumber]);
```

The unique index enforces [Domain Rule 1.1](03-domain-rules.md#rules). The database's default collation is case-insensitive and ignores trailing spaces, so `ABC`, `abc` and `ABC ` count as the same serial number. There are no foreign keys or check constraints.

## Stored procedures

Every database operation in the API goes through a stored procedure. All asset CRUD in `ITAssetController` goes through these: They are created by the `AddITAssetStoredProcedures` migration (`CREATE OR ALTER`), and its `Down` drops them.

| Procedure            | Parameters                                                              | Returns / effect |
| -------------------- | ----------------------------------------------------------------------- | ---------------- |
| `sp_ITAsset_List`    | none                                                                    | All rows |
| `sp_ITAsset_GetById` | `@Id`                                                                   | Zero or one row |
| `sp_ITAsset_Create`  | `@Brand`, `@SerialNumber`, `@WarrantyExpirationDate`, `@Status`         | Inserts a row; `Id` = `NEWID()`, `CreatedDate` = `GETDATE()` |
| `sp_ITAsset_Update`  | `@Id`, `@Brand`, `@SerialNumber`, `@WarrantyExpirationDate`, `@Status`  | Updates the row; `CreatedDate` is never changed. Row count 0 = not found |
| `sp_ITAsset_Delete`  | `@Id`                                                                   | Deletes the row. Row count 0 = not found |

`@Status` is the enum name (e.g. `InUse`). `Update` and `Delete` leave `NOCOUNT` off so the API can read the affected row count.

The API calls them with `_db.ITAssets.FromSql($"EXEC ...")` for reads and `_db.Database.ExecuteSqlAsync($"EXEC ...")` for writes. `ExecuteSqlAsync` returns the affected row count, which is why `Update` and `Delete` must not set `NOCOUNT ON`. If a write breaks the unique index (SQL error 2627/2601), the API returns 409. Interpolated values become SQL parameters, so there is no injection risk. To change a procedure, add a new migration with an updated `CREATE OR ALTER`; don't edit the applied one.

### Dashboard procedures

`DashboardController` reads its figures through these procedures. They are created by the `AddDashboardStoredProcedures` migration.

| Procedure                                | Returns |
| ---------------------------------------- | ------- |
| `sp_Dashboard_AssetsByBrand`             | `Brand`, `Count` per brand, largest first |
| `sp_Dashboard_AssetsByStatus`            | `Status`, `Count` per status (statuses with no assets are left out) |
| `sp_Dashboard_WarrantyExpiringThisMonth` | Full asset rows whose `WarrantyExpirationDate` is in the current calendar month (DB server time), ordered by that date |

The count columns map to the `BrandCount` and `StatusCount` classes in `Models/DashboardCounts.cs` through `_db.Database.SqlQuery<T>`, so the column names must match the property names.

### Mapping

The entity is `Models/ITAsset.cs`, where `Brand` and `SerialNumber` are `[Required, MaxLength(100)]`. `Data/AppDbContext.cs` adds:

```csharp
modelBuilder.Entity<ITAsset>()
    .Property(a => a.Status)
    .HasConversion<string>()   // store "InUse", not 2
    .HasMaxLength(20);

modelBuilder.Entity<ITAsset>()
    .HasIndex(a => a.SerialNumber)
    .IsUnique();
```

## Migrations

| Migration               | Date       | Change                     |
| ----------------------- | ---------- | -------------------------- |
| `20261029215901_init`   | 2026-10-29 | Creates the `ITAssets` table |
| `20261102170513_AddITAssetStoredProcedures` | 2026-11-02 | Creates the five `sp_ITAsset_*` CRUD procedures |
| `20261102201813_AddSerialNumberUniqueIndex` | 2026-11-02 | Shrinks `Brand`/`SerialNumber` to `nvarchar(100)` and adds the unique index on `SerialNumber`. Fails if the table already has duplicate serial numbers or values over 100 characters; fix those rows first |
| `20261102205358_AddDashboardStoredProcedures` | 2026-11-02 | Creates the three `sp_Dashboard_*` read procedures |

### Common commands

Run all commands from `TawzeaITAssetAPI/`.

```bash
# Apply all pending migrations
dotnet ef database update

# After changing a model or AppDbContext, create a new migration
dotnet ef migrations add <DescriptiveName>

# Undo the last migration (only if it has not been applied anywhere)
dotnet ef migrations remove

# Roll the database back to a specific migration
dotnet ef database update <MigrationName>

# Generate a SQL script (for a DBA or production)
dotnet ef migrations script --idempotent -o migrate.sql
```

### Rules for changing the schema

1. Change the model, then **always** add a migration. Never edit the database by hand.
2. Never edit a migration that has already been applied to a shared database. Add a new one instead.
3. Commit the migration files and the updated `AppDbContextModelSnapshot.cs` together with the model change.
4. If a change affects a [domain rule](03-domain-rules.md), update that document too.

## Sample data

There is no seed data. To add test rows quickly:

```sql
INSERT INTO ITAssets (Id, Brand, SerialNumber, WarrantyExpirationDate, Status, CreatedDate) VALUES
 (NEWID(), 'Dell',   'DL-1001', DATEADD(day, 10,  CAST(GETDATE() AS date)), 'InUse',    GETDATE()),
 (NEWID(), 'HP',     'HP-2001', DATEADD(day, -30, CAST(GETDATE() AS date)), 'Damaged',  GETDATE()),
 (NEWID(), 'Lenovo', 'LN-3001', DATEADD(year, 2,  CAST(GETDATE() AS date)), 'New',      GETDATE()),
 (NEWID(), 'Dell',   'DL-1002', DATEADD(year, -1, CAST(GETDATE() AS date)), 'Disposed', GETDATE());
```
