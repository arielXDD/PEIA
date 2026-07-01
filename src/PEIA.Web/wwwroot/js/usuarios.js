// PEIA — Usuarios JS
// Mock data, DataTable, Modal, filters

document.addEventListener('DOMContentLoaded', () => {

  // ─── Mock Data ──────────────────────────────
  const usuarios = [
    { id: 1, nombre: 'Ariel Guevara',     email: 'admin@peia.com',       userName: 'admin',       rol: 'Administrador',     centros: ['Bodega Norte', 'Bodega Sur'], estado: 'Activo' },
    { id: 2, nombre: 'Carlos Inventario', email: 'inventario@peia.com', userName: 'inventario', rol: 'OperadorInventario', centros: ['Bodega Norte'],               estado: 'Activo' },
    { id: 3, nombre: 'Laura Logística',   email: 'logistica@peia.com', userName: 'logistica',  rol: 'Logistica',          centros: ['Bodega Sur'],                  estado: 'Activo' },
    { id: 4, nombre: 'María Reportes',    email: 'reportes@peia.com',  userName: 'reportes',   rol: 'Reportes',           centros: ['Bodega Norte', 'Bodega Sur'], estado: 'Activo' },
    { id: 5, nombre: 'Pedro Supervisor',  email: 'supervisor@peia.com',userName: 'supervisor', rol: 'Supervisor',         centros: ['Bodega Norte'],               estado: 'Activo' },
    { id: 6, nombre: 'Ana García',        email: 'ana@peia.com',       userName: 'ana.garcia', rol: 'OperadorInventario', centros: ['Bodega Sur'],                  estado: 'Inactivo' },
    { id: 7, nombre: 'Roberto Méndez',    email: 'roberto@peia.com',   userName: 'r.mendez',   rol: 'Logistica',          centros: ['Bodega Norte'],               estado: 'Activo' },
    { id: 8, nombre: 'Sofía Torres',      email: 'sofia@peia.com',     userName: 's.torres',   rol: 'Reportes',           centros: ['Bodega Sur'],                  estado: 'Activo' },
    { id: 9, nombre: 'Diego Ramírez',     email: 'diego@peia.com',     userName: 'd.ramirez',  rol: 'OperadorInventario', centros: ['Bodega Norte', 'Bodega Sur'], estado: 'Activo' },
    { id: 10, nombre: 'Valentina López',  email: 'valentina@peia.com', userName: 'v.lopez',    rol: 'Supervisor',         centros: ['Bodega Sur'],                  estado: 'Inactivo' },
  ];

  const rolLabels = {
    'Administrador': 'Administrador',
    'OperadorInventario': 'Inventario',
    'Logistica': 'Logística',
    'Reportes': 'Reportes',
    'Supervisor': 'Supervisor',
  };

  const rolColors = {
    'Administrador': 'status-info',
    'OperadorInventario': 'status-active',
    'Logistica': 'status-warning',
    'Reportes': 'status-pending',
    'Supervisor': 'status-critical',
  };

  // ─── DataTable ──────────────────────────────
  const table = new DataTable(document.getElementById('usersTable'), {
    columns: [
      { key: 'nombre', label: 'Nombre', render: (val, row) => `
        <div style="display:flex;align-items:center;gap:10px;">
          <div class="avatar" style="width:30px;height:30px;font-size:11px;">${row.nombre.split(' ').map(n=>n[0]).join('').slice(0,2)}</div>
          <div>
            <div class="cell-primary">${val}</div>
            <div style="font-size:11.5px;color:var(--text-muted);">@${row.userName}</div>
          </div>
        </div>` },
      { key: 'email', label: 'Email' },
      { key: 'rol', label: 'Rol', render: (val) => `<span class="status-badge ${rolColors[val] || ''}">${rolLabels[val] || val}</span>` },
      { key: 'centros', label: 'Centros', render: (val) => val.map(c => `<span class="tag">${c}</span>`).join(' ') },
      { key: 'estado', label: 'Estado', render: (val) => `<span class="status-badge ${val === 'Activo' ? 'status-active' : 'status-inactive'}"><span class="status-dot-sm"></span>${val}</span>` },
      { key: 'id', label: 'Acciones', sortable: false, render: (val, row) => `
        <div class="cell-actions">
          <button class="btn-icon btn-edit" data-id="${val}" title="Editar">
            <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
          </button>
          <button class="btn-icon danger btn-toggle" data-id="${val}" title="${row.estado === 'Activo' ? 'Desactivar' : 'Activar'}">
            <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">${row.estado === 'Activo'
              ? '<path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94"/><path d="M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19"/><line x1="1" y1="1" x2="23" y2="23"/>'
              : '<path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/>'}</svg>
          </button>
        </div>` },
    ],
    data: usuarios,
    pageSize: 8,
  });

  // ─── Search & Filters ───────────────────────
  const searchInput = document.getElementById('searchUsers');
  const filterRol = document.getElementById('filterRol');
  const filterEstado = document.getElementById('filterEstado');

  function applyFilters() {
    const term = searchInput.value.toLowerCase();
    const rol = filterRol.value;
    const estado = filterEstado.value;

    let data = [...usuarios];
    if (term) {
      data = data.filter(u =>
        u.nombre.toLowerCase().includes(term) ||
        u.email.toLowerCase().includes(term) ||
        u.userName.toLowerCase().includes(term) ||
        (rolLabels[u.rol] || u.rol).toLowerCase().includes(term)
      );
    }
    if (rol) data = data.filter(u => u.rol === rol);
    if (estado) data = data.filter(u => u.estado === estado);

    table.setData(data);
  }

  searchInput.addEventListener('input', applyFilters);
  filterRol.addEventListener('change', applyFilters);
  filterEstado.addEventListener('change', applyFilters);

  // ─── Nuevo Usuario Modal ────────────────────
  const userFormFields = [
    { key: 'nombre', label: 'Nombre completo', type: 'text', placeholder: 'Ej: Juan Pérez', required: true },
    { key: 'email', label: 'Correo electrónico', type: 'email', placeholder: 'ejemplo@peia.com', required: true, half: true },
    { key: 'userName', label: 'Nombre de usuario', type: 'text', placeholder: 'j.perez', required: true, half: true },
    { key: 'password', label: 'Contraseña', type: 'password', placeholder: '••••••••', required: true, half: true },
    { key: 'rol', label: 'Rol', type: 'select', required: true, half: true, options: [
      { value: 'Administrador', label: 'Administrador' },
      { value: 'OperadorInventario', label: 'Inventario' },
      { value: 'Logistica', label: 'Logística' },
      { value: 'Reportes', label: 'Reportes' },
      { value: 'Supervisor', label: 'Supervisor' },
    ]},
    { key: 'activo', label: 'Usuario activo', type: 'checkbox', value: true },
  ];

  document.getElementById('btnNuevoUsuario').addEventListener('click', () => {
    new ModalForm({
      title: 'Nuevo usuario',
      fields: userFormFields,
      onSave: (data) => {
        // Mock: just add to array and refresh
        usuarios.push({
          id: usuarios.length + 1,
          nombre: data.nombre,
          email: data.email,
          userName: data.userName,
          rol: data.rol,
          centros: ['Bodega Norte'],
          estado: data.activo ? 'Activo' : 'Inactivo',
        });
        applyFilters();
      },
    });
  });

  // ─── Edit / Toggle via event delegation ─────
  document.getElementById('usersTable').addEventListener('click', (e) => {
    const editBtn = e.target.closest('.btn-edit');
    const toggleBtn = e.target.closest('.btn-toggle');

    if (editBtn) {
      const id = parseInt(editBtn.dataset.id);
      const user = usuarios.find(u => u.id === id);
      if (!user) return;

      const editFields = [
        { key: 'nombre', label: 'Nombre completo', type: 'text', required: true },
        { key: 'email', label: 'Correo electrónico', type: 'email', required: true, half: true },
        { key: 'userName', label: 'Nombre de usuario', type: 'text', required: true, half: true },
        { key: 'rol', label: 'Rol', type: 'select', required: true, options: [
          { value: 'Administrador', label: 'Administrador' },
          { value: 'OperadorInventario', label: 'Inventario' },
          { value: 'Logistica', label: 'Logística' },
          { value: 'Reportes', label: 'Reportes' },
          { value: 'Supervisor', label: 'Supervisor' },
        ]},
        { key: 'activo', label: 'Usuario activo', type: 'checkbox' },
      ];

      new ModalForm({
        title: `Editar usuario — ${user.nombre}`,
        fields: editFields,
        initialData: { ...user, activo: user.estado === 'Activo' },
        onSave: (data) => {
          Object.assign(user, {
            nombre: data.nombre,
            email: data.email,
            userName: data.userName,
            rol: data.rol,
            estado: data.activo ? 'Activo' : 'Inactivo',
          });
          applyFilters();
        },
      });
    }

    if (toggleBtn) {
      const id = parseInt(toggleBtn.dataset.id);
      const user = usuarios.find(u => u.id === id);
      if (user) {
        user.estado = user.estado === 'Activo' ? 'Inactivo' : 'Activo';
        applyFilters();
      }
    }
  });

  // ─── Warehouse selector ─────────────────────
  const warehouseSelector = document.querySelector('.warehouse-selector');
  warehouseSelector.addEventListener('click', (e) => { e.stopPropagation(); warehouseSelector.classList.toggle('open'); });
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

  // ─── Logout & User ─────────────────────────
  document.getElementById('btnLogout').addEventListener('click', () => {
    localStorage.removeItem('peia_token');
    localStorage.removeItem('peia_user');
    window.location.href = '/login.html';
  });

  const user = JSON.parse(localStorage.getItem('peia_user') || '{}');
  if (user.nombreCompleto) {
    document.getElementById('userName').textContent = user.nombreCompleto;
    const initials = user.nombreCompleto.split(' ').map(n => n[0]).join('').slice(0, 2).toUpperCase();
    document.getElementById('userAvatar').textContent = initials;
  }
});
