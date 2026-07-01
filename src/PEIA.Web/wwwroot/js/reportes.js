// PEIA — Reportes JS
// Tabs, Charts, DataTables, PDF/Excel export

document.addEventListener('DOMContentLoaded', () => {

  // ─── Chart.js global ────────────────────────
  Chart.defaults.font.family = "'Inter', sans-serif";
  Chart.defaults.font.size = 11;
  Chart.defaults.color = '#9ca3af';

  // ─── Mock Data ──────────────────────────────

  // Inventory report
  const invData = [
    { producto: 'Tarima de Madera',     categoria: 'Almacenamiento', centro: 'Bodega Norte', stock: 320, minimo: 50,  valor: 144000 },
    { producto: 'Caja Plástica 60L',    categoria: 'Almacenamiento', centro: 'Bodega Norte', stock: 185, minimo: 40,  valor: 22200  },
    { producto: 'Pallet Plástico',      categoria: 'Almacenamiento', centro: 'Bodega Sur',   stock: 95,  minimo: 30,  valor: 74100  },
    { producto: 'Cinta Adhesiva 48mm',  categoria: 'Embalaje',       centro: 'Bodega Norte', stock: 25,  minimo: 50,  valor: 875    },
    { producto: 'Film Stretch',         categoria: 'Embalaje',       centro: 'Bodega Sur',   stock: 150, minimo: 30,  valor: 14250  },
    { producto: 'Guantes de Nitrilo L', categoria: 'Seguridad',      centro: 'Bodega Norte', stock: 0,   minimo: 100, valor: 0      },
    { producto: 'Casco de Seguridad',   categoria: 'Seguridad',      centro: 'Bodega Sur',   stock: 42,  minimo: 20,  valor: 3570   },
    { producto: 'Etiquetas Código',     categoria: 'Embalaje',       centro: 'Bodega Norte', stock: 2400,minimo: 500, valor: 19200  },
    { producto: 'Escáner de Código',    categoria: 'Herramientas',   centro: 'Bodega Sur',   stock: 12,  minimo: 5,   valor: 14400  },
    { producto: 'Desengrasante Ind.',   categoria: 'Limpieza',       centro: 'Bodega Norte', stock: 18,  minimo: 25,  valor: 1170   },
  ];

  // Movements report
  const movData = [
    { fecha: '01/07/2026', tipo: 'Entrada', producto: 'Tarima de Madera',   cantidad: 120, ubicacion: 'A-01-01', usuario: 'admin' },
    { fecha: '01/07/2026', tipo: 'Salida',  producto: 'Caja Plástica 60L',  cantidad: 45,  ubicacion: 'B-03-03', usuario: 'inventario' },
    { fecha: '01/07/2026', tipo: 'Entrada', producto: 'Film Stretch',       cantidad: 80,  ubicacion: 'C-07-02', usuario: 'inventario' },
    { fecha: '30/06/2026', tipo: 'Salida',  producto: 'Pallet Plástico',    cantidad: 20,  ubicacion: 'A-02-03', usuario: 'logistica' },
    { fecha: '30/06/2026', tipo: 'Entrada', producto: 'Etiquetas Código',   cantidad: 500, ubicacion: 'C-05-01', usuario: 'admin' },
    { fecha: '29/06/2026', tipo: 'Salida',  producto: 'Cinta Adhesiva 48mm',cantidad: 30,  ubicacion: 'C-01-02', usuario: 'inventario' },
    { fecha: '29/06/2026', tipo: 'Entrada', producto: 'Casco de Seguridad', cantidad: 15,  ubicacion: 'D-01-03', usuario: 'inventario' },
    { fecha: '28/06/2026', tipo: 'Salida',  producto: 'Film Stretch',       cantidad: 60,  ubicacion: 'C-07-02', usuario: 'logistica' },
    { fecha: '28/06/2026', tipo: 'Entrada', producto: 'Desengrasante Ind.', cantidad: 24,  ubicacion: 'F-03-02', usuario: 'admin' },
    { fecha: '27/06/2026', tipo: 'Salida',  producto: 'Tarima de Madera',   cantidad: 50,  ubicacion: 'A-01-01', usuario: 'logistica' },
    { fecha: '27/06/2026', tipo: 'Entrada', producto: 'Caja Plástica 60L',  cantidad: 100, ubicacion: 'B-03-03', usuario: 'inventario' },
    { fecha: '26/06/2026', tipo: 'Salida',  producto: 'Escáner de Código',  cantidad: 2,   ubicacion: 'E-01-01', usuario: 'admin' },
  ];

  // Orders report
  const pedData = [
    { pedido: 'PED-0045', cliente: 'Distribuciones del Sur', estado: 'En preparación', fecha: '01/07/2026', items: 8,  total: 12500 },
    { pedido: 'PED-0044', cliente: 'LogExpress S.A.',        estado: 'Pendiente',      fecha: '01/07/2026', items: 3,  total: 4200  },
    { pedido: 'PED-0043', cliente: 'Almacenes Andinos',      estado: 'Enviado',        fecha: '30/06/2026', items: 12, total: 28700 },
    { pedido: 'PED-0042', cliente: 'Retail Corp',            estado: 'Pendiente',      fecha: '30/06/2026', items: 5,  total: 8900  },
    { pedido: 'PED-0041', cliente: 'SuperMarket Plus',       estado: 'Completado',     fecha: '29/06/2026', items: 7,  total: 15300 },
    { pedido: 'PED-0040', cliente: 'Distribuciones del Sur', estado: 'Completado',     fecha: '28/06/2026', items: 4,  total: 6100  },
    { pedido: 'PED-0039', cliente: 'FarmaCentral',           estado: 'Enviado',        fecha: '28/06/2026', items: 9,  total: 21000 },
    { pedido: 'PED-0038', cliente: 'TechStore',              estado: 'Cancelado',      fecha: '27/06/2026', items: 2,  total: 3400  },
    { pedido: 'PED-0037', cliente: 'LogExpress S.A.',        estado: 'Completado',     fecha: '27/06/2026', items: 6,  total: 11800 },
    { pedido: 'PED-0036', cliente: 'Almacenes Andinos',      estado: 'Completado',     fecha: '26/06/2026', items: 10, total: 32000 },
  ];

  const estadoPill = { 'En preparación': 'status-info', 'Pendiente': 'status-pending', 'Enviado': 'status-active', 'Completado': 'status-active', 'Cancelado': 'status-critical' };

  // ─── Tabs ───────────────────────────────────
  document.querySelectorAll('.tab-btn').forEach(btn => {
    btn.addEventListener('click', () => {
      document.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
      document.querySelectorAll('.tab-panel').forEach(p => p.classList.remove('active'));
      btn.classList.add('active');
      document.getElementById('panel-' + btn.dataset.tab).classList.add('active');
    });
  });

  // ─── Tab 1: Inventory Report ────────────────
  // Chart
  const catGroups = {};
  invData.forEach(r => { catGroups[r.categoria] = (catGroups[r.categoria] || 0) + r.stock; });
  const catColors = ['#3b82f6', '#22c55e', '#f59e0b', '#7c3aed', '#0891b2'];

  new Chart(document.getElementById('chartInvCategoria'), {
    type: 'bar',
    data: {
      labels: Object.keys(catGroups),
      datasets: [{ data: Object.values(catGroups), backgroundColor: catColors.slice(0, Object.keys(catGroups).length), borderRadius: 6, barThickness: 32 }],
    },
    options: {
      responsive: true, maintainAspectRatio: false,
      plugins: { legend: { display: false } },
      scales: { x: { grid: { display: false } }, y: { grid: { color: '#f3f4f6' }, border: { display: false } } },
    },
  });

  // Table
  const invTable = new DataTable(document.getElementById('tableInvReport'), {
    columns: [
      { key: 'producto', label: 'Producto', render: v => `<span class="cell-primary">${v}</span>` },
      { key: 'categoria', label: 'Categoría', render: v => `<span class="tag">${v}</span>` },
      { key: 'centro', label: 'Centro' },
      { key: 'stock', label: 'Stock', render: v => `<span class="cell-bold">${v}</span>` },
      { key: 'minimo', label: 'Mínimo' },
      { key: 'valor', label: 'Valor ($)', render: v => `<span class="cell-bold">$${v.toLocaleString()}</span>` },
    ],
    data: invData,
    pageSize: 8,
  });

  document.getElementById('filterCentroInv').addEventListener('change', (e) => {
    const c = e.target.value;
    invTable.setData(c ? invData.filter(r => r.centro === c) : [...invData]);
  });

  // ─── Tab 2: Movements Report ────────────────
  const trendLabels = ['26/06','27/06','28/06','29/06','30/06','01/07'];
  const entradas = [0, 100, 24, 15, 500, 200];
  const salidas  = [2, 50, 60, 30, 20, 45];

  new Chart(document.getElementById('chartMovTrend'), {
    type: 'line',
    data: {
      labels: trendLabels,
      datasets: [
        { label: 'Entradas', data: entradas, borderColor: '#3b82f6', backgroundColor: 'rgba(59,130,246,.08)', tension: .35, fill: true, pointRadius: 4, pointBackgroundColor: '#3b82f6', borderWidth: 2 },
        { label: 'Salidas',  data: salidas,  borderColor: '#ef4444', backgroundColor: 'rgba(239,68,68,.06)',  tension: .35, fill: true, pointRadius: 4, pointBackgroundColor: '#ef4444', borderWidth: 2 },
      ],
    },
    options: {
      responsive: true, maintainAspectRatio: false,
      plugins: { legend: { position: 'top', labels: { usePointStyle: true, padding: 16 } } },
      scales: { x: { grid: { display: false } }, y: { grid: { color: '#f3f4f6' }, border: { display: false } } },
    },
  });

  new DataTable(document.getElementById('tableMovReport'), {
    columns: [
      { key: 'fecha', label: 'Fecha' },
      { key: 'tipo', label: 'Tipo', render: v => `<span class="${v === 'Entrada' ? 'tipo-entrada' : 'tipo-salida'}">${v}</span>` },
      { key: 'producto', label: 'Producto', render: v => `<span class="cell-primary">${v}</span>` },
      { key: 'cantidad', label: 'Cantidad', render: v => `<span class="cell-bold">${v}</span>` },
      { key: 'ubicacion', label: 'Ubicación', render: v => `<span class="cell-mono">${v}</span>` },
      { key: 'usuario', label: 'Usuario' },
    ],
    data: movData,
    pageSize: 8,
  });

  // ─── Tab 3: Orders Report ──────────────────
  const estadoCounts = {};
  pedData.forEach(p => { estadoCounts[p.estado] = (estadoCounts[p.estado] || 0) + 1; });
  const donutColors = { 'Pendiente': '#f59e0b', 'En preparación': '#3b82f6', 'Enviado': '#22c55e', 'Completado': '#6366f1', 'Cancelado': '#ef4444' };

  new Chart(document.getElementById('chartPedEstados'), {
    type: 'doughnut',
    data: {
      labels: Object.keys(estadoCounts),
      datasets: [{ data: Object.values(estadoCounts), backgroundColor: Object.keys(estadoCounts).map(k => donutColors[k] || '#9ca3af'), borderWidth: 0, hoverOffset: 6 }],
    },
    options: { responsive: true, maintainAspectRatio: false, cutout: '60%', plugins: { legend: { position: 'bottom', labels: { usePointStyle: true, padding: 10 } } } },
  });

  const pedTable = new DataTable(document.getElementById('tablePedReport'), {
    columns: [
      { key: 'pedido', label: 'Pedido', render: v => `<span class="cell-mono">${v}</span>` },
      { key: 'cliente', label: 'Cliente', render: v => `<span class="cell-primary">${v}</span>` },
      { key: 'estado', label: 'Estado', render: v => `<span class="status-badge ${estadoPill[v] || ''}">${v}</span>` },
      { key: 'fecha', label: 'Fecha' },
      { key: 'items', label: 'Items', render: v => `<span class="cell-bold">${v}</span>` },
      { key: 'total', label: 'Total ($)', render: v => `<span class="cell-bold">$${v.toLocaleString()}</span>` },
    ],
    data: pedData,
    pageSize: 8,
  });

  document.getElementById('filterEstadoPed').addEventListener('change', (e) => {
    const s = e.target.value;
    pedTable.setData(s ? pedData.filter(p => p.estado === s) : [...pedData]);
  });

  // ─── Export: PDF ────────────────────────────
  function exportPDF(title, headers, rows) {
    const { jsPDF } = window.jspdf;
    const doc = new jsPDF();
    doc.setFontSize(16);
    doc.text(`PEIA — ${title}`, 14, 20);
    doc.setFontSize(10);
    doc.setTextColor(150);
    doc.text(`Generado: ${new Date().toLocaleString('es-MX')}`, 14, 28);
    doc.autoTable({ head: [headers], body: rows, startY: 34, styles: { fontSize: 9, cellPadding: 3 }, headStyles: { fillColor: [29, 78, 216] } });
    doc.save(`PEIA_${title.replace(/\s/g, '_')}.pdf`);
  }

  function exportExcel(title, headers, rows) {
    const wsData = [headers, ...rows];
    const ws = XLSX.utils.aoa_to_sheet(wsData);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, title.slice(0, 31));
    XLSX.writeFile(wb, `PEIA_${title.replace(/\s/g, '_')}.xlsx`);
  }

  // Inventory exports
  document.getElementById('exportInvPdf').addEventListener('click', () => {
    exportPDF('Reporte Inventario', ['Producto','Categoría','Centro','Stock','Mínimo','Valor ($)'],
      invData.map(r => [r.producto, r.categoria, r.centro, r.stock, r.minimo, `$${r.valor.toLocaleString()}`]));
  });
  document.getElementById('exportInvExcel').addEventListener('click', () => {
    exportExcel('Inventario', ['Producto','Categoría','Centro','Stock','Mínimo','Valor ($)'],
      invData.map(r => [r.producto, r.categoria, r.centro, r.stock, r.minimo, r.valor]));
  });

  // Movements exports
  document.getElementById('exportMovPdf').addEventListener('click', () => {
    exportPDF('Reporte Movimientos', ['Fecha','Tipo','Producto','Cantidad','Ubicación','Usuario'],
      movData.map(r => [r.fecha, r.tipo, r.producto, r.cantidad, r.ubicacion, r.usuario]));
  });
  document.getElementById('exportMovExcel').addEventListener('click', () => {
    exportExcel('Movimientos', ['Fecha','Tipo','Producto','Cantidad','Ubicación','Usuario'],
      movData.map(r => [r.fecha, r.tipo, r.producto, r.cantidad, r.ubicacion, r.usuario]));
  });

  // Orders exports
  document.getElementById('exportPedPdf').addEventListener('click', () => {
    exportPDF('Reporte Pedidos', ['Pedido','Cliente','Estado','Fecha','Items','Total ($)'],
      pedData.map(r => [r.pedido, r.cliente, r.estado, r.fecha, r.items, `$${r.total.toLocaleString()}`]));
  });
  document.getElementById('exportPedExcel').addEventListener('click', () => {
    exportExcel('Pedidos', ['Pedido','Cliente','Estado','Fecha','Items','Total ($)'],
      pedData.map(r => [r.pedido, r.cliente, r.estado, r.fecha, r.items, r.total]));
  });

  // ─── Common: Warehouse selector, Logout ─────
  const ws = document.querySelector('.warehouse-selector');
  ws.addEventListener('click', (e) => { e.stopPropagation(); ws.classList.toggle('open'); });
  document.querySelectorAll('.warehouse-opt').forEach(btn => {
    btn.addEventListener('click', (e) => { e.stopPropagation(); document.querySelectorAll('.warehouse-opt').forEach(b => b.classList.remove('active')); btn.classList.add('active'); document.getElementById('activeCentro').textContent = btn.textContent; ws.classList.remove('open'); });
  });
  document.addEventListener('click', () => ws.classList.remove('open'));

  document.getElementById('btnLogout').addEventListener('click', () => { localStorage.removeItem('peia_token'); localStorage.removeItem('peia_user'); window.location.href = '/login.html'; });
  const user = JSON.parse(localStorage.getItem('peia_user') || '{}');
  if (user.nombreCompleto) { document.getElementById('userName').textContent = user.nombreCompleto; document.getElementById('userAvatar').textContent = user.nombreCompleto.split(' ').map(n => n[0]).join('').slice(0, 2).toUpperCase(); }
});
