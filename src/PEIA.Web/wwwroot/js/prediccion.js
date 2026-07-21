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

  // ─── EXPORTACIONES ────────────────────────────────────────────────────────
  function exportXlsx(title, headers, rows) {
    if (typeof XLSX === 'undefined') {
      PEIA.toast.error('No se pudo cargar la librería de Excel. Revisa tu conexión e inténtalo de nuevo.');
      return;
    }
    const wsData = [headers, ...rows];
    const ws = XLSX.utils.aoa_to_sheet(wsData);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, title.slice(0, 31));
    XLSX.writeFile(wb, `PEIA_${title.replace(/\s/g, '_')}.xlsx`);
  }

  function exportPdf(title, headers, rows, horizontal) {
    if (!window.jspdf?.jsPDF) {
      PEIA.toast.error('No se pudo cargar el generador PDF. Revisa tu conexión e inténtalo de nuevo.');
      return;
    }

    const { jsPDF } = window.jspdf;
    const doc = new jsPDF({ orientation: horizontal ? 'landscape' : 'portrait' });
    doc.setFontSize(16);
    doc.text(`PEIA - ${title}`, 14, 20);
    doc.setFontSize(10);
    doc.setTextColor(110);
    doc.text(`Generado: ${new Date().toLocaleString('es-MX')}`, 14, 28);

    if (typeof doc.autoTable === 'function') {
      doc.autoTable({ head: [headers], body: rows, startY: 34, styles: { fontSize: 9, cellPadding: 3 }, headStyles: { fillColor: [29, 78, 216] } });
    } else {
      doc.setTextColor(30);
      rows.slice(0, 35).forEach((row, index) => doc.text(row.join(' | '), 14, 38 + (index * 6), { maxWidth: 180 }));
    }

    doc.save(`PEIA_${title.replace(/\s/g, '_')}.pdf`);
  }

  function printExecutiveReport(reportTitle) {
    const printWindow = window.open('', '_blank');
    if (!printWindow) {
      PEIA.toast.error('El navegador bloqueó la ventana de impresión. Permite ventanas emergentes e inténtalo de nuevo.');
      return;
    }

    const chartBase64 = chartPrincipal?.toBase64Image?.() || '';
    const escape = value => String(value ?? '').replace(/[&<>"']/g, char => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' })[char]);
    
    const headers = ['Producto', 'Categoría', 'Stock actual', 'Demanda', 'Recomendado', 'Confianza'];
    const rows = productosPred.map(p => [p.nombre, p.cat, p.stock, p.estimado, p.recomendado > 0 ? `+${p.recomendado}` : 'Suficiente', `${p.confianza}%`]);
    
    const tableRowsHtml = rows.map(row => `<tr>${row.map(value => `<td>${escape(value)}</td>`).join('')}</tr>`).join('');
    const tableHeadersHtml = headers.map(header => `<th>${escape(header)}</th>`).join('');
    const generatedAt = new Intl.DateTimeFormat('es-MX', { dateStyle: 'long', timeStyle: 'short' }).format(new Date());

    printWindow.document.write(`<!doctype html><html lang="es"><head><meta charset="utf-8"><title>${escape(reportTitle)}</title><style>
      @page { size: A4 portrait; margin: 12mm; }
      * { box-sizing: border-box; } body { color:#172033; font-family: Georgia, 'Times New Roman', serif; margin:0; font-size:10pt; }
      .masthead { display:flex; justify-content:space-between; align-items:flex-start; border-bottom:2px solid #15233f; padding-bottom:12px; margin-bottom:16px; }
      .brand { font:700 22pt/1.1 Georgia,serif; color:#15233f; letter-spacing:-.04em; } .eyebrow { color:#5d6b82; font:700 8pt/1.2 Arial,sans-serif; letter-spacing:.14em; text-transform:uppercase; margin-bottom:5px; }
      h1 { margin:0; font-size:20pt; letter-spacing:-.025em; } .meta { text-align:right; color:#5d6b82; font:9pt/1.45 Arial,sans-serif; }
      .chart { width:100%; border:1px solid #d7dce5; padding:16px 20px 12px; margin:0 0 20px; background:#fff; page-break-inside:avoid; } .chart img { display:block; width:100%; max-height:280px; object-fit:contain; }
      .section-title { margin:0 0 8px; padding-top:8px; border-top:1px solid #ccd3df; color:#1d3154; font:700 11pt Arial,sans-serif; text-transform:uppercase; letter-spacing:.08em; }
      table { width:100%; border-collapse:collapse; font:8.5pt/1.35 Arial,sans-serif; } th { background:#15233f; color:#fff; text-align:left; padding:8px; font-size:8pt; letter-spacing:.03em; text-transform:uppercase; } td { border-bottom:1px solid #dde2ea; padding:7px 8px; vertical-align:top; } tr:nth-child(even) td { background:#f7f9fb; }
      .footer { margin-top:12px; padding-top:8px; border-top:1px solid #ccd3df; display:flex; justify-content:space-between; color:#62708a; font:8pt Arial,sans-serif; }
    </style></head><body><header class="masthead"><div><div class="eyebrow">PEIA · Informe operativo</div><div class="brand">PEIA</div><h1>${escape(reportTitle)}</h1></div><div class="meta">Centro: ${escape(PEIA.getActiveCentro()?.nombre || 'No seleccionado')}<br>Emitido: ${escape(generatedAt)}</div></header><main>${chartBase64 ? `<figure class="chart"><img src="${chartBase64}" alt="Gráfica del reporte"></figure>` : ''}<h2 class="section-title">Detalle de predicción</h2><table><thead><tr>${tableHeadersHtml}</tr></thead><tbody>${tableRowsHtml || `<tr><td colspan="${headers.length}">No hay registros para mostrar.</td></tr>`}</tbody></table></main><footer class="footer"><span>Documento generado por PEIA</span><span>Uso interno</span></footer></body></html>`);
    printWindow.document.close();
    printWindow.focus();
    printWindow.onafterprint = () => printWindow.close();
    printWindow.setTimeout(() => { printWindow.print(); }, 250);
  }

  document.getElementById('exportPredPdf')?.addEventListener('click', (e) => {
    e.preventDefault();
    const headers = ['Producto', 'Categoría', 'Stock actual', 'Demanda estimada', 'Recomendado', 'Confianza (%)'];
    const rows = productosPred.map(p => [p.nombre, p.cat, p.stock, p.estimado, p.recomendado > 0 ? `+${p.recomendado}` : 'Suficiente', p.confianza]);
    exportPdf('Reporte Prediccion', headers, rows, false);
  });

  document.getElementById('exportPredExcel')?.addEventListener('click', (e) => {
    e.preventDefault();
    const headers = ['Producto', 'Categoría', 'Stock actual', 'Demanda estimada', 'Recomendado', 'Confianza (%)'];
    const rows = productosPred.map(p => [p.nombre, p.cat, p.stock, p.estimado, p.recomendado > 0 ? `+${p.recomendado}` : 'Suficiente', p.confianza]);
    exportXlsx('Reporte Prediccion', headers, rows);
  });

  document.getElementById('printPred')?.addEventListener('click', (e) => {
    e.preventDefault();
    printExecutiveReport('Reporte de Predicción');
  });

  document.getElementById('periodoSelect').addEventListener('change', (e) => {
    renderPrincipal(e.target.value).catch(err => PEIA.toast.error(err.message));
  });

  try {
    await loadAll();
  } catch (error) {
    PEIA.toast.error(error.message);
  }
});
