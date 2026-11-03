# 06. Frontend

The frontend is plain HTML, CSS and JavaScript in `Web/`. There is no build step, no bundler and no framework. Third-party libraries load from CDNs.

## Pages

| Page          | File          | Script          | Libraries   | Purpose                                 |
| ------------- | ------------- | --------------- | ----------- | --------------------------------------- |
| Dashboard     | `index.html`  | `js/dashboard.js` | Highcharts | Stats, charts, expiring warranties      |
| Manage Assets | `manage.html` | `js/manage.js`    | AG Grid    | Grid with View / Add / Edit / Delete    |

Both pages share:

- `css/style.css`: all styles
- `js/config.js`: defines `API_Link` (the API base URL)
- `js/layout.js`: loads `components/header.html` into `#Header` and `components/footer.html` into `#Footer`, and fills in the current year

### Script load order

```html
<link rel="stylesheet" href="css/style.css">
<script src="js/config.js" defer></script>   <!-- 1. API_Link -->
<script src="js/layout.js" defer></script>   <!-- 2. header/footer -->
<script src="<library CDN>"></script>          <!--    AG Grid or Highcharts (blocking) -->
<script src="js/<page>.js" defer></script>   <!-- 3. page logic -->
```

`defer` scripts run in document order after parsing, so `API_Link` and the library are both available before the page script runs. Scripts are classic (not ES modules) and share the global scope.

## Layout (header, navigation, footer)

The header and footer are kept in `components/` and fetched at runtime, so they are defined in one place.

- **Header** (`components/header.html`): the app title and navigation links to `index.html` and `manage.html`
- **Footer** (`components/footer.html`): copyright with the current year and a GitHub link

Because they are loaded with `fetch`, the pages **must be served over HTTP**. Opening the files directly (`file://`) leaves the header and footer empty.

## Dashboard (`js/dashboard.js`)

1. `GET /Dashboard`
2. Fills in the three stat cards (`#statTotal`, `#statBrands`, `#statExpiring`).
3. `renderBrandChart`: a Highcharts **bar** chart, sorted largest first.
4. `renderStatusChart`: a Highcharts **donut** chart, with colours matching the status badges.
5. `renderExpiringTable`: a table of assets whose warranty expires this month. Expired rows are marked.

If the request fails, a message appears in `#dashboardError`.

## Manage Assets (`js/manage.js`)

### Grid (AG Grid)

| Column           | Source field             | Renderer                         |
| ---------------- | ------------------------ | -------------------------------- |
| Serial Number    | `serialNumber`           | Text                             |
| Brand            | `brand`                  | Text                             |
| Status           | `status`                 | Coloured badge                   |
| Warranty Expires | `warrantyExpirationDate` | Formatted date, red if expired   |
| Actions          | –                        | View / Edit / Delete buttons     |

- Pagination: 10 rows per page (choices: 10, 20, 50)
- Every column has a filter. The search box sets AG Grid's `quickFilterText`
- The "N assets in inventory" counter shows the number of rows currently displayed, after filtering

### Dialogs

Dialogs use the native `<dialog>` element. `setupDialog()` connects the close buttons and closes the dialog when the backdrop is clicked.

| Dialog         | Opened by          | Behaviour                                                       |
| -------------- | ------------------ | --------------------------------------------------------------- |
| `#viewDialog`  | **View**           | Read-only details, fetched fresh with `getById`. Shows "Asset not found." on a 404 |
| `#assetDialog` | **+ Add Asset** / **Edit** | One shared form. `editingAsset` is `null` when adding |

**On save:**
- **Add** sends the form values to `POST /ITAsset/create`.
- **Edit** sends the form values to `PUT /ITAsset/update/{id}`.
- Both endpoints return the full list, which replaces the grid data directly.
- If the save fails, an error appears in the form. A 409 shows "An asset with this serial number already exists."; any other error shows a generic message.

**Delete:** asks for confirmation, calls `DELETE`, then reloads the list.

## Conventions

- User-entered values are written with `textContent`, never `innerHTML`, to prevent HTML injection.
- Status labels and colours are defined in `STATUS_LABELS`/`STATUS_COLORS` in the JS, and as `.status-<Value>` classes in the CSS. Keep the three in sync.
- Dates are shown with `toLocaleDateString` (for example, "31 May 2027").
- To change the API URL, edit `js/config.js` only.

## Adding a page

1. Create `Web/<name>.html` with `<div id="Header"></div>`, `<main class="page">…</main>` and `<footer id="Footer"></footer>`.
2. Include `css/style.css`, `js/config.js`, `js/layout.js` and your own `js/<name>.js`.
3. Add a link in `components/header.html`.
