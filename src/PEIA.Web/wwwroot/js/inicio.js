document.addEventListener('DOMContentLoaded', async () => {
  if (!window.PEIA?.requireAuth()) return;

  const charts = {};
  const colors = ['#f59e0b', '#3b82f6', '#22c55e', '#6366f1', '#ef4444'];
  let refreshTimer = null;

  Chart.defaults.font.family = "'Inter', sans-serif";
  Chart.defaults.font.size = 11;
  Chart.defaults.color = '#64748b';

  const formatDate = value => value
    ? new Intl.DateTimeFormat('es-MX', { dateStyle: 'short', timeStyle: 'short' }).format(new Date(value))
    : '—';

  const dateKey = value => new Date(value).toISOString().slice(0, 10);
  const escapeHtml = value => String(value ?? '')
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#039;');

  function replaceChart(name, elementId, config) {
    charts[name]?.destroy();
    const canvas = document.getElementById(elementId);
    if (canvas) charts[name] = new Chart(canvas, config);
  }

  function renderMovimientosChart(movimientos) {
    const days = Array.from({ length: 7 }, (_, index) => {
      const date = new Date();
      date.setHours(0, 0, 0, 0);
      date.setDate(date.getDate() - (6 - index));
      return date;
    });
    const labels = days.map(day => new Intl.DateTimeFormat('es-MX', { day: '2-digit', month: 'short' }).format(day));
    const totals = type => days.map(day => movimientos
      .filter(item => dateKey(item.fecha) === dateKey(day) && String(item.tipo).toLowerCase() === type)
      .reduce((sum, item) => sum + Number(item.cantidad || 0), 0));

    replaceChart('movimientos', 'chartMovimientos', {
      type: 'line',
      data: { labels, datasets: [
        { label: 'Entradas', data: totals('entrada'), borderColor: '#3b82f6', backgroundColor: 'rgba(59,130,246,.08)', tension: .35, fill: true },
        { label: 'Salidas', data: totals('salida'), borderColor: '#22c55e', backgroundColor: 'rgba(34,197,94,.06)', tension: .35, fill: true }
      ] },
      options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { display: false } }, scales: { x: { grid: { display: false } }, y: { beginAtZero: true } } }
    });
  }

  function renderTopProductos(movimientos) {
    const totals = movimientos.reduce((map, item) => {
      const product = item.producto || 'Sin producto';
      map[product] = (map[product] || 0) + Number(item.cantidad || 0);
      return map;
    }, {});
    const top = Object.entries(totals).sort((a, b) => b[1] - a[1]).slice(0, 5);
    replaceChart('top', 'chartTopProductos', {
      type: 'bar',
      data: { labels: top.map(item => item[0]), datasets: [{ data: top.map(item => item[1]), backgroundColor: '#3b82f6', borderRadius: 4 }] },
      options: { indexAxis: 'y', responsive: true, maintainAspectRatio: false, plugins: { legend: { display: false } }, scales: { x: { beginAtZero: true }, y: { grid: { display: false } } } }
    });
  }

  function renderEstados(report) {
    const states = report.estados || {};
    const labels = ['Creado', 'Asignado', 'En ruta', 'Entregado', 'Cancelado'];
    const values = [states.creado, states.asignado, states.enRuta, states.entregado, states.cancelado].map(value => Number(value || 0));
    replaceChart('estados', 'chartEstados', {
      type: 'doughnut',
      data: { labels, datasets: [{ data: values, backgroundColor: colors, borderWidth: 0 }] },
      options: { responsive: true, maintainAspectRatio: false, cutout: '65%', plugins: { legend: { display: false } } }
    });
    const total = values.reduce((sum, value) => sum + value, 0);
    document.querySelector('.donut-card .chart-subtitle').textContent = `Total: ${total}`;
    document.getElementById('donutLegend').innerHTML = labels.map((label, index) => `
      <div class="donut-legend-item"><span><i class="dot" style="background:${colors[index]};border-radius:50%;width:9px;height:9px;display:inline-block;margin-right:6px"></i>${label}</span><strong>${values[index]}</strong></div>`).join('');
  }

  function renderPrediccion(prediction) {
    replaceChart('prediccion', 'chartPrediccion', {
      type: 'line',
      data: { labels: prediction.labels || [], datasets: [{ data: prediction.valores || [], borderColor: '#7c3aed', backgroundColor: 'rgba(124,58,237,.08)', tension: .4, fill: true, borderDash: [5, 3] }] },
      options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { display: false } }, scales: { x: { grid: { display: false } }, y: { beginAtZero: true } } }
    });
  }

  function renderTables(movimientos, pedidos, notificaciones) {
    document.getElementById('tbodyMovimientos').innerHTML = movimientos.slice(0, 6).map(item => `
      <tr><td>${formatDate(item.fecha)}</td><td><span class="${String(item.tipo).toLowerCase() === 'entrada' ? 'tipo-entrada' : 'tipo-salida'}">${escapeHtml(item.tipo)}</span></td><td>${escapeHtml(item.producto)}</td><td>${Number(item.cantidad || 0).toLocaleString('es-MX')}</td><td>${escapeHtml(item.referencia || '—')}</td></tr>`).join('') || '<tr><td colspan="5" class="empty-state">Sin movimientos recientes</td></tr>';

    const pill = { creado: 'pill-orange', asignado: 'pill-blue', enruta: 'pill-blue', entregado: 'pill-green', cancelado: 'pill-red' };
    document.getElementById('tbodyPedidos').innerHTML = pedidos.slice(0, 6).map(item => `
      <tr><td>${escapeHtml(item.codigo)}</td><td>${escapeHtml(item.cliente)}</td><td><span class="pill ${pill[String(item.estado).toLowerCase()] || 'pill-blue'}">${escapeHtml(item.estado)}</span></td><td>${formatDate(item.fechaPedido)}</td></tr>`).join('') || '<tr><td colspan="4" class="empty-state">Sin pedidos recientes</td></tr>';

    document.getElementById('alertsList').innerHTML = notificaciones.slice(0, 5).map(item => `
      <li class="alert-item ${item.tipo === 'error' ? 'error' : 'warn'}"><div class="alert-text"><strong>${escapeHtml(item.titulo)}</strong>${escapeHtml(item.descripcion || '')}</div></li>`).join('') || '<li class="empty-state">Sin alertas recientes</li>';
  }

  async function loadDashboard() {
    const centro = PEIA.getActiveCentro();
    if (!centro?.id) {
      PEIA.toast.error('Selecciona un centro para cargar el dashboard.');
      return;
    }
    const query = `centroId=${encodeURIComponent(centro.id)}`;
    const [inventory, movements, orderReport, orders, notifications, prediction] = await Promise.all([
      PEIA.request(`/api/reportes/inventario?${query}`),
      PEIA.request(`/api/reportes/movimientos?${query}`),
      PEIA.request(`/api/reportes/pedidos?${query}`),
      PEIA.request(`/api/pedidos?${query}`),
      PEIA.request(`/api/notificaciones?${query}`),
      PEIA.request('/api/predicciones/prediccion?dias=14')
    ]);

    const today = dateKey(new Date());
    const todayMovements = movements.filter(item => dateKey(item.fecha) === today);
    const pending = Number(orderReport.estados?.creado || 0) + Number(orderReport.estados?.asignado || 0);
    const unread = notifications.filter(item => !item.leida);
    const cameraIncidents = notifications.filter(item => String(item.tipo).toLowerCase().includes('camara')).length;

    document.getElementById('kpi-stock').textContent = Number(inventory.stockTotal || 0).toLocaleString('es-MX');
    document.getElementById('kpi-pedidos').textContent = pending.toLocaleString('es-MX');
    document.getElementById('kpi-alertas').textContent = unread.length.toLocaleString('es-MX');
    document.getElementById('kpi-entradas').textContent = todayMovements.filter(item => String(item.tipo).toLowerCase() === 'entrada').reduce((sum, item) => sum + Number(item.cantidad || 0), 0).toLocaleString('es-MX');
    document.getElementById('kpi-salidas').textContent = todayMovements.filter(item => String(item.tipo).toLowerCase() === 'salida').reduce((sum, item) => sum + Number(item.cantidad || 0), 0).toLocaleString('es-MX');
    document.getElementById('kpi-incidencias').textContent = cameraIncidents.toLocaleString('es-MX');
    document.querySelectorAll('.kpi-trend').forEach(item => { item.className = 'kpi-trend'; item.textContent = 'Datos actuales'; });
    const badge = document.querySelector('a[href="/Notificaciones"] .badge');
    if (badge) badge.textContent = unread.length;

    renderMovimientosChart(movements);
    renderTopProductos(movements);
    renderEstados(orderReport);
    renderPrediccion(prediction);
    renderTables(movements, orders, notifications);
  }

  const scheduleRefresh = () => {
    window.clearTimeout(refreshTimer);
    refreshTimer = window.setTimeout(() => loadDashboard().catch(error => PEIA.toast.error(error.message)), 350);
  };

  window.addEventListener('peia:centro-changed', scheduleRefresh);

  try {
    await loadDashboard();
    const hub = await PEIA.connectHub();
    hub?.on('notificacion', notification => {
      PEIA.toast.info(notification.titulo || 'Se recibió una actualización.');
      scheduleRefresh();
    });
    hub?.onreconnected(scheduleRefresh);
  } catch (error) {
    PEIA.toast.error(`No fue posible cargar el dashboard: ${error.message}`);
  }
});
