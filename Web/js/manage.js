// Uses the API's { message } body when present (e.g. 409 duplicate serial number)
const apiError = async (response) => {
  const body = await response.json().catch(() => null);
  const err = new Error(body?.message ?? `Request failed (${response.status})`);
  err.status = response.status;
  return err;
};

const getList = async () => {
  const response = await fetch(`${API_Link}/ITAsset/List`);
  const data = await response.json();
  return data;
};

// Returns the asset, or null when the API responds 404
const getById = async (id) => {
  const response = await fetch(`${API_Link}/ITAsset/getById/${id}`);
  if (response.status === 404) return null;
  if (!response.ok) throw new Error(`Fetch failed (${response.status})`);
  const data = await response.json();
  return data;
}

const deleteById = async (id) => {
  const response = await fetch(`${API_Link}/ITAsset/delete/${id}`, {
    method: 'DELETE',
  });
  if (!response.ok && response.status !== 404) throw new Error(`Delete failed (${response.status})`);
  const data = await response.json();
  return data;
};

const createAsset = async (asset) => {
  const response = await fetch(`${API_Link}/ITAsset/create`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(asset)
  });
  if (!response.ok) throw await apiError(response);
  const data = await response.json();
  return data;
};

const editById = async (id, updatedAsset) => {
  const response = await fetch(`${API_Link}/ITAsset/update/${id}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(updatedAsset)
  });
  if (!response.ok) throw await apiError(response);
  const data = await response.json();
  return data;
};

let gridApi;

const STATUS_LABELS = {
  New: "New",
  InUse: "In Use",
  Damaged: "Damaged",
  Disposed: "Disposed",
};

const formatDate = (value) =>
  value ? new Date(value).toLocaleDateString(undefined, { year: "numeric", month: "short", day: "numeric" }) : "-";

// Wires up the close buttons and backdrop click for a dialog
const setupDialog = (dialog) => {
  dialog.querySelectorAll(".modal-close").forEach((btn) =>
    btn.addEventListener("click", () => dialog.close())
  );
  dialog.addEventListener("click", (e) => {
    if (e.target === dialog) dialog.close();
  });
  return dialog;
};

const viewDialog = setupDialog(document.querySelector("#viewDialog"));
// One form is shared by Add and Edit; editingAsset is null when adding
const assetDialog = setupDialog(document.querySelector("#assetDialog"));
const assetForm = document.querySelector("#assetForm");
const assetFormError = document.querySelector("#assetFormError");
let editingAsset = null;

const openAssetForm = (asset = null) => {
  editingAsset = asset;
  assetForm.reset();
  assetFormError.hidden = true;
  document.querySelector("#assetDialogTitle").textContent = asset ? "Edit Asset" : "Add Asset";

  if (asset) {
    assetForm.serialNumber.value = asset.serialNumber;
    assetForm.brand.value = asset.brand;
    assetForm.status.value = asset.status;
    // Date input expects yyyy-mm-dd
    assetForm.warrantyExpirationDate.value = asset.warrantyExpirationDate?.slice(0, 10) ?? "";
  }

  assetDialog.showModal();
};

document.querySelector("#createBtn").addEventListener("click", () => openAssetForm());

assetForm.addEventListener("submit", async (e) => {
  e.preventDefault();
  const submitBtn = assetForm.querySelector("[type=submit]");
  const formValues = Object.fromEntries(new FormData(assetForm));

  submitBtn.disabled = true;
  assetFormError.hidden = true;
  try {
    // Both endpoints return the full updated list
    const assetList = editingAsset
      ? await editById(editingAsset.id, formValues)
      : await createAsset(formValues);
    gridApi.setGridOption("rowData", assetList);
    assetDialog.close();
  } catch (err) {
    assetFormError.textContent = err.status === 409
      ? "An asset with this serial number already exists."
      : "Could not save the asset. Please try again.";
    assetFormError.hidden = false;
    console.error(err);
  } finally {
    submitBtn.disabled = false;
  }
});

const viewAsset = async (id) => {
  const asset = await getById(id);
  if (!asset) {
    alert("Asset not found.");
    return;
  }

  const warrantyExpired = new Date(asset.warrantyExpirationDate) < new Date();

  document.querySelector("#viewSerialNumber").textContent = asset.serialNumber;
  document.querySelector("#viewBrand").textContent = asset.brand;
  document.querySelector("#viewWarranty").textContent =
    formatDate(asset.warrantyExpirationDate) + (warrantyExpired ? " (expired)" : "");
  document.querySelector("#viewCreated").textContent = formatDate(asset.createdDate);
  document.querySelector("#viewId").textContent = asset.id;

  const statusEl = document.querySelector("#viewStatus");
  statusEl.textContent = STATUS_LABELS[asset.status] ?? asset.status;
  statusEl.className = `status-badge status-${asset.status}`;

  viewDialog.showModal();
};

const editAsset = async (id) => {
  const asset = await getById(id);
  if (!asset) {
    alert("Asset not found.");
    return;
  }
  openAssetForm(asset);
};

const deleteAsset = async (id) => {
  if (!confirm("Are you sure you want to delete this asset?")) return;
  await deleteById(id);
  gridApi.setGridOption("rowData", await getList());
};

// Renders the View / Edit / Delete buttons for each row
const actionsCellRenderer = (params) => {
  const container = document.createElement("div");
  container.className = "row-actions";
  const buttons = [
    { label: "View", className: "btn-view", onClick: viewAsset },
    { label: "Edit", className: "btn-edit", onClick: editAsset },
    { label: "Delete", className: "btn-delete", onClick: deleteAsset },
  ];

  buttons.forEach(({ label, className, onClick }) => {
    const btn = document.createElement("button");
    btn.className = `btn ${className}`;
    btn.textContent = label;
    btn.addEventListener("click", () => onClick(params.data.id));
    container.appendChild(btn);
  });

  return container;
};

const statusCellRenderer = (params) => {
  const badge = document.createElement("span");
  badge.className = `status-badge status-${params.value}`;
  badge.textContent = STATUS_LABELS[params.value] ?? params.value;
  return badge;
};

const warrantyCellRenderer = (params) => {
  const span = document.createElement("span");
  span.textContent = formatDate(params.value);
  if (new Date(params.value) < new Date()) {
    span.className = "warranty-expired";
    span.title = "Warranty expired";
  }
  return span;
};

const updateAssetCount = (params) => {
  document.querySelector("#assetCount").textContent = params.api.getDisplayedRowCount();
};

const loadGrid = async () => {
  let assetList = await getList();
  const gridOptions = {
    // Row Data: The data to be displayed.
    rowData: assetList,
    // Column Definitions: Defines the columns to be displayed.
    columnDefs: [
      { field: "serialNumber", headerName: "Serial Number" },
      { field: "brand" },
      { field: "status", cellRenderer: statusCellRenderer },
      { field: "createdDate", headerName: "Created At", valueFormatter: (params) => formatDate(params.value) },
      { field: "warrantyExpirationDate", headerName: "Warranty Expires", cellRenderer: warrantyCellRenderer },
      { headerName: "Actions", cellRenderer: actionsCellRenderer, sortable: false, filter: false, minWidth: 230 },
    ],
    defaultColDef: { flex: 1, minWidth: 140, filter: true },
    rowHeight: 48,
    pagination: true,
    paginationPageSize: 10,
    paginationPageSizeSelector: [10, 20, 50],
    overlayNoRowsTemplate: "No assets found",
    onModelUpdated: updateAssetCount,
  };

  const myGridElement = document.querySelector("#myGrid");
  gridApi = agGrid.createGrid(myGridElement, gridOptions);
};

document.querySelector("#searchInput").addEventListener("input", (e) => {
  gridApi?.setGridOption("quickFilterText", e.target.value);
});

loadGrid();
    