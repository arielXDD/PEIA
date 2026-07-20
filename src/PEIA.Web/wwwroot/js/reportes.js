// PEIA — Reportes JS
// Conectado a /api/reportes, /api/inventario y /api/pedidos

document.addEventListener('DOMContentLoaded', async () => {
  if (!PEIA.requireAuth()) return;
  PEIA.hydrateShell();
  PEIA.bindWarehouseSelector();

  const chartsAvailable = typeof Chart !== 'undefined';
  if (chartsAvailable) {
    Chart.defaults.font.family = "'Inter', sans-serif";
    Chart.defaults.font.size = 11;
    Chart.defaults.color = '#9ca3af';
  }

  let productos = [];
  let movimientos = [];
  let pedidos = [];

  let chartInvCategoria = null;
  let chartMovTrend = null;
  let chartPedEstados = null;
  let invTable = null;
  let movTable = null;
  let pedTable = null;

  const today = new Date();
  const thirtyDaysAgo = new Date();
  thirtyDaysAgo.setDate(today.getDate() - 30);
  document.getElementById('dateFrom').value = thirtyDaysAgo.toISOString().slice(0, 10);
  document.getElementById('dateTo').value = today.toISOString().slice(0, 10);

  const estadoPill = {
    Creado: 'status-pending',
    Asignado: 'status-info',
    EnRuta: 'status-info',
    Entregado: 'status-active',
    Cancelado: 'status-critical',
  };
  const estadoLabel = {
    Creado: 'Creado',
    Asignado: 'Asignado',
    EnRuta: 'En ruta',
    Entregado: 'Entregado',
    Cancelado: 'Cancelado',
  };
  const donutColors = { Creado: '#f59e0b', Asignado: '#3b82f6', EnRuta: '#6366f1', Entregado: '#22c55e', Cancelado: '#ef4444' };

  // ─── Tabs ───────────────────────────────────
  document.querySelectorAll('.tab-btn').forEach(btn => {
    btn.addEventListener('click', () => {
      document.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
      document.querySelectorAll('.tab-panel').forEach(p => p.classList.remove('active'));
      btn.classList.add('active');
      document.getElementById('panel-' + btn.dataset.tab).classList.add('active');
    });
  });

  // ─── Tab 1: Inventario ──────────────────────
  async function loadInventario() {
    const centro = PEIA.getActiveCentro();
    if (!centro?.id) throw new Error('Selecciona un centro activo.');

    const [resumen, productosRaw] = await Promise.all([
      PEIA.request(`/api/reportes/inventario?centroId=${centro.id}`),
      PEIA.request(`/api/inventario/productos?centroId=${centro.id}`),
    ]);

    productos = productosRaw.map(p => ({
      producto: p.nombre,
      categoria: p.categoria,
      stock: p.stock,
      minimo: p.stockMinimo,
      valor: p.stock * Number(p.precioUnitario || 0),
    }));

    const filter = document.getElementById('filterCategoriaInv');
    const current = filter.value;
    const categoriasUnicas = [...new Set(productos.map(p => p.categoria))];
    filter.innerHTML = '<option value="">Todas las categorías</option>' +
      categoriasUnicas.map(c => `<option value="${c}">${c}</option>`).join('');
    filter.value = current;

    const catColors = ['#3b82f6', '#22c55e', '#f59e0b', '#7c3aed', '#0891b2', '#ef4444'];
    const categorias = resumen.categorias || [];
    if (chartsAvailable) {
      if (chartInvCategoria) chartInvCategoria.destroy();
      chartInvCategoria = new Chart(document.getElementById('chartInvCategoria'), {
        type: 'bar',
        data: {
          labels: categorias.map(c => c.nombre),
          datasets: [{ data: categorias.map(c => c.cantidad), backgroundColor: catColors.slice(0, categorias.length), borderRadius: 6, barThickness: 32 }],
        },
        options: {
          responsive: true, maintainAspectRatio: false,
          plugins: { legend: { display: false } },
          scales: { x: { grid: { display: false } }, y: { grid: { color: '#f3f4f6' }, border: { display: false } } },
        },
      });
    }

    if (!invTable) {
      invTable = new DataTable(document.getElementById('tableInvReport'), {
        columns: [
          { key: 'producto', label: 'Producto', render: v => `<span class="cell-primary">${v}</span>` },
          { key: 'categoria', label: 'Categoría', render: v => `<span class="tag">${v}</span>` },
          { key: 'stock', label: 'Stock', render: v => `<span class="cell-bold">${v}</span>` },
          { key: 'minimo', label: 'Mínimo' },
          { key: 'valor', label: 'Valor ($)', render: v => `<span class="cell-bold">$${v.toLocaleString('es-MX')}</span>` },
        ],
        data: productos,
        pageSize: 8,
      });
    } else {
      invTable.setData(productos);
    }

    applyInvFilter();
  }

  function applyInvFilter() {
    const cat = document.getElementById('filterCategoriaInv').value;
    invTable.setData(cat ? productos.filter(p => p.categoria === cat) : [...productos]);
  }

  document.getElementById('filterCategoriaInv').addEventListener('change', applyInvFilter);

  // ─── Tab 2: Movimientos ─────────────────────
  async function loadMovimientos() {
    const centro = PEIA.getActiveCentro();
    if (!centro?.id) throw new Error('Selecciona un centro activo.');

    const desde = document.getElementById('dateFrom').value;
    const hasta = document.getElementById('dateTo').value;
    const params = new URLSearchParams({ centroId: centro.id });
    if (desde) params.set('fechaInicio', desde);
    if (hasta) params.set('fechaFin', hasta);

    const movimientosRaw = await PEIA.request(`/api/reportes/movimientos?${params}`);
    movimientos = movimientosRaw.map(m => ({
      fecha: new Date(m.fecha).toLocaleDateString('es-MX'),
      fechaRaw: m.fecha,
      tipo: m.tipo,
      producto: m.producto,
      cantidad: m.cantidad,
      referencia: m.referencia || '-',
    }));

    const porDia = {};
    movimientos.forEach(m => {
      const dia = m.fecha;
      porDia[dia] = porDia[dia] || { entradas: 0, salidas: 0 };
      if (m.tipo === 'Entrada') porDia[dia].entradas += m.cantidad;
      else if (m.tipo === 'Salida') porDia[dia].salidas += m.cantidad;
    });
    const dias = Object.keys(porDia).sort((a, b) => new Date(a.split('/').reverse().join('-')) - new Date(b.split('/').reverse().join('-')));

    if (chartsAvailable) {
      if (chartMovTrend) chartMovTrend.destroy();
      chartMovTrend = new Chart(document.getElementById('chartMovTrend'), {
        type: 'line',
        data: {
          labels: dias,
          datasets: [
            { label: 'Entradas', data: dias.map(d => porDia[d].entradas), borderColor: '#3b82f6', backgroundColor: 'rgba(59,130,246,.08)', tension: .35, fill: true, pointRadius: 4, pointBackgroundColor: '#3b82f6', borderWidth: 2 },
            { label: 'Salidas', data: dias.map(d => porDia[d].salidas), borderColor: '#ef4444', backgroundColor: 'rgba(239,68,68,.06)', tension: .35, fill: true, pointRadius: 4, pointBackgroundColor: '#ef4444', borderWidth: 2 },
          ],
        },
        options: {
          responsive: true, maintainAspectRatio: false,
          plugins: { legend: { position: 'top', labels: { usePointStyle: true, padding: 16 } } },
          scales: { x: { grid: { display: false } }, y: { grid: { color: '#f3f4f6' }, border: { display: false } } },
        },
      });
    }

    if (!movTable) {
      movTable = new DataTable(document.getElementById('tableMovReport'), {
        columns: [
          { key: 'fecha', label: 'Fecha' },
          { key: 'tipo', label: 'Tipo', render: v => `<span class="${v === 'Entrada' ? 'tipo-entrada' : 'tipo-salida'}">${v}</span>` },
          { key: 'producto', label: 'Producto', render: v => `<span class="cell-primary">${v}</span>` },
          { key: 'cantidad', label: 'Cantidad', render: v => `<span class="cell-bold">${v}</span>` },
          { key: 'referencia', label: 'Referencia', render: v => `<span class="cell-mono">${v}</span>` },
        ],
        data: movimientos,
        pageSize: 8,
      });
    } else {
      movTable.setData(movimientos);
    }
  }

  document.getElementById('dateFrom').addEventListener('change', () => loadMovimientos().catch(err => PEIA.toast.error(err.message)));
  document.getElementById('dateTo').addEventListener('change', () => loadMovimientos().catch(err => PEIA.toast.error(err.message)));

  // ─── Tab 3: Pedidos ─────────────────────────
  async function loadPedidos() {
    const centro = PEIA.getActiveCentro();
    if (!centro?.id) throw new Error('Selecciona un centro activo.');

    const [resumen, pedidosRaw] = await Promise.all([
      PEIA.request(`/api/reportes/pedidos?centroId=${centro.id}`),
      PEIA.request(`/api/pedidos?centroId=${centro.id}`),
    ]);

    pedidos = pedidosRaw.map(p => ({
      pedido: p.codigo,
      cliente: p.cliente,
      estado: p.estado,
      fecha: new Date(p.fechaPedido).toLocaleDateString('es-MX'),
      fechaEstimada: new Date(p.fechaEstimadaEntrega).toLocaleDateString('es-MX'),
      sla: p.sla?.estadoSLA || '-',
    }));

    const estados = resumen.estados || {};
    const labels = Object.keys(estados).filter(k => estados[k] > 0);
    if (chartsAvailable) {
      if (chartPedEstados) chartPedEstados.destroy();
      chartPedEstados = new Chart(document.getElementById('chartPedEstados'), {
        type: 'doughnut',
        data: {
          labels: labels.map(l => estadoLabel[l] || l),
          datasets: [{ data: labels.map(l => estados[l]), backgroundColor: labels.map(l => donutColors[l] || '#9ca3af'), borderWidth: 0, hoverOffset: 6 }],
        },
        options: { responsive: true, maintainAspectRatio: false, cutout: '60%', plugins: { legend: { position: 'bottom', labels: { usePointStyle: true, padding: 10 } } } },
      });
    }

    if (!pedTable) {
      pedTable = new DataTable(document.getElementById('tablePedReport'), {
        columns: [
          { key: 'pedido', label: 'Pedido', render: v => `<span class="cell-mono">${v}</span>` },
          { key: 'cliente', label: 'Cliente', render: v => `<span class="cell-primary">${v}</span>` },
          { key: 'estado', label: 'Estado', render: v => `<span class="status-badge ${estadoPill[v] || ''}">${estadoLabel[v] || v}</span>` },
          { key: 'fecha', label: 'Fecha pedido' },
          { key: 'fechaEstimada', label: 'Entrega estimada' },
          { key: 'sla', label: 'SLA' },
        ],
        data: pedidos,
        pageSize: 8,
      });
    } else {
      pedTable.setData(pedidos);
    }

    applyPedFilter();
  }

  function applyPedFilter() {
    const s = document.getElementById('filterEstadoPed').value;
    pedTable.setData(s ? pedidos.filter(p => p.estado === s) : [...pedidos]);
  }

  document.getElementById('filterEstadoPed').addEventListener('change', applyPedFilter);

  // ─── Export Modal System ────────────────────────────────────

  // SVG icons
  const SVG_PDF_FILE = `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>`;
  const SVG_XLS_FILE = `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M4 4h16v16H4z"/><path d="M4 9h16"/><path d="M4 14h16"/><path d="M9 4v16"/><path d="M14 4v16"/></svg>`;

  // PDF step labels per report type
  const pdfStepLabels = {
    inv:  ['Recopilando datos del inventario...', 'Generando tabla de productos...', 'Finalizando formato PDF...'],
    mov:  ['Recopilando movimientos...', 'Renderizando gráficas de tendencia...', 'Finalizando formato PDF...'],
    ped:  ['Recopilando pedidos...', 'Procesando estados y SLA...', 'Finalizando formato PDF...'],
  };
  const excelStepLabels = {
    inv:  ['Recopilando datos del inventario...', 'Construyendo hoja de cálculo...', 'Finalizando archivo XLSX...'],
    mov:  ['Recopilando movimientos...', 'Aplicando formato de tabla...', 'Finalizando archivo XLSX...'],
    ped:  ['Recopilando pedidos...', 'Aplicando estilos y colores...', 'Finalizando archivo XLSX...'],
  };

  // State
  let _pendingExportFn = null;  // () => void — actual download fn
  let _retryFn = null;          // () => void — repeat current flow from progress
  let _cancelled = false;
  let _progressTimer = null;

  // Helpers
  function showOverlay(id)  { document.getElementById(id)?.classList.add('is-visible'); }
  function hideOverlay(id)  { document.getElementById(id)?.classList.remove('is-visible'); }
  function hideAllOverlays() {
    ['overlayPdf','overlayExcel','overlayProgress','overlaySuccess','overlayError','overlayPrint']
      .forEach(id => hideOverlay(id));
  }

  // Orientation selection (PDF config)
  document.getElementById('orientVertical')?.addEventListener('click', () => {
    document.getElementById('orientVertical').classList.add('selected');
    document.getElementById('orientHorizontal').classList.remove('selected');
  });
  document.getElementById('orientHorizontal')?.addEventListener('click', () => {
    document.getElementById('orientHorizontal').classList.add('selected');
    document.getElementById('orientVertical').classList.remove('selected');
  });

  // Close / Cancel buttons (config modals)
  ['closePdfConfig','cancelPdfConfig'].forEach(id =>
    document.getElementById(id)?.addEventListener('click', hideAllOverlays));
  ['closeExcelConfig','cancelExcelConfig'].forEach(id =>
    document.getElementById(id)?.addEventListener('click', hideAllOverlays));

  // Cancel / Close (other modals)
  document.getElementById('cancelProgress')?.addEventListener('click', () => {
    _cancelled = true;
    clearTimeout(_progressTimer);
    hideAllOverlays();
  });
  document.getElementById('closeSuccess')?.addEventListener('click', hideAllOverlays);
  document.getElementById('cancelError')?.addEventListener('click', hideAllOverlays);
  document.getElementById('retryExport')?.addEventListener('click', () => {
    hideOverlay('overlayError');
    if (_retryFn) _retryFn();
  });

  // Click outside to close config modals
  ['overlayPdf','overlayExcel'].forEach(id => {
    document.getElementById(id)?.addEventListener('click', e => {
      if (e.target === document.getElementById(id)) hideAllOverlays();
    });
  });

  // ── Progress animation ──────────────────────────────────────
  function runProgressAnimation(stepTexts, isExcel, onComplete) {
    _cancelled = false;

    // Setup spinner colour
    const spinner = document.getElementById('progressSpinner');
    spinner.className = 'progress-spinner' + (isExcel ? ' excel-spinner' : '');

    // Reset progress bar
    const bar = document.getElementById('progressBarFill');
    bar.className = 'progress-bar-fill' + (isExcel ? ' excel-fill' : '');
    bar.style.width = '0%';

    // Reset steps
    const steps = [
      document.getElementById('pstep1'),
      document.getElementById('pstep2'),
      document.getElementById('pstep3'),
    ];
    const activeClass = isExcel ? 'active excel-active' : 'active';
    steps.forEach((s, i) => {
      s.className = 'pstep';
      s.querySelector('.pstep-icon').innerHTML = `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="10"/></svg>`;
      document.getElementById(`pstep${i+1}text`).textContent = stepTexts[i];
    });

    const DONE_ICON = `<svg viewBox="0 0 24 24" fill="none" stroke="#16a34a" stroke-width="2.5"><polyline points="20 6 9 17 4 12"/></svg>`;
    const SPIN_ICON = `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" style="animation:spin .7s linear infinite"><circle cx="12" cy="12" r="10" stroke-dasharray="30 10"/></svg>`;

    const delays = [0, 900, 1900];  // ms when each step becomes active
    const barPcts = ['10%', '50%', '85%'];

    delays.forEach((delay, i) => {
      _progressTimer = setTimeout(() => {
        if (_cancelled) return;
        if (i > 0) {
          // mark previous as done
          steps[i-1].className = 'pstep done';
          steps[i-1].querySelector('.pstep-icon').innerHTML = DONE_ICON;
        }
        steps[i].className = `pstep ${activeClass}`;
        steps[i].querySelector('.pstep-icon').innerHTML = SPIN_ICON;
        bar.style.width = barPcts[i];
      }, delay);
    });

    _progressTimer = setTimeout(() => {
      if (_cancelled) return;
      // Mark last step done
      steps[2].className = 'pstep done';
      steps[2].querySelector('.pstep-icon').innerHTML = DONE_ICON;
      bar.style.width = '100%';

      setTimeout(() => {
        if (_cancelled) return;
        onComplete();
      }, 350);
    }, 3000);
  }

  // ── Open config modals ─────────────────────────────────────
  function openPdfConfig(reportKey, title, headers, getRows) {
    if (!document.getElementById('overlayPdf')) {
      exportCsv(title, headers, getRows());
      return;
    }
    // Update subtitle with report name
    document.querySelector('#emPdfTitle').textContent = `Exportar ${title} a PDF`;
    hideAllOverlays();
    showOverlay('overlayPdf');

    // Wire confirm button
    document.getElementById('confirmPdfConfig').onclick = () => {
      const horizontal = document.getElementById('orientHorizontal').classList.contains('selected');
      startExport({
        type: 'pdf',
        isExcel: false,
        reportKey,
        title,
        fileName: `PEIA_${title.replace(/\s/g,'_')}.pdf`,
        doExport: () => {
          const { jsPDF } = window.jspdf;
          const orientation = horizontal ? 'landscape' : 'portrait';
          const doc = new jsPDF({ orientation });
          doc.setFontSize(16);
          doc.text(`PEIA — ${title}`, 14, 20);
          doc.setFontSize(10);
          doc.setTextColor(150);
          doc.text(`Generado: ${new Date().toLocaleString('es-MX')}`, 14, 28);
          doc.autoTable({ head: [headers], body: getRows(), startY: 34, styles: { fontSize: 9, cellPadding: 3 }, headStyles: { fillColor: [29, 78, 216] } });
          doc.save(`PEIA_${title.replace(/\s/g,'_')}.pdf`);
        },
      });
    };
  }

  function openExcelConfig(reportKey, title, headers, getRows) {
    if (!document.getElementById('overlayExcel')) {
      exportCsv(title, headers, getRows());
      return;
    }
    document.querySelector('#emExcelTitle').textContent = `Exportar ${title} a Excel`;
    hideAllOverlays();
    showOverlay('overlayExcel');

    document.getElementById('confirmExcelConfig').onclick = () => {
      startExport({
        type: 'excel',
        isExcel: true,
        reportKey,
        title,
        fileName: `PEIA_${title.replace(/\s/g,'_')}.xlsx`,
        doExport: () => {
          const wsData = [headers, ...getRows()];
          const ws = XLSX.utils.aoa_to_sheet(wsData);
          const wb = XLSX.utils.book_new();
          XLSX.utils.book_append_sheet(wb, ws, title.slice(0, 31));
          XLSX.writeFile(wb, `PEIA_${title.replace(/\s/g,'_')}.xlsx`);
        },
      });
    };
  }

  function exportCsv(title, headers, rows) {
    const csvRows = [headers, ...rows].map(row =>
      row.map(value => `"${String(value ?? '').replaceAll('"', '""')}"`).join(',')
    );
    const blob = new Blob([csvRows.join('\n')], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `PEIA_${title.replace(/\s/g, '_')}.csv`;
    link.click();
    URL.revokeObjectURL(url);
  }

  // ── Core export flow ───────────────────────────────────────
  function startExport({ type, isExcel, reportKey, title, fileName, doExport }) {
    hideAllOverlays();
    showOverlay('overlayProgress');

    const stepLabels = isExcel ? excelStepLabels[reportKey] : pdfStepLabels[reportKey];

    _retryFn = () => {
      showOverlay('overlayProgress');
      run();
    };

    function run() {
      runProgressAnimation(stepLabels, isExcel, () => {
        try {
          doExport();
          // Show success
          hideOverlay('overlayProgress');

          // File card
          const iconEl = document.getElementById('successFileIcon');
          iconEl.className = `success-file-icon ${isExcel ? 'excel-icon' : 'pdf-icon'}`;
          iconEl.innerHTML = isExcel ? SVG_XLS_FILE : SVG_PDF_FILE;

          document.getElementById('successFileName').textContent = fileName;
          document.getElementById('successFileMeta').textContent = `Generado hace un momento`;
          document.getElementById('successDesc').textContent =
            isExcel
              ? `Tu hoja de cálculo está lista para descargar. Contiene el reporte de ${title}.`
              : `Tu documento PDF está listo para descargar. Contiene el reporte de ${title}.`;

          const dlBtn = document.getElementById('successDownloadBtn');
          dlBtn.className = `success-btn-download${isExcel ? ' excel-dl' : ''}`;
          dlBtn.onclick = () => { doExport(); hideAllOverlays(); };

          showOverlay('overlaySuccess');
        } catch (err) {
          hideOverlay('overlayProgress');
          showOverlay('overlayError');
        }
      });
    }

    run();
  }

  // ── Wire up 6 export buttons ───────────────────────────────
  document.getElementById('exportInvPdf').addEventListener('click', () =>
    openPdfConfig('inv', 'Reporte Inventario',
      ['Producto','Categoría','Stock','Mínimo','Valor ($)'],
      () => productos.map(r => [r.producto, r.categoria, r.stock, r.minimo, `$${r.valor.toLocaleString()}`])));

  document.getElementById('exportInvExcel').addEventListener('click', () =>
    openExcelConfig('inv', 'Inventario',
      ['Producto','Categoría','Stock','Mínimo','Valor ($)'],
      () => productos.map(r => [r.producto, r.categoria, r.stock, r.minimo, r.valor])));

  document.getElementById('exportMovPdf').addEventListener('click', () =>
    openPdfConfig('mov', 'Reporte Movimientos',
      ['Fecha','Tipo','Producto','Cantidad','Referencia'],
      () => movimientos.map(r => [r.fecha, r.tipo, r.producto, r.cantidad, r.referencia])));

  document.getElementById('exportMovExcel').addEventListener('click', () =>
    openExcelConfig('mov', 'Movimientos',
      ['Fecha','Tipo','Producto','Cantidad','Referencia'],
      () => movimientos.map(r => [r.fecha, r.tipo, r.producto, r.cantidad, r.referencia])));

  document.getElementById('exportPedPdf').addEventListener('click', () =>
    openPdfConfig('ped', 'Reporte Pedidos',
      ['Pedido','Cliente','Estado','Fecha pedido','Entrega estimada','SLA'],
      () => pedidos.map(r => [r.pedido, r.cliente, estadoLabel[r.estado]||r.estado, r.fecha, r.fechaEstimada, r.sla])));

  document.getElementById('exportPedExcel').addEventListener('click', () =>
    openExcelConfig('ped', 'Pedidos',
      ['Pedido','Cliente','Estado','Fecha pedido','Entrega estimada','SLA'],
      () => pedidos.map(r => [r.pedido, r.cliente, estadoLabel[r.estado]||r.estado, r.fecha, r.fechaEstimada, r.sla])));

  // ── Print Modal ────────────────────────────────────────────
  function openPrintModal(reportTitle) {
    if (!document.getElementById('overlayPrint')) {
      window.print();
      return;
    }

    // Update preview doc title
    document.getElementById('pmDocTitle').textContent = reportTitle;
    document.getElementById('pmDocDate').textContent =
      new Date().toLocaleDateString('es-MX', { year: 'numeric', month: 'long', day: 'numeric' });

    // Reset copies
    document.getElementById('pmCopiesVal').textContent = '1';

    // Reset orientation
    document.getElementById('pmOrientPortrait').classList.add('pm-orient-active');
    document.getElementById('pmOrientLandscape').classList.remove('pm-orient-active');

    // Reset page range
    document.getElementById('pmRangeAll').checked = true;
    document.getElementById('pmCustomRange').disabled = true;

    hideAllOverlays();
    showOverlay('overlayPrint');
  }

  // Orientation toggle
  document.getElementById('pmOrientPortrait')?.addEventListener('click', () => {
    document.getElementById('pmOrientPortrait').classList.add('pm-orient-active');
    document.getElementById('pmOrientLandscape').classList.remove('pm-orient-active');
  });
  document.getElementById('pmOrientLandscape')?.addEventListener('click', () => {
    document.getElementById('pmOrientLandscape').classList.add('pm-orient-active');
    document.getElementById('pmOrientPortrait').classList.remove('pm-orient-active');
  });

  // Custom range enable/disable
  document.querySelectorAll('input[name="pmPageRange"]').forEach(radio => {
    radio.addEventListener('change', () => {
      document.getElementById('pmCustomRange').disabled =
        document.getElementById('pmRangeCustom').checked ? false : true;
    });
  });

  // Copies counter
  let _copies = 1;
  document.getElementById('pmCopiesPlus')?.addEventListener('click', () => {
    _copies = Math.min(_copies + 1, 99);
    document.getElementById('pmCopiesVal').textContent = _copies;
  });
  document.getElementById('pmCopiesMinus')?.addEventListener('click', () => {
    _copies = Math.max(_copies - 1, 1);
    document.getElementById('pmCopiesVal').textContent = _copies;
  });

  // Close / Cancel
  ['closePrintModal', 'cancelPrintModal'].forEach(id =>
    document.getElementById(id)?.addEventListener('click', hideAllOverlays));

  // Click outside
  document.getElementById('overlayPrint')?.addEventListener('click', e => {
    if (e.target === document.getElementById('overlayPrint')) hideAllOverlays();
  });

  // Confirm → print
  document.getElementById('confirmPrintModal')?.addEventListener('click', () => {
    hideAllOverlays();
    setTimeout(() => window.print(), 120);
  });

  // Wire print buttons
  document.getElementById('printInv').addEventListener('click', () =>
    openPrintModal('Reporte de Inventario'));
  document.getElementById('printMov').addEventListener('click', () =>
    openPrintModal('Reporte de Movimientos'));
  document.getElementById('printPed').addEventListener('click', () =>
    openPrintModal('Reporte de Pedidos'));

  // ─── Reload on centro change ────────────────
  window.addEventListener('peia:centro-changed', () => {
    loadInventario().catch(err => PEIA.toast.error(err.message));
    loadMovimientos().catch(err => PEIA.toast.error(err.message));
    loadPedidos().catch(err => PEIA.toast.error(err.message));
  });

  try {
    await Promise.all([loadInventario(), loadMovimientos(), loadPedidos()]);
  } catch (error) {
    PEIA.toast.error(error.message);
  }
});
