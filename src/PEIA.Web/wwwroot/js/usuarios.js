document.addEventListener('DOMContentLoaded', async () => {
  if (!PEIA.requireAuth()) return;
  PEIA.hydrateShell();
  PEIA.bindWarehouseSelector();

  let usuarios = [];
  let roles = [];
  let centros = [];

  const rolLabels = {
    Administrador: 'Administrador',
    OperadorInventario: 'Inventario',
    Logistica: 'Logística',
    Reportes: 'Reportes',
    Supervisor: 'Supervisor',
  };

  const rolColors = {
    Administrador: 'status-info',
    OperadorInventario: 'status-active',
    Logistica: 'status-warning',
    Reportes: 'status-pending',
    Supervisor: 'status-critical',
  };

  const table = new DataTable(document.getElementById('usersTable'), {
    columns: [
      { key: 'nombre', label: 'Nombre', render: (val, row) => `
        <div style="display:flex;align-items:center;gap:10px;">
          <div class="avatar" style="width:30px;height:30px;font-size:11px;">${val.split(' ').map(n=>n[0]).join('').slice(0,2)}</div>
          <div>
            <div class="cell-primary">${val}</div>
            <div style="font-size:11.5px;color:var(--text-muted);">@${row.userName}</div>
          </div>
        </div>` },
      { key: 'email', label: 'Email' },
      { key: 'rol', label: 'Rol', render: (val) => `<span class="status-badge ${rolColors[val] || ''}">${rolLabels[val] || val || 'Sin rol'}</span>` },
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
    data: [],
    pageSize: 8,
  });

  const searchInput = document.getElementById('searchUsers');
  const filterRol = document.getElementById('filterRol');
  const filterEstado = document.getElementById('filterEstado');

  function mapUsuario(u) {
    return {
      id: u.id,
      nombre: u.nombreCompleto,
      email: u.email,
      userName: u.userName,
      rol: u.roles?.[0] || '',
      roles: u.roles || [],
      centros: (u.centros || []).map(c => c.nombre),
      centroIds: (u.centros || []).map(c => c.id),
      estado: u.activo ? 'Activo' : 'Inactivo',
    };
  }

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

  async function loadData() {
    const [usuariosRaw, rolesRaw, centrosRaw] = await Promise.all([
      PEIA.request('/api/usuarios'),
      PEIA.request('/api/roles'),
      PEIA.request('/api/centros')
    ]);
    usuarios = usuariosRaw.map(mapUsuario);
    roles = rolesRaw;
    centros = centrosRaw;
    applyFilters();
  }

  function userFields(isEdit = false) {
    return [
      { key: 'nombre', label: 'Nombre completo', type: 'text', required: true },
      { key: 'email', label: 'Correo electrónico', type: 'email', required: true, half: true },
      { key: 'userName', label: 'Nombre de usuario', type: 'text', required: true, half: true },
      { key: 'password', label: isEdit ? 'Nueva contraseña' : 'Contraseña', type: 'password', required: !isEdit, half: true },
      { key: 'rol', label: 'Rol', type: 'select', required: true, half: true, options: roles.map(r => ({ value: r.nombre, label: rolLabels[r.nombre] || r.nombre })) },
      { key: 'centro1', label: 'Centro 1', type: 'select', required: true, half: true, options: centros.map(c => ({ value: c.id, label: c.nombre })) },
      { key: 'centro2', label: 'Centro 2', type: 'select', half: true, options: centros.map(c => ({ value: c.id, label: c.nombre })) },
      { key: 'activo', label: 'Usuario activo', type: 'checkbox', value: true },
    ];
  }

  function buildPayload(data, isEdit = false) {
    const centroIds = [data.centro1, data.centro2].filter(Boolean);
    return {
      userName: data.userName,
      email: data.email,
      nombreCompleto: data.nombre,
      password: data.password || '',
      activo: !!data.activo,
      roles: data.rol ? [data.rol] : [],
      centroIds: [...new Set(centroIds)]
    };
  }

  searchInput.addEventListener('input', applyFilters);
  filterRol.addEventListener('change', applyFilters);
  filterEstado.addEventListener('change', applyFilters);

  document.getElementById('btnNuevoUsuario').addEventListener('click', () => {
    new ModalForm({
      title: 'Nuevo usuario',
      fields: userFields(false),
      onSave: async data => {
        try {
          await PEIA.request('/api/usuarios', { method: 'POST', body: JSON.stringify(buildPayload(data)) });
          await loadData();
        } catch (error) {
          PEIA.toast.error(error.message);
        }
      },
    });
  });

  document.getElementById('usersTable').addEventListener('click', e => {
    const editBtn = e.target.closest('.btn-edit');
    const toggleBtn = e.target.closest('.btn-toggle');

    if (editBtn) {
      const user = usuarios.find(u => u.id === editBtn.dataset.id);
      if (!user) return;
      new ModalForm({
        title: `Editar usuario - ${user.nombre}`,
        fields: userFields(true),
        initialData: { ...user, activo: user.estado === 'Activo', centro1: user.centroIds[0] || '', centro2: user.centroIds[1] || '' },
        onSave: async data => {
          try {
            await PEIA.request(`/api/usuarios/${user.id}`, { method: 'PUT', body: JSON.stringify(buildPayload(data, true)) });
            await loadData();
          } catch (error) {
            PEIA.toast.error(error.message);
          }
        },
      });
    }

    if (toggleBtn) {
      const user = usuarios.find(u => u.id === toggleBtn.dataset.id);
      if (!user) return;
      PEIA.request(`/api/usuarios/${user.id}`, {
        method: 'PUT',
        body: JSON.stringify({
          userName: user.userName,
          email: user.email,
          nombreCompleto: user.nombre,
          password: '',
          activo: user.estado !== 'Activo',
          roles: user.roles,
          centroIds: user.centroIds
        })
      }).then(loadData).catch(error => PEIA.toast.error(error.message));
    }
  });

  try {
    await loadData();
  } catch (error) {
    PEIA.toast.error(error.message);
  }
});
