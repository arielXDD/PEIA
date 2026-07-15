// PEIA — Predicción JS
// Conectado a /api/predicciones (histórico, pronóstico ML.NET, resumen y detalle por producto)

document.addEventListener('DOMContentLoaded', async () => {
  if (!PEIA.requireAuth()) return;
  PEIA.hydrateShell();
  PEIA.bindWarehouseSelector();

  Chart.defaults.font.family = "'Inter', sans-serif";
  Chart.defaults.font.size = 11;
  Chart.defaults.color = '#9ca3af';

  let historico = { labels: [], valores: [] };
  let productosPred = [];

  let chartPrincipal = null;
  let chartCategorias = null;
  let chartReabastecer = null;
  let chartTendencia = null;

  async function renderPrincipal(periodo) {
    const prediccion = await PEIA.request(`/api/predicciones/prediccion?dias=${periodo}`);
    const ctx = document.getElementById('chartPrincipal').getContext('2d');
    if (chartPrincipal) chartPrincipal.destroy();

    const allLabels = [...historico.labels, ...prediccion.labels];
    const historicValues = [...historico.valores, ...Array(prediccion.labels.length).fill(null)];
    const predValues = [...Array(historico.labels.length).fill(null), ...prediccion.valores];

    const upperCI = predValues.map((v, i) => v !== null ? v + (prediccion.confianza[i - historico.labels.length] || 0) : null);
    const lowerCI = predValues.map((v, i) => v !== null ? Math.max(0, v - (prediccion.confianza[i - historico.labels.length] || 0)) : null);

    chartPrincipal = new Chart(ctx, {
      type: 'line',
      data: {
        labels: allLabels,
        datasets: [
          { label: 'Intervalo confianza', data: upperCI, borderColor: 'transparent', backgroundColor: 'rgba(124,58,237,.08)', fill: '+1', pointRadius: 0, tension: 0.35 },
          { label: 'Intervalo confianza', data: lowerCI, borderColor: 'transparent', backgroundColor: 'transparent', fill: false, pointRadius: 0 },
          { label: 'Histórico', data: historicValues, borderColor: '#3b82f6', backgroundColor: 'rgba(59,130,246,.08)', tension: 0.35, fill: true, pointRadius: 4, pointBackgroundColor: '#3b82f6', borderWidth: 2 },
          { label: 'Predicción', data: predValues, borderColor: '#7c3aed', backgroundColor: 'rgba(124,58,237,.06)', tension: 0.35, fill: true, pointRadius: 4, pointBackgroundColor: '#7c3aed', borderWidth: 2 },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: {
          x: { grid: { display: false }, ticks: { maxTicksLimit: 10 } },
          y: { grid: { color: '#f3f4f6' }, border: { display: false }, ticks: { maxTicksLimit: 5 } },
        },
      },
    });
  }

  function renderCategorias() {
    const porCategoria = {};
    productosPred.forEach(p => { porCategoria[p.cat] = (porCategoria[p.cat] || 0) + p.estimado; });
    const labels = Object.keys(porCategoria);

    if (chartCategorias) chartCategorias.destroy();
    chartCategorias = new Chart(document.getElementById('chartCategorias'), {
      type: 'bar',
      data: {
        labels,
        datasets: [{
          data: labels.map(l => porCategoria[l]),
          backgroundColor: ['#3b82f6', '#22c55e', '#f59e0b', '#ef4444', '#8b5cf6', '#0891b2'],
          borderRadius: 4,
          barThickness: 18,
        }],
      },
      options: {
        indexAxis: 'y',
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: {
          x: { grid: { color: '#f3f4f6' }, border: { display: false }, ticks: { maxTicksLimit: 4 } },
          y: { grid: { display: false }, ticks: { font: { size: 10 } } },
        },
      },
    });
  }

  function renderReabastecer() {
    const top = [...productosPred]
      .filter(p => p.recomendado > 0)
      .sort((a, b) => b.recomendado - a.recomendado)
      .slice(0, 5);

    if (chartReabastecer) chartReabastecer.destroy();
    chartReabastecer = new Chart(document.getElementById('chartReabastecer'), {
      type: 'bar',
      data: {
        labels: top.map(p => p.nombre),
        datasets: [{ data: top.map(p => p.recomendado), backgroundColor: '#ef4444', borderRadius: 4, barThickness: 14 }],
      },
      options: {
        indexAxis: 'y',
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: {
          x: { grid: { color: '#f3f4f6' }, border: { display: false }, ticks: { maxTicksLimit: 4 } },
          y: { grid: { display: false }, ticks: { font: { size: 10 } } },
        },
      },
    });
  }

  function computeTendenciaSemanal() {
    const dayNames = ['Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb'];
    const n = historico.valores.length;
    const today = new Date();
    today.setUTCHours(0, 0, 0, 0);
    const totals = new Array(7).fill(0);

    for (let i = 0; i < n; i++) {
      const daysAgo = n - i;
      const date = new Date(today);
      date.setUTCDate(date.getUTCDate() - daysAgo);
      totals[date.getUTCDay()] += historico.valores[i];
    }

    const order = [1, 2, 3, 4, 5, 6, 0];
    return { labels: order.map(d => dayNames[d]), values: order.map(d => totals[d]) };
  }

  function renderTendencia() {
    const tendencia = computeTendenciaSemanal();
    const colors = ['#3b82f6', '#22c55e', '#f59e0b', '#3b82f6', '#ef4444', '#22c55e', '#8b5cf6'];
    const total = tendencia.values.reduce((a, b) => a + b, 0) || 1;

    if (chartTendencia) chartTendencia.destroy();
    chartTendencia = new Chart(document.getElementById('chartTendencia'), {
      type: 'doughnut',
      data: { labels: tendencia.labels, datasets: [{ data: tendencia.values, backgroundColor: colors, borderWidth: 0, hoverOffset: 6 }] },
      options: { responsive: true, maintainAspectRatio: false, cutout: '65%', plugins: { legend: { display: false } } },
    });

    const legend = document.getElementById('donutLegendPred');
    legend.innerHTML = '';
    tendencia.labels.forEach((lbl, i) => {
      const pct = Math.round((tendencia.values[i] / total) * 100);
      legend.innerHTML += `
        <div class="donut-legend-item">
          <span><i class="dot" style="background:${colors[i]};border-radius:50%;width:9px;height:9px;display:inline-block;margin-right:6px;"></i>${lbl}</span>
          <span style="font-weight:600;color:#374151">${pct}% <span style="color:#9ca3af">del total</span></span>
        </div>`;
    });
  }

  function renderTabla() {
    const tbody = document.getElementById('tbodyPrediccion');
    tbody.innerHTML = '';
    productosPred.forEach(p => {
      tbody.innerHTML += `
        <tr>
          <td style="font-weight:500;color:#111827">${p.nombre}</td>
          <td>${p.cat}</td>
          <td style="font-weight:600">${p.stock}</td>
          <td>${p.estimado}</td>
          <td>
            ${p.recomendado > 0
              ? `<span style="color:#dc2626;font-weight:600">+${p.recomendado}</span>`
              : `<span style="color:#16a34a">Suficiente</span>`}
          </td>
          <td>
            <div style="display:flex;align-items:center;gap:6px">
              <div style="flex:1;max-width:60px;height:6px;background:#e5e7eb;border-radius:3px;overflow:hidden">
                <div style="width:${p.confianza}%;height:100%;background:${p.confianza >= 90 ? '#16a34a' : p.confianza >= 80 ? '#f59e0b' : '#ef4444'};border-radius:3px"></div>
              </div>
              ${p.confianza}%
            </div>
          </td>
        </tr>`;
    });
  }

  function renderKpis(resumen) {
    document.getElementById('kpi-demanda').textContent = resumen.demandaEstimada.toLocaleString('es-MX');
    document.getElementById('kpi-precision').textContent = `${resumen.precisionModelo.toFixed(1)}%`;
    document.getElementById('kpi-alertas').textContent = resumen.alertasActivas;
    const catEl = document.querySelector('.kpi-card:nth-child(4) .kpi-value');
    if (catEl) catEl.textContent = resumen.categoriasAnalizadas;
  }

  async function loadAll() {
    const [historicoData, resumen, productos] = await Promise.all([
      PEIA.request('/api/predicciones/historico'),
      PEIA.request('/api/predicciones/resumen'),
      PEIA.request('/api/predicciones'),
    ]);

    historico = historicoData;
    productosPred = productos.map(p => ({
      nombre: p.nombre,
      cat: p.categoria,
      stock: p.stockActual,
      estimado: p.demandaEstimada,
      recomendado: p.recomendado,
      confianza: p.confianza,
    }));

    renderKpis(resumen);
    await renderPrincipal(document.getElementById('periodoSelect').value);
    renderCategorias();
    renderReabastecer();
    renderTendencia();
    renderTabla();
  }

  document.getElementById('periodoSelect').addEventListener('change', (e) => {
    renderPrincipal(e.target.value).catch(err => PEIA.toast.error(err.message));
  });

  try {
    await loadAll();
  } catch (error) {
    PEIA.toast.error(error.message);
  }
});
