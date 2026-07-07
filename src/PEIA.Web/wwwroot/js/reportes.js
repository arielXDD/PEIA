// PEIA — Reportes JS
// Conectado a /api/reportes, /api/inventario y /api/pedidos

document.addEventListener('DOMContentLoaded', async () => {
  if (!PEIA.requireAuth()) return;
  PEIA.hydrateShell();
  PEIA.bindWarehouseSelector();

  Chart.defaults.font.family = "'Inter', sans-serif";
  Chart.defaults.font.size = 11;
  Chart.defaults.color = '#9ca3af';

  let productos = [];
  let movimientos = [];
  let pedidos = [];

  let chartInvCategoria = null;
  let chartMovTrend = null;
  let chartPedEstados = null;
  let invTable = null;
  let movTable = null;
  let pedTable = null;

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

    if (chartInvCategoria) chartInvCategoria.destroy();
    const catColors = ['#3b82f6', '#22c55e', '#f59e0b', '#7c3aed', '#0891b2', '#ef4444'];
    const categorias = resumen.categorias || [];
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

  document.getElementById('dateFrom').addEventListener('change', () => loadMovimientos().catch(err => alert(err.message)));
  document.getElementById('dateTo').addEventListener('change', () => loadMovimientos().catch(err => alert(err.message)));

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
    if (chartPedEstados) chartPedEstados.destroy();
    const labels = Object.keys(estados).filter(k => estados[k] > 0);
    chartPedEstados = new Chart(document.getElementById('chartPedEstados'), {
      type: 'doughnut',
      data: {
        labels: labels.map(l => estadoLabel[l] || l),
        datasets: [{ data: labels.map(l => estados[l]), backgroundColor: labels.map(l => donutColors[l] || '#9ca3af'), borderWidth: 0, hoverOffset: 6 }],
      },
      options: { responsive: true, maintainAspectRatio: false, cutout: '60%', plugins: { legend: { position: 'bottom', labels: { usePointStyle: true, padding: 10 } } } },
    });

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

  document.getElementById('exportInvPdf').addEventListener('click', () => {
    exportPDF('Reporte Inventario', ['Producto', 'Categoría', 'Stock', 'Mínimo', 'Valor ($)'],
      productos.map(r => [r.producto, r.categoria, r.stock, r.minimo, `$${r.valor.toLocaleString()}`]));
  });
  document.getElementById('exportInvExcel').addEventListener('click', () => {
    exportExcel('Inventario', ['Producto', 'Categoría', 'Stock', 'Mínimo', 'Valor ($)'],
      productos.map(r => [r.producto, r.categoria, r.stock, r.minimo, r.valor]));
  });

  document.getElementById('exportMovPdf').addEventListener('click', () => {
    exportPDF('Reporte Movimientos', ['Fecha', 'Tipo', 'Producto', 'Cantidad', 'Referencia'],
      movimientos.map(r => [r.fecha, r.tipo, r.producto, r.cantidad, r.referencia]));
  });
  document.getElementById('exportMovExcel').addEventListener('click', () => {
    exportExcel('Movimientos', ['Fecha', 'Tipo', 'Producto', 'Cantidad', 'Referencia'],
      movimientos.map(r => [r.fecha, r.tipo, r.producto, r.cantidad, r.referencia]));
  });

  document.getElementById('exportPedPdf').addEventListener('click', () => {
    exportPDF('Reporte Pedidos', ['Pedido', 'Cliente', 'Estado', 'Fecha pedido', 'Entrega estimada', 'SLA'],
      pedidos.map(r => [r.pedido, r.cliente, estadoLabel[r.estado] || r.estado, r.fecha, r.fechaEstimada, r.sla]));
  });
  document.getElementById('exportPedExcel').addEventListener('click', () => {
    exportExcel('Pedidos', ['Pedido', 'Cliente', 'Estado', 'Fecha pedido', 'Entrega estimada', 'SLA'],
      pedidos.map(r => [r.pedido, r.cliente, estadoLabel[r.estado] || r.estado, r.fecha, r.fechaEstimada, r.sla]));
  });

  // ─── Reload on centro change ────────────────
  window.addEventListener('peia:centro-changed', () => {
    loadInventario().catch(err => alert(err.message));
    loadMovimientos().catch(err => alert(err.message));
    loadPedidos().catch(err => alert(err.message));
  });

  try {
    await Promise.all([loadInventario(), loadMovimientos(), loadPedidos()]);
  } catch (error) {
    alert(error.message);
  }
});
