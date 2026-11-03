const getDashboard = async () => {
  const response = await fetch(`${API_Link}/Dashboard`);
  if (!response.ok) throw new Error(`Dashboard request failed (${response.status})`);
  const data = await response.json();
  return data;
};

const STATUS_LABELS = {
  New: "New",
  InUse: "In Use",
  Damaged: "Damaged",
  Disposed: "Disposed",
};

// Same colors as the status badges in style.css
const STATUS_COLORS = {
  New: "#3b82f6",
  InUse: "#22c55e",
  Damaged: "#f59e0b",
  Disposed: "#9ca3af",
};

const formatDate = (value) =>
  value ? new Date(value).toLocaleDateString(undefined, { year: "numeric", month: "short", day: "numeric" }) : "-";

Highcharts.setOptions({
  chart: { style: { fontFamily: '"Segoe UI", Arial, sans-serif' } },
  title: { text: null },
  credits: { enabled: false },
});

const renderBrandChart = (assetsByBrand) => {
  // Largest brand first
  const sorted = [...assetsByBrand].sort((a, b) => b.count - a.count);

  Highcharts.chart("brandChart", {
    chart: { type: "bar" },
    xAxis: { categories: sorted.map((b) => b.brand), title: { text: null } },
    yAxis: { min: 0, allowDecimals: false, title: { text: "Assets" } },
    legend: { enabled: false },
    tooltip: { pointFormat: "<b>{point.y}</b> assets" },
    plotOptions: { bar: { borderRadius: 4, dataLabels: { enabled: true } } },
    series: [{ name: "Assets", color: "#3b82f6", data: sorted.map((b) => b.count) }],
  });
};

const renderStatusChart = (assetsByStatus) => {
  Highcharts.chart("statusChart", {
    chart: { type: "pie" },
    tooltip: { pointFormat: "<b>{point.y}</b> assets ({point.percentage:.0f}%)" },
    plotOptions: {
      pie: {
        innerSize: "55%",
        showInLegend: true,
        dataLabels: { enabled: true, format: "{point.name}: {point.y}" },
      },
    },
    series: [{
      name: "Assets",
      data: assetsByStatus.map((s) => ({
        name: STATUS_LABELS[s.status] ?? s.status,
        y: s.count,
        color: STATUS_COLORS[s.status],
      })),
    }],
  });
};

const renderExpiringTable = (assets) => {
  const body = document.querySelector("#expiringBody");
  body.innerHTML = "";

  if (assets.length === 0) {
    body.innerHTML = `<tr><td colspan="4" class="table-empty">No warranties expire this month</td></tr>`;
    return;
  }

  assets.forEach((asset) => {
    const row = document.createElement("tr");
    const warrantyExpired = new Date(asset.warrantyExpirationDate) < new Date();

    // textContent keeps user-entered values from being parsed as HTML
    [asset.serialNumber, asset.brand].forEach((value) => {
      const cell = document.createElement("td");
      cell.textContent = value;
      row.appendChild(cell);
    });

    const statusCell = document.createElement("td");
    const badge = document.createElement("span");
    badge.className = `status-badge status-${asset.status}`;
    badge.textContent = STATUS_LABELS[asset.status] ?? asset.status;
    statusCell.appendChild(badge);
    row.appendChild(statusCell);

    const warrantyCell = document.createElement("td");
    warrantyCell.textContent = formatDate(asset.warrantyExpirationDate) + (warrantyExpired ? " (expired)" : "");
    if (warrantyExpired) warrantyCell.className = "warranty-expired";
    row.appendChild(warrantyCell);

    body.appendChild(row);
  });
};

const loadDashboard = async () => {
  try {
    const data = await getDashboard();

    document.querySelector("#statTotal").textContent = data.totalAssets;
    document.querySelector("#statBrands").textContent = data.assetsByBrand.length;
    document.querySelector("#statExpiring").textContent = data.warrantyExpiringThisMonth.length;

    renderBrandChart(data.assetsByBrand);
    renderStatusChart(data.assetsByStatus);
    renderExpiringTable(data.warrantyExpiringThisMonth);
  } catch (err) {
    const error = document.querySelector("#dashboardError");
    error.textContent = "Could not load dashboard data. Make sure the API is running.";
    error.hidden = false;
    document.querySelector("#expiringBody").innerHTML = `<tr><td colspan="4" class="table-empty">-</td></tr>`;
    console.error(err);
  }
};

loadDashboard();
