// PEIA — Roles JS
// Mock data, DataTable, Modal

document.addEventListener('DOMContentLoaded', () => {

  const roles = [
    { id: 1, nombre: 'Administrador',      descripcion: 'Acceso total al sistema, gestión de usuarios, configuración y reportes.',         usuarios: 1, permisos: ['usuarios', 'roles', 'inventario', 'pedidos', 'reportes', 'config', 'camaras', 'notificaciones'] },
    { id: 2, nombre: 'OperadorInventario',  descripcion: 'Gestión de productos, stock, entradas y salidas de inventario.',                  usuarios: 3, permisos: ['inventario', 'notificaciones'] },
    { id: 3, nombre: 'Logistica',           descripcion: 'Gestión de pedidos, rutas, rastreo y control de SLAs.',                           usuarios: 2, permisos: ['pedidos', 'notificaciones'] },
    { id: 4, nombre: 'Reportes',            descripcion: 'Visualización de reportes, gráficas y exportación de datos.',                     usuarios: 2, permisos: ['reportes', 'inventario'] },
    { id: 5, nombre: 'Supervisor',          descripcion: 'Supervisión general de operaciones, alertas y monitoreo de cámaras.',             usuarios: 2, permisos: ['inventario', 'pedidos', 'reportes', 'camaras', 'notificaciones'] },
  ];

  const permisoLabels = {
    'usuarios': 'Usuarios', 'roles': 'Roles', 'inventario': 'Inventario',
    'pedidos': 'Pedidos', 'reportes': 'Reportes', 'config': 'Configuración',
    'camaras': 'Cámaras', 'notificaciones': 'Notificaciones',
  };

  const table = new DataTable(document.getElementById('rolesTable'), {
    columns: [
      { key: 'nombre', label: 'Rol', render: (val) => `<span class="cell-bold">${val}</span>` },
      { key: 'descripcion', label: 'Descripción', render: (val) => `<span style="max-width:320px;display:inline-block;line-height:1.4;">${val}</span>` },
      { key: 'usuarios', label: 'Usuarios', render: (val) => `<span class="cell-bold">${val}</span>` },
      { key: 'permisos', label: 'Permisos', sortable: false, render: (val) => `<div class="permisos-grid">${val.map(p => `<span class="permiso-tag active">${permisoLabels[p] || p}</span>`).join('')}</div>` },
      { key: 'id', label: 'Acciones', sortable: false, render: (val, row) => `
        <div class="cell-actions">
          <button class="btn-icon btn-edit" data-id="${val}" title="Editar">
            <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
          </button>
        </div>` },
    ],
    data: roles,
    pageSize: 10,
  });

  // Search
  document.getElementById('searchRoles').addEventListener('input', (e) => {
    const term = e.target.value.toLowerCase();
    const filtered = roles.filter(r =>
      r.nombre.toLowerCase().includes(term) ||
      r.descripcion.toLowerCase().includes(term)
    );
    table.setData(filtered);
  });

  // New role
  document.getElementById('btnNuevoRol').addEventListener('click', () => {
    new ModalForm({
      title: 'Nuevo rol',
      fields: [
        { key: 'nombre', label: 'Nombre del rol', type: 'text', placeholder: 'Ej: Auditor', required: true },
        { key: 'descripcion', label: 'Descripción', type: 'textarea', placeholder: 'Describe los permisos y responsabilidades...' },
      ],
      onSave: (data) => {
        roles.push({ id: roles.length + 1, nombre: data.nombre, descripcion: data.descripcion, usuarios: 0, permisos: [] });
        table.setData([...roles]);
      },
    });
  });

  // Edit
  document.getElementById('rolesTable').addEventListener('click', (e) => {
    const editBtn = e.target.closest('.btn-edit');
    if (editBtn) {
      const id = parseInt(editBtn.dataset.id);
      const rol = roles.find(r => r.id === id);
      if (!rol) return;
      new ModalForm({
        title: `Editar rol — ${rol.nombre}`,
        fields: [
          { key: 'nombre', label: 'Nombre del rol', type: 'text', required: true },
          { key: 'descripcion', label: 'Descripción', type: 'textarea' },
        ],
        initialData: rol,
        onSave: (data) => {
          Object.assign(rol, { nombre: data.nombre, descripcion: data.descripcion });
          table.setData([...roles]);
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

  // Logout
  document.getElementById('btnLogout').addEventListener('click', () => { localStorage.removeItem('peia_token'); localStorage.removeItem('peia_user'); window.location.href = '/login.html'; });
  const user = JSON.parse(localStorage.getItem('peia_user') || '{}');
  if (user.nombreCompleto) { document.getElementById('userName').textContent = user.nombreCompleto; document.getElementById('userAvatar').textContent = user.nombreCompleto.split(' ').map(n => n[0]).join('').slice(0, 2).toUpperCase(); }
});
