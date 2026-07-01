// PEIA — Dashboard JS
// Mock data + Chart.js + Interactividad

document.addEventListener('DOMContentLoaded', () => {

  // ─── Datos mock ──────────────────────────────
  const movimientosData = {
    labels: ['13 May','14 May','15 May','16 May','17 May','18 May','19 May'],
    entradas: [520, 620, 480, 700, 690, 740, 800],
    salidas:  [400, 450, 390, 580, 510, 600, 660],
  };

  const topProductos = {
    labels: ['Tarima de Madera','Caja Plástica 60L','Pallet Plástico','Cinta Adhesiva','Film Stretch'],
    values: [1250, 1250, 760, 640, 520],
  };

  const estadosPedidos = {
    labels: ['Pendiente','En preparación','Enviado','Completado','Cancelado'],
    values:  [36, 19, 44, 18, 9],
    colors:  ['#f59e0b','#3b82f6','#22c55e','#6366f1','#ef4444'],
  };

  const prediccionData = {
    labels: ['19 May','23 May','28 May','02 Jun'],
    values: [400, 340, 600, 520],
  };

  const movimientos = [
    { fecha: '19/05/2025 11:32', tipo: 'Entrada', producto: 'Tarima de Madera', cantidad: 120, ubic: 'A-01-01' },
    { fecha: '19/05/2025 10:45', tipo: 'Salida',  producto: 'Caja Plástica 60L', cantidad: 45, ubic: 'B-03-03' },
    { fecha: '19/05/2025 09:18', tipo: 'Entrada', producto: 'Film Stretch',      cantidad: 80, ubic: 'C-07-02' },
    { fecha: '19/05/2025 08:05', tipo: 'Salida',  producto: 'Pallet Plástico',   cantidad: 20, ubic: 'A-02-03' },
  ];

  const pedidos = [
    { id: 'PED-0045', cliente: 'Distribuciones del Sur', estado: 'En preparación', fecha: '19/05 11:20' },
    { id: 'PED-0044', cliente: 'LogExpress S.A.',        estado: 'Pendiente',      fecha: '19/05 10:05' },
    { id: 'PED-0043', cliente: 'Almacenes Andinos',      estado: 'Enviado',        fecha: '19/05 08:15' },
    { id: 'PED-0042', cliente: 'Retail Corp',            estado: 'Pendiente',      fecha: '19/05 08:40' },
  ];

  const alertas = [
    { tipo: 'warn',  titulo: 'Stock bajo: Cinta Adhesiva 48mm', detalle: 'Quedan 25 unidades — mínimo 50' },
    { tipo: 'warn',  titulo: 'Temperatura elevada en Zona B',    detalle: 'Sensor 03 reporta 28°C' },
    { tipo: 'error', titulo: 'Movimiento no autorizado — Cámara 2', detalle: '19/05 03:42 AM — Área C-07' },
    { tipo: 'warn',  titulo: 'Producto agotado: Guantes de Nitrilo', detalle: 'Stock en 0 — Talla L' },
  ];

  // ─── ChartJS Global Config ────────────────────
  Chart.defaults.font.family = "'Inter', sans-serif";
  Chart.defaults.font.size   = 11;
  Chart.defaults.color       = '#9ca3af';

  // ─── Chart 1: Movimientos (línea) ─────────────
  new Chart(document.getElementById('chartMovimientos'), {
    type: 'line',
    data: {
      labels: movimientosData.labels,
      datasets: [
        {
          label: 'Entradas',
          data: movimientosData.entradas,
          borderColor: '#3b82f6',
          backgroundColor: 'rgba(59,130,246,.08)',
          tension: 0.35,
          fill: true,
          pointRadius: 4,
          pointBackgroundColor: '#3b82f6',
          borderWidth: 2,
        },
        {
          label: 'Salidas',
          data: movimientosData.salidas,
          borderColor: '#22c55e',
          backgroundColor: 'rgba(34,197,94,.06)',
          tension: 0.35,
          fill: true,
          pointRadius: 4,
          pointBackgroundColor: '#22c55e',
          borderWidth: 2,
        },
      ],
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: { legend: { display: false } },
      scales: {
        x: { grid: { display: false }, ticks: { maxTicksLimit: 7 } },
        y: { grid: { color: '#f3f4f6' }, border: { display: false }, ticks: { maxTicksLimit: 5 } },
      },
    },
  });

  // ─── Chart 2: Top productos (barras horizontales) ──
  new Chart(document.getElementById('chartTopProductos'), {
    type: 'bar',
    data: {
      labels: topProductos.labels,
      datasets: [{
        data: topProductos.values,
        backgroundColor: '#3b82f6',
        borderRadius: 4,
        barThickness: 14,
      }],
    },
    options: {
      indexAxis: 'y',
      responsive: true,
      maintainAspectRatio: false,
      plugins: { legend: { display: false } },
      scales: {
        x: { grid: { color: '#f3f4f6' }, border: { display: false }, ticks: { maxTicksLimit: 4 } },
        y: { grid: { display: false }, ticks: { font: { size: 11 } } },
      },
    },
  });

  // ─── Chart 3: Estados pedidos (donut) ──────────
  new Chart(document.getElementById('chartEstados'), {
    type: 'doughnut',
    data: {
      labels: estadosPedidos.labels,
      datasets: [{
        data: estadosPedidos.values,
        backgroundColor: estadosPedidos.colors,
        borderWidth: 0,
        hoverOffset: 6,
      }],
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      cutout: '65%',
      plugins: { legend: { display: false } },
    },
  });

  // Donut legend manual
  const legend = document.getElementById('donutLegend');
  estadosPedidos.labels.forEach((lbl, i) => {
    const total = estadosPedidos.values.reduce((a, b) => a + b, 0);
    const pct = Math.round(estadosPedidos.values[i] / total * 100);
    legend.innerHTML += `
      <div class="donut-legend-item">
        <span><i class="dot" style="background:${estadosPedidos.colors[i]};border-radius:50%;width:9px;height:9px;display:inline-block;margin-right:6px;"></i>${lbl}</span>
        <span style="font-weight:600;color:#374151">${estadosPedidos.values[i]} <span style="color:#9ca3af">(${pct}%)</span></span>
      </div>`;
  });

  // ─── Chart 4: Predicción demanda ────────────────
  new Chart(document.getElementById('chartPrediccion'), {
    type: 'line',
    data: {
      labels: prediccionData.labels,
      datasets: [{
        data: prediccionData.values,
        borderColor: '#7c3aed',
        backgroundColor: 'rgba(124,58,237,.08)',
        tension: 0.4,
        fill: true,
        borderDash: [5,3],
        pointRadius: 4,
        pointBackgroundColor: '#7c3aed',
        borderWidth: 2,
      }],
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: { legend: { display: false } },
      scales: {
        x: { grid: { display: false } },
        y: { grid: { color: '#f3f4f6' }, border: { display: false }, ticks: { maxTicksLimit: 4 } },
      },
    },
  });

  // ─── Tabla: Últimos movimientos ────────────────
  const tbodyMov = document.getElementById('tbodyMovimientos');
  movimientos.forEach(m => {
    const cls = m.tipo === 'Entrada' ? 'tipo-entrada' : 'tipo-salida';
    tbodyMov.innerHTML += `
      <tr>
        <td>${m.fecha}</td>
        <td><span class="${cls}">${m.tipo}</span></td>
        <td>${m.producto}</td>
        <td style="font-weight:600">${m.cantidad}</td>
        <td>${m.ubic}</td>
      </tr>`;
  });

  // ─── Tabla: Pedidos recientes ──────────────────
  const estadoPill = { 'En preparación': 'pill-blue', 'Pendiente': 'pill-orange', 'Enviado': 'pill-green', 'Cancelado': 'pill-red' };
  const tbodyPed = document.getElementById('tbodyPedidos');
  pedidos.forEach(p => {
    const pill = estadoPill[p.estado] || 'pill-blue';
    tbodyPed.innerHTML += `
      <tr>
        <td style="color:#1d4ed8;font-weight:600">${p.id}</td>
        <td>${p.cliente}</td>
        <td><span class="pill ${pill}">${p.estado}</span></td>
        <td>${p.fecha}</td>
      </tr>`;
  });

  // ─── Lista: Alertas recientes ──────────────────
  const alertsList = document.getElementById('alertsList');
  alertas.forEach(a => {
    const iconColor = a.tipo === 'error' ? '#dc2626' : '#ea580c';
    const iconPath = a.tipo === 'error'
      ? `<circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/>`
      : `<path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/>`;
    alertsList.innerHTML += `
      <li class="alert-item ${a.tipo}">
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="${iconColor}" stroke-width="2">${iconPath}</svg>
        <div class="alert-text">
          <strong>${a.titulo}</strong>
          ${a.detalle}
        </div>
      </li>`;
  });

  // ─── Selector de bodega ────────────────────────
  const warehouseSelector = document.querySelector('.warehouse-selector');
  warehouseSelector.addEventListener('click', (e) => {
    e.stopPropagation();
    warehouseSelector.classList.toggle('open');
  });

  document.querySelectorAll('.warehouse-opt').forEach(btn => {
    btn.addEventListener('click', (e) => {
      e.stopPropagation();
      document.querySelectorAll('.warehouse-opt').forEach(b => b.classList.remove('active'));
      btn.classList.add('active');
      document.getElementById('activeCentro').textContent = btn.textContent;
      warehouseSelector.classList.remove('open');
    });
  });

  document.addEventListener('click', () => warehouseSelector.classList.remove('open'));

  // ─── Cerrar sesión ─────────────────────────────
  document.getElementById('btnLogout').addEventListener('click', () => {
    localStorage.removeItem('peia_token');
    localStorage.removeItem('peia_user');
    window.location.href = '/login.html';
  });

  // ─── Nombre de usuario ─────────────────────────
  const user = JSON.parse(localStorage.getItem('peia_user') || '{}');
  if (user.nombreCompleto) {
    document.getElementById('userName').textContent = user.nombreCompleto;
    const initials = user.nombreCompleto.split(' ').map(n => n[0]).join('').slice(0, 2).toUpperCase();
    document.getElementById('userAvatar').textContent = initials;
  }

});
