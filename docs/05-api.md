# 05. API

- **Base URL (development):** `http://localhost:5295`
- **Format:** JSON. Property names are camelCase. Enums are strings.
- **Authentication:** none
- **Interactive docs:** `http://localhost:5295/swagger` (Development environment only)

## Endpoints

| Method | Path                      | Description                  | Returns                 |
| ------ | ------------------------- | ---------------------------- | ----------------------- |
| GET    | `/ITAsset/List`           | All assets                   | `ITAsset[]`             |
| GET    | `/ITAsset/getById/{id}`   | One asset                    | `ITAsset`               |
| POST   | `/ITAsset/create`         | Create an asset              | `ITAsset[]` (full list) |
| PUT    | `/ITAsset/update/{id}`    | Update an asset              | `ITAsset[]` (full list) |
| DELETE | `/ITAsset/delete/{id}`    | Delete an asset              | `{ message }`           |
| GET    | `/Dashboard`              | Dashboard statistics         | `Dashboard`             |

## Schemas

### `ITAsset`

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "brand": "Dell",
  "serialNumber": "DL-1001",
  "warrantyExpirationDate": "2027-05-31T00:00:00",
  "status": "InUse",
  "createdDate": "2026-10-30T09:15:22.123"
}
```

`status` is one of `New`, `InUse`, `Damaged` or `Disposed`. See [Domain Rules](03-domain-rules.md#2-status-lifecycle).

### `ITAssetRequest`

The request body for `create` and `update` (`Models/ITAssetRequest.cs`). `id` and `createdDate` are server-owned, so they are not part of it.

| Field                    | Rules                                   |
| ------------------------ | --------------------------------------- |
| `brand`                  | Required, max 100 characters            |
| `serialNumber`           | Required, max 100 characters, unique    |
| `warrantyExpirationDate` | Required                                |
| `status`                 | Required, a valid status name           |

### `Dashboard`

```json
{
  "totalAssets": 4,
  "assetsByBrand":  [ { "brand": "Dell", "count": 2 }, { "brand": "HP", "count": 1 } ],
  "assetsByStatus": [ { "status": "InUse", "count": 1 }, { "status": "New", "count": 1 } ],
  "warrantyExpiringThisMonth": [ /* ITAsset[], ordered by warrantyExpirationDate */ ]
}
```

`assetsByBrand` is ordered largest first. `assetsByStatus` leaves out statuses with no assets. `totalAssets` is the sum of the `assetsByStatus` counts.

## Endpoint details

### `GET /ITAsset/List`

Returns every asset, in no guaranteed order. There is no paging or filtering on the server. The grid does both on the client.

### `GET /ITAsset/getById/{id}`

`id` must be a GUID. Returns the asset, or **404** with `{ "message": "Asset not found" }`.

### `POST /ITAsset/create`

Request body (`ITAssetRequest`):

```json
{
  "serialNumber": "DL-1001",
  "brand": "Dell",
  "status": "New",
  "warrantyExpirationDate": "2027-05-31"
}
```

- `sp_ITAsset_Create` generates `id` and `createdDate`. If a client sends them, they are ignored.
- Responds with **200** and the full asset list after the insert.
- Responds with **400** if a field is missing, longer than 100 characters, or `status` is not a valid name.
- Responds with **409** and `{ "message": "Serial number already exists" }` if another asset has the same serial number.

### `PUT /ITAsset/update/{id}`

`id` in the URL selects the asset. The body is an `ITAssetRequest`, with the same fields and rules as `create`. All four fields are overwritten. `createdDate` never changes.

- Responds with **200** and the full asset list.
- Responds with **400** on validation errors, as for `create`.
- Responds with **404** and `{ "message": "Asset not found" }` if no asset has that `id`.
- Responds with **409** and `{ "message": "Serial number already exists" }` if another asset has the same serial number.

### `DELETE /ITAsset/delete/{id}`

Deletes the asset permanently.

- Responds with **200** and `{ "message": "Asset deleted successfully" }`.
- Responds with **404** and `{ "message": "Asset not found" }` if no asset has that `id`.

### `GET /Dashboard`

Returns the figures defined in [Domain Rules §5](03-domain-rules.md#5-dashboard-figures).

## Errors

The API uses ASP.NET Core's defaults:

| Case                                  | Response                                  |
| ------------------------------------- | ----------------------------------------- |
| Model validation fails                | `400` with a `ProblemDetails` body listing the field errors |
| Unhandled exception (e.g. DB down)    | `500`. Details are shown only in Development |
| Asset not found                       | `404` with `{ "message": "Asset not found" }` (`getById`, `update`, `delete`) |
| Duplicate serial number               | `409` with `{ "message": "Serial number already exists" }` (`create`, `update`) |

## Trying it out

The easiest way is Swagger UI at `/swagger`. You can also use curl:

```bash
curl http://localhost:5295/ITAsset/List

curl -X POST http://localhost:5295/ITAsset/create \
  -H "Content-Type: application/json" \
  -d '{"serialNumber":"DL-1001","brand":"Dell","status":"New","warrantyExpirationDate":"2027-05-31"}'
```
