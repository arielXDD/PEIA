document.addEventListener('DOMContentLoaded', async () => {
  if (!PEIA.requireAuth()) return;
  PEIA.hydrateShell();
  PEIA.bindWarehouseSelector();

  let roles = [];

  const table = new DataTable(document.getElementById('rolesTable'), {
    columns: [
      { key: 'nombre', label: 'Rol', render: val => `<span class="cell-bold">${val}</span>` },
      { key: 'descripcion', label: 'Descripción', render: val => `<span style="max-width:320px;display:inline-block;line-height:1.4;">${val || 'Rol del sistema'}</span>` },
      { key: 'usuarios', label: 'Usuarios', render: val => `<span class="cell-bold">${val ?? '-'}</span>` },
      { key: 'permisos', label: 'Permisos', sortable: false, render: () => `<div class="permisos-grid"><span class="permiso-tag active">Según backend</span></div>` },
      { key: 'id', label: 'Acciones', sortable: false, render: val => `
        <div class="cell-actions">
          <button class="btn-icon btn-edit" data-id="${val}" title="Editar">
            <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
          </button>
          <button class="btn-icon danger btn-delete" data-id="${val}" title="Eliminar">
            <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"/></svg>
          </button>
        </div>` },
    ],
    data: [],
    pageSize: 10,
  });

  async function loadRoles() {
    const [rolesRaw, usuariosRaw] = await Promise.all([
      PEIA.request('/api/roles'),
      PEIA.request('/api/usuarios').catch(() => [])
    ]);
    roles = rolesRaw.map(r => ({
      ...r,
      descripcion: r.descripcion || 'Rol operativo de PEIA',
      usuarios: usuariosRaw.filter(u => (u.roles || []).includes(r.nombre)).length,
      permisos: []
    }));
    applySearch();
  }

  function applySearch() {
    const term = document.getElementById('searchRoles').value.toLowerCase();
    const filtered = roles.filter(r =>
      r.nombre.toLowerCase().includes(term) ||
      (r.descripcion || '').toLowerCase().includes(term)
    );
    table.setData(filtered);
  }

  document.getElementById('searchRoles').addEventListener('input', applySearch);

  document.getElementById('btnNuevoRol').addEventListener('click', () => {
    new ModalForm({
      title: 'Nuevo rol',
      fields: [
        { key: 'nombre', label: 'Nombre del rol', type: 'text', placeholder: 'Ej: Auditor', required: true },
        { key: 'descripcion', label: 'Descripción', type: 'textarea', placeholder: 'Descripción visible en la tabla' },
      ],
      onSave: async data => {
        try {
          await PEIA.request('/api/roles', { method: 'POST', body: JSON.stringify({ nombre: data.nombre }) });
          await loadRoles();
        } catch (error) {
          alert(error.message);
        }
      },
    });
  });

  document.getElementById('rolesTable').addEventListener('click', e => {
    const editBtn = e.target.closest('.btn-edit');
    const deleteBtn = e.target.closest('.btn-delete');

    if (editBtn) {
      const rol = roles.find(r => r.id === editBtn.dataset.id);
      if (!rol) return;
      new ModalForm({
        title: `Editar rol - ${rol.nombre}`,
        fields: [
          { key: 'nombre', label: 'Nombre del rol', type: 'text', required: true },
          { key: 'descripcion', label: 'Descripción', type: 'textarea' },
        ],
        initialData: rol,
        onSave: async data => {
          try {
            await PEIA.request(`/api/roles/${rol.id}`, { method: 'PUT', body: JSON.stringify({ nombre: data.nombre }) });
            await loadRoles();
          } catch (error) {
            alert(error.message);
          }
        },
      });
    }

    if (deleteBtn) {
      const rol = roles.find(r => r.id === deleteBtn.dataset.id);
      if (!rol || !confirm(`Eliminar rol ${rol.nombre}?`)) return;
      PEIA.request(`/api/roles/${rol.id}`, { method: 'DELETE' })
        .then(loadRoles)
        .catch(error => alert(error.message));
    }
  });

  try {
    await loadRoles();
  } catch (error) {
    alert(error.message);
  }
});
