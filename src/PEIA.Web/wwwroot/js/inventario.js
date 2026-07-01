// PEIA — Inventario JS
// Mock products data, DataTable, filters, CRUD modals

document.addEventListener('DOMContentLoaded', () => {

  const productos = [
    { id: 1,  sku: 'TAR-001', nombre: 'Tarima de Madera',      categoria: 'Almacenamiento', stock: 320, minimo: 50,  ubicacion: 'A-01-01', ultimaEntrada: '01/07/2026', precio: 450 },
    { id: 2,  sku: 'CAJ-002', nombre: 'Caja Plástica 60L',     categoria: 'Almacenamiento', stock: 185, minimo: 40,  ubicacion: 'B-03-03', ultimaEntrada: '30/06/2026', precio: 120 },
    { id: 3,  sku: 'PAL-003', nombre: 'Pallet Plástico',       categoria: 'Almacenamiento', stock: 95,  minimo: 30,  ubicacion: 'A-02-03', ultimaEntrada: '29/06/2026', precio: 780 },
    { id: 4,  sku: 'CIN-004', nombre: 'Cinta Adhesiva 48mm',   categoria: 'Embalaje',       stock: 25,  minimo: 50,  ubicacion: 'C-01-02', ultimaEntrada: '28/06/2026', precio: 35  },
    { id: 5,  sku: 'FIL-005', nombre: 'Film Stretch',          categoria: 'Embalaje',       stock: 150, minimo: 30,  ubicacion: 'C-07-02', ultimaEntrada: '01/07/2026', precio: 95  },
    { id: 6,  sku: 'GUA-006', nombre: 'Guantes de Nitrilo L',  categoria: 'Seguridad',      stock: 0,   minimo: 100, ubicacion: 'D-02-01', ultimaEntrada: '25/06/2026', precio: 15  },
    { id: 7,  sku: 'CAS-007', nombre: 'Casco de Seguridad',    categoria: 'Seguridad',      stock: 42,  minimo: 20,  ubicacion: 'D-01-03', ultimaEntrada: '27/06/2026', precio: 85  },
    { id: 8,  sku: 'ETI-008', nombre: 'Etiquetas Código Barras',categoria:'Embalaje',       stock: 2400,minimo: 500, ubicacion: 'C-05-01', ultimaEntrada: '01/07/2026', precio: 8   },
    { id: 9,  sku: 'ESC-009', nombre: 'Escáner de Código',     categoria: 'Herramientas',   stock: 12,  minimo: 5,   ubicacion: 'E-01-01', ultimaEntrada: '20/06/2026', precio: 1200},
    { id: 10, sku: 'DES-010', nombre: 'Desengrasante Industrial',categoria:'Limpieza',      stock: 18,  minimo: 25,  ubicacion: 'F-03-02', ultimaEntrada: '26/06/2026', precio: 65  },
    { id: 11, sku: 'BOL-011', nombre: 'Bolsas de Plástico XL', categoria: 'Embalaje',       stock: 800, minimo: 200, ubicacion: 'C-02-01', ultimaEntrada: '30/06/2026', precio: 5   },
    { id: 12, sku: 'CHA-012', nombre: 'Chaleco Reflectante',   categoria: 'Seguridad',      stock: 35,  minimo: 15,  ubicacion: 'D-03-01', ultimaEntrada: '28/06/2026', precio: 45  },
  ];

  function getStockStatus(stock, minimo) {
    if (stock === 0) return { label: 'Agotado', cls: 'status-critical', barCls: 'red', pct: 0 };
    if (stock < minimo) return { label: 'Stock bajo', cls: 'status-warning', barCls: 'orange', pct: Math.round((stock / minimo) * 100) };
    return { label: 'Normal', cls: 'status-active', barCls: 'green', pct: Math.min(100, Math.round((stock / minimo) * 100)) };
  }

  const table = new DataTable(document.getElementById('productsTable'), {
    columns: [
      { key: 'sku', label: 'SKU', render: (val) => `<span class="cell-mono">${val}</span>` },
      { key: 'nombre', label: 'Producto', render: (val) => `<span class="cell-primary">${val}</span>` },
      { key: 'categoria', label: 'Categoría', render: (val) => `<span class="tag">${val}</span>` },
      { key: 'stock', label: 'Stock', render: (val, row) => {
        const s = getStockStatus(val, row.minimo);
        return `<span class="cell-bold">${val}</span> <span style="color:var(--text-muted);font-size:11px;">/ ${row.minimo} mín</span>
          <div class="stock-bar"><div class="stock-bar-fill ${s.barCls}" style="width:${Math.min(100, s.pct)}%"></div></div>`;
      }},
      { key: 'stock', label: 'Estado', render: (val, row) => {
        const s = getStockStatus(val, row.minimo);
        return `<span class="status-badge ${s.cls}"><span class="status-dot-sm"></span>${s.label}</span>`;
      }},
      { key: 'ubicacion', label: 'Ubicación', render: (val) => `<span class="cell-mono">${val}</span>` },
      { key: 'ultimaEntrada', label: 'Últ. entrada' },
      { key: 'id', label: 'Acciones', sortable: false, render: (val, row) => `
        <div class="cell-actions">
          <button class="btn-icon btn-edit" data-id="${val}" title="Editar">
            <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
          </button>
        </div>` },
    ],
    data: productos,
    pageSize: 8,
  });

  // Search & Filters
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

  searchInput.addEventListener('input', applyFilters);
  filterCat.addEventListener('change', applyFilters);
  filterStock.addEventListener('change', applyFilters);

  // New product
  const productFields = [
    { key: 'sku', label: 'SKU', type: 'text', placeholder: 'TAR-001', required: true, half: true },
    { key: 'nombre', label: 'Nombre del producto', type: 'text', placeholder: 'Tarima de Madera', required: true, half: true },
    { key: 'categoria', label: 'Categoría', type: 'select', required: true, half: true, options: [
      { value: 'Embalaje', label: 'Embalaje' }, { value: 'Almacenamiento', label: 'Almacenamiento' },
      { value: 'Seguridad', label: 'Seguridad' }, { value: 'Herramientas', label: 'Herramientas' },
      { value: 'Limpieza', label: 'Limpieza' },
    ]},
    { key: 'ubicacion', label: 'Ubicación', type: 'text', placeholder: 'A-01-01', required: true, half: true },
    { key: 'stock', label: 'Stock inicial', type: 'number', placeholder: '0', min: 0, half: true },
    { key: 'minimo', label: 'Stock mínimo', type: 'number', placeholder: '50', min: 0, half: true },
    { key: 'precio', label: 'Precio unitario ($)', type: 'number', placeholder: '0.00', min: 0, half: true },
  ];

  document.getElementById('btnNuevoProducto').addEventListener('click', () => {
    new ModalForm({
      title: 'Nuevo producto',
      fields: productFields,
      onSave: (data) => {
        productos.push({
          id: productos.length + 1, sku: data.sku, nombre: data.nombre,
          categoria: data.categoria, stock: data.stock || 0, minimo: data.minimo || 0,
          ubicacion: data.ubicacion, ultimaEntrada: new Date().toLocaleDateString('es-MX'),
          precio: data.precio || 0,
        });
        applyFilters();
      },
    });
  });

  // Edit
  document.getElementById('productsTable').addEventListener('click', (e) => {
    const editBtn = e.target.closest('.btn-edit');
    if (editBtn) {
      const id = parseInt(editBtn.dataset.id);
      const prod = productos.find(p => p.id === id);
      if (!prod) return;
      new ModalForm({
        title: `Editar — ${prod.nombre}`,
        fields: productFields,
        initialData: prod,
        onSave: (data) => {
          Object.assign(prod, data);
          applyFilters();
        },
      });
    }
  });

  // Warehouse selector
  const ws = document.querySelector('.warehouse-selector');
  ws.addEventListener('click', (e) => { e.stopPropagation(); ws.classList.toggle('open'); });
  document.querySelectorAll('.warehouse-opt').forEach(btn => {
    btn.addEventListener('click', (e) => { e.stopPropagation(); document.querySelectorAll('.warehouse-opt').forEach(b => b.classList.remove('active')); btn.classList.add('active'); document.getElementById('activeCentro').textContent = btn.textContent; ws.classList.remove('open'); });
  });
  document.addEventListener('click', () => ws.classList.remove('open'));

  // Logout & User
  document.getElementById('btnLogout').addEventListener('click', () => { localStorage.removeItem('peia_token'); localStorage.removeItem('peia_user'); window.location.href = '/login.html'; });
  const user = JSON.parse(localStorage.getItem('peia_user') || '{}');
  if (user.nombreCompleto) { document.getElementById('userName').textContent = user.nombreCompleto; document.getElementById('userAvatar').textContent = user.nombreCompleto.split(' ').map(n => n[0]).join('').slice(0, 2).toUpperCase(); }
});
