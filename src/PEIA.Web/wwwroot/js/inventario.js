document.addEventListener('DOMContentLoaded', async () => {
  if (!PEIA.requireAuth()) return;
  PEIA.hydrateShell();
  PEIA.bindWarehouseSelector();

  let productos = [];
  let categorias = [];

  function getStockStatus(stock, minimo) {
    if (stock === 0) return { label: 'Agotado', cls: 'status-critical', barCls: 'red', pct: 0 };
    if (stock < minimo) return { label: 'Stock bajo', cls: 'status-warning', barCls: 'orange', pct: Math.round((stock / Math.max(1, minimo)) * 100) };
    return { label: 'Normal', cls: 'status-active', barCls: 'green', pct: Math.min(100, Math.round((stock / Math.max(1, minimo)) * 100)) };
  }

  const table = new DataTable(document.getElementById('productsTable'), {
    columns: [
      { key: 'sku', label: 'SKU', render: val => `<span class="cell-mono">${val}</span>` },
      { key: 'nombre', label: 'Producto', render: val => `<span class="cell-primary">${val}</span>` },
      { key: 'categoria', label: 'Categoría', render: val => `<span class="tag">${val}</span>` },
      { key: 'stock', label: 'Stock', render: (val, row) => {
        const s = getStockStatus(val, row.minimo);
        return `<span class="cell-bold">${val}</span> <span style="color:var(--text-muted);font-size:11px;">/ ${row.minimo} mín</span>
          <div class="stock-bar"><div class="stock-bar-fill ${s.barCls}" style="width:${Math.min(100, s.pct)}%"></div></div>`;
      }},
      { key: 'stock', label: 'Estado', render: (val, row) => {
        const s = getStockStatus(val, row.minimo);
        return `<span class="status-badge ${s.cls}"><span class="status-dot-sm"></span>${s.label}</span>`;
      }},
      { key: 'ubicacion', label: 'Ubicación', render: val => `<span class="cell-mono">${val || '-'}</span>` },
      { key: 'precio', label: 'Precio', render: val => `$${Number(val || 0).toLocaleString('es-MX')}` },
      { key: 'id', label: 'Acciones', sortable: false, render: val => `
        <div class="cell-actions">
          <button class="btn-icon btn-edit" data-id="${val}" title="Editar">
            <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
          </button>
          <button class="btn-icon btn-move" data-id="${val}" title="Movimiento">
            <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 12H3"/><path d="M16 7l5 5-5 5"/></svg>
          </button>
        </div>` },
    ],
    data: [],
    pageSize: 8,
  });

  function mapProducto(p) {
    return {
      id: p.id,
      sku: p.sku,
      nombre: p.nombre,
      descripcion: p.descripcion || '',
      categoria: p.categoria,
      categoriaId: p.categoriaId,
      stock: p.stock,
      minimo: p.stockMinimo,
      ubicacion: p.ubicacion,
      precio: p.precioUnitario,
      unidadMedida: p.unidadMedida,
      activo: p.activo
    };
  }

  async function loadData() {
    const centro = PEIA.getActiveCentro();
    if (!centro?.id) throw new Error('Selecciona un centro activo.');
    const [productosRaw, categoriasRaw] = await Promise.all([
      PEIA.request(`/api/inventario/productos?centroId=${centro.id}`),
      PEIA.request('/api/inventario/categorias')
    ]);
    productos = productosRaw.map(mapProducto);
    categorias = categoriasRaw;
    hydrateCategoriaFilter();
    applyFilters();
    updateKpis();
  }

  function hydrateCategoriaFilter() {
    const filter = document.getElementById('filterCategoria');
    const current = filter.value;
    filter.innerHTML = '<option value="">Todas las categorías</option>' +
      categorias.map(c => `<option value="${c.nombre}">${c.nombre}</option>`).join('');
    filter.value = current;
  }

  function updateKpis() {
    const total = productos.length;
    const bajo = productos.filter(p => p.stock > 0 && p.stock < p.minimo).length;
    const agotado = productos.filter(p => p.stock === 0).length;
    const totalEl = document.getElementById('kpi-total');
    const bajoEl = document.getElementById('kpi-bajo');
    const agotadoEl = document.getElementById('kpi-agotado');
    if (totalEl) totalEl.textContent = total;
    if (bajoEl) bajoEl.textContent = bajo;
    if (agotadoEl) agotadoEl.textContent = agotado;
  }

  const searchInput = document.getElementById('searchProducts');
  const filterCat = document.getElementById('filterCategoria');
  const filterStock = document.getElementById('filterStock');

  function applyFilters() {
    const term = searchInput.value.toLowerCase();
    const cat = filterCat.value;
    const stock = filterStock.value;

    let data = [...productos];
    if (term) data = data.filter(p => p.sku.toLowerCase().includes(term) || p.nombre.toLowerCase().includes(term) || p.categoria.toLowerCase().includes(term));
    if (cat) data = data.filter(p => p.categoria === cat);
    if (stock === 'normal') data = data.filter(p => p.stock >= p.minimo);
    else if (stock === 'bajo') data = data.filter(p => p.stock > 0 && p.stock < p.minimo);
    else if (stock === 'agotado') data = data.filter(p => p.stock === 0);

    table.setData(data);
  }

  function productFields() {
    return [
      { key: 'sku', label: 'SKU', type: 'text', required: true, half: true },
      { key: 'nombre', label: 'Nombre del producto', type: 'text', required: true, half: true },
      { key: 'categoriaId', label: 'Categoría', type: 'select', required: true, half: true, options: categorias.map(c => ({ value: c.id, label: c.nombre })) },
      { key: 'ubicacion', label: 'Ubicación', type: 'text', placeholder: 'A-01-01', half: true },
      { key: 'stock', label: 'Stock inicial', type: 'number', min: 0, half: true },
      { key: 'minimo', label: 'Stock mínimo', type: 'number', min: 0, half: true },
      { key: 'precio', label: 'Precio unitario ($)', type: 'number', min: 0, half: true },
      { key: 'unidadMedida', label: 'Unidad', type: 'text', value: 'Pieza', half: true },
      { key: 'descripcion', label: 'Descripción', type: 'textarea' },
    ];
  }

  function buildProductPayload(data) {
    return {
      sku: data.sku,
      nombre: data.nombre,
      descripcion: data.descripcion || '',
      unidadMedida: data.unidadMedida || 'Pieza',
      precioUnitario: data.precio || 0,
      stockMinimo: data.minimo || 0,
      activo: true,
      categoriaId: data.categoriaId,
      centroId: PEIA.getActiveCentro().id
    };
  }

  searchInput.addEventListener('input', applyFilters);
  filterCat.addEventListener('change', applyFilters);
  filterStock.addEventListener('change', applyFilters);
  window.addEventListener('peia:centro-changed', () => loadData().catch(error => alert(error.message)));

  document.getElementById('btnNuevoProducto').addEventListener('click', () => {
    new ModalForm({
      title: 'Nuevo producto',
      fields: productFields(),
      onSave: async data => {
        try {
          const created = await PEIA.request('/api/inventario/productos', { method: 'POST', body: JSON.stringify(buildProductPayload(data)) });
          if (data.stock > 0) {
            await PEIA.request('/api/inventario/movimientos', {
              method: 'POST',
              body: JSON.stringify({
                productoId: created.id,
                centroId: PEIA.getActiveCentro().id,
                tipo: 'Entrada',
                cantidad: data.stock,
                ubicacion: data.ubicacion || '',
                motivo: 'Stock inicial',
                referencia: 'Alta producto'
              })
            });
          }
          await loadData();
        } catch (error) {
          alert(error.message);
        }
      },
    });
  });

  document.getElementById('productsTable').addEventListener('click', e => {
    const editBtn = e.target.closest('.btn-edit');
    const moveBtn = e.target.closest('.btn-move');

    if (editBtn) {
      const prod = productos.find(p => p.id === editBtn.dataset.id);
      if (!prod) return;
      new ModalForm({
        title: `Editar - ${prod.nombre}`,
        fields: productFields().filter(f => f.key !== 'stock'),
        initialData: prod,
        onSave: async data => {
          try {
            await PEIA.request(`/api/inventario/productos/${prod.id}`, { method: 'PUT', body: JSON.stringify(buildProductPayload(data)) });
            await loadData();
          } catch (error) {
            alert(error.message);
          }
        },
      });
    }

    if (moveBtn) {
      const prod = productos.find(p => p.id === moveBtn.dataset.id);
      if (!prod) return;
      new ModalForm({
        title: `Movimiento - ${prod.nombre}`,
        fields: [
          { key: 'tipo', label: 'Tipo', type: 'select', required: true, options: [
            { value: 'Entrada', label: 'Entrada' },
            { value: 'Salida', label: 'Salida' },
            { value: 'Ajuste', label: 'Ajuste' },
          ]},
          { key: 'cantidad', label: 'Cantidad', type: 'number', min: 1, required: true, half: true },
          { key: 'ubicacion', label: 'Ubicación', type: 'text', value: prod.ubicacion || '', half: true },
          { key: 'motivo', label: 'Motivo', type: 'textarea' },
          { key: 'referencia', label: 'Referencia', type: 'text' },
        ],
        initialData: { tipo: 'Entrada', ubicacion: prod.ubicacion || '' },
        onSave: async data => {
          try {
            await PEIA.request('/api/inventario/movimientos', {
              method: 'POST',
              body: JSON.stringify({
                productoId: prod.id,
                centroId: PEIA.getActiveCentro().id,
                tipo: data.tipo,
                cantidad: data.cantidad,
                ubicacion: data.ubicacion,
                motivo: data.motivo,
                referencia: data.referencia
              })
            });
            await loadData();
          } catch (error) {
            alert(error.message);
          }
        },
      });
    }
  });

  try {
    await loadData();
  } catch (error) {
    alert(error.message);
  }
});
