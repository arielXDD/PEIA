/**
 * PEIA — configuracion.js
 * Conectado a /api/configuracion, /api/centros y /api/sistema/salud.
 */

'use strict';

document.addEventListener('DOMContentLoaded', async () => {
  if (!PEIA.requireAuth()) return;
  PEIA.hydrateShell();
  PEIA.bindWarehouseSelector();

  /* ── Tabs ─────────────────────────────────────────── */
  document.querySelectorAll('.tab-btn').forEach(btn => {
    btn.addEventListener('click', () => {
      const target = btn.dataset.tab;

      document.querySelectorAll('.tab-btn').forEach(b => {
        b.classList.remove('active');
        b.setAttribute('aria-selected', 'false');
      });
      document.querySelectorAll('.tab-panel').forEach(p => p.classList.remove('active'));

      btn.classList.add('active');
      btn.setAttribute('aria-selected', 'true');
      document.getElementById(`panel-${target}`)?.classList.add('active');
    });
  });

  async function safeLoad(path) {
    try {
      return await PEIA.request(path);
    } catch (error) {
      console.warn(`No se pudo cargar ${path}:`, error.message);
      return null;
    }
  }

  /* ── Empresa ──────────────────────────────────────── */
  async function loadEmpresa() {
    const empresa = await safeLoad('/api/configuracion/empresa');
    if (!empresa) return;
    document.getElementById('empresaNombre').value = empresa.nombre || '';
    document.getElementById('empresaRfc').value = empresa.rfc || '';
    document.getElementById('empresaTel').value = empresa.telefono || '';
    document.getElementById('empresaDireccion').value = empresa.direccion || '';
    document.getElementById('empresaEmail').value = empresa.email || '';
  }

  document.getElementById('btnGuardarEmpresa').addEventListener('click', async () => {
    const payload = {
      nombre: document.getElementById('empresaNombre').value.trim(),
      rfc: document.getElementById('empresaRfc').value.trim() || null,
      telefono: document.getElementById('empresaTel').value.trim() || null,
      direccion: document.getElementById('empresaDireccion').value.trim() || null,
      email: document.getElementById('empresaEmail').value.trim() || null,
    };
    try {
      await PEIA.request('/api/configuracion/empresa', { method: 'PUT', body: JSON.stringify(payload) });
      alert('Datos de la empresa guardados.');
    } catch (error) {
      alert(error.message);
    }
  });

  /* ── Centros ──────────────────────────────────────── */
  async function loadCentros() {
    const centros = await safeLoad('/api/centros');
    const list = document.getElementById('centrosList');
    if (!centros) {
      list.innerHTML = '<p style="color:var(--text-muted);font-size:13px;">No se pudieron cargar los centros.</p>';
      return;
    }

    list.innerHTML = centros.map(c => `
      <div class="centro-item" data-id="${c.id}">
        <div class="centro-info">
          <span class="centro-name">${c.nombre} <span style="color:var(--text-muted);font-weight:400;">(${c.codigo})</span></span>
          <span class="status-badge ${c.activo ? 'status-active' : 'status-inactive'}"><span class="status-dot-sm"></span>${c.activo ? 'Activo' : 'Inactivo'}</span>
        </div>
        <button class="btn-icon btn-edit-centro" title="Editar" data-id="${c.id}">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
        </button>
      </div>`).join('') || '<p style="color:var(--text-muted);font-size:13px;">Sin centros registrados.</p>';

    list.querySelectorAll('.btn-edit-centro').forEach(btn => {
      btn.addEventListener('click', () => {
        const centro = centros.find(c => c.id === btn.dataset.id);
        if (!centro) return;
        new ModalForm({
          title: `Editar — ${centro.nombre}`,
          fields: centroFields(),
          initialData: centro,
          onSave: async data => {
            try {
              await PEIA.request(`/api/centros/${centro.id}`, {
                method: 'PUT',
                body: JSON.stringify({ nombre: data.nombre, codigo: data.codigo, direccion: data.direccion, activo: data.activo !== false }),
              });
              await loadCentros();
            } catch (error) {
              alert(error.message);
            }
          },
        });
      });
    });
  }

  function centroFields() {
    return [
      { key: 'nombre', label: 'Nombre', type: 'text', required: true, half: true },
      { key: 'codigo', label: 'Código', type: 'text', required: true, half: true },
      { key: 'direccion', label: 'Dirección', type: 'text' },
      { key: 'activo', label: 'Activo', type: 'checkbox', value: true },
    ];
  }

  document.getElementById('btnNuevoCentro').addEventListener('click', () => {
    new ModalForm({
      title: 'Nuevo centro',
      fields: centroFields(),
      onSave: async data => {
        try {
          await PEIA.request('/api/centros', {
            method: 'POST',
            body: JSON.stringify({ nombre: data.nombre, codigo: data.codigo, direccion: data.direccion, activo: data.activo !== false }),
          });
          await loadCentros();
        } catch (error) {
          alert(error.message);
        }
      },
    });
  });

  /* ── Preferencias ─────────────────────────────────── */
  async function loadPreferencias() {
    const prefs = await safeLoad('/api/configuracion/preferencias');
    if (!prefs) return;
    document.getElementById('prefTimezone').value = prefs.timezone || 'America/Mexico_City';
    document.getElementById('prefDateFmt').value = prefs.dateFormat || 'DD/MM/YYYY';
    document.getElementById('prefCurrency').value = prefs.currency || 'MXN';
    document.getElementById('prefPageSize').value = String(prefs.pageSize || 10);
  }

  document.getElementById('btnGuardarPrefs').addEventListener('click', async () => {
    const payload = {
      timezone: document.getElementById('prefTimezone').value,
      dateFormat: document.getElementById('prefDateFmt').value,
      currency: document.getElementById('prefCurrency').value,
      pageSize: Number(document.getElementById('prefPageSize').value),
    };
    try {
      await PEIA.request('/api/configuracion/preferencias', { method: 'PUT', body: JSON.stringify(payload) });
      alert('Preferencias guardadas.');
    } catch (error) {
      alert(error.message);
    }
  });

  /* ── Notificaciones (preferencias) ────────────────── */
  async function loadNotificacionesPrefs() {
    const prefs = await safeLoad('/api/configuracion/notificaciones');
    if (!prefs) return;
    document.getElementById('chk-stock').checked = !!prefs.stock;
    document.getElementById('chk-sla').checked = !!prefs.sla;
    document.getElementById('chk-pedido').checked = !!prefs.pedido;
    document.getElementById('chk-camara').checked = !!prefs.camara;
    document.getElementById('chk-email').checked = !!prefs.email;
  }

  document.getElementById('btnGuardarNotif').addEventListener('click', async () => {
    const payload = {
      stock: document.getElementById('chk-stock').checked,
      sla: document.getElementById('chk-sla').checked,
      pedido: document.getElementById('chk-pedido').checked,
      camara: document.getElementById('chk-camara').checked,
      email: document.getElementById('chk-email').checked,
    };
    try {
      await PEIA.request('/api/configuracion/notificaciones', { method: 'PUT', body: JSON.stringify(payload) });
      alert('Preferencias de notificaciones guardadas.');
    } catch (error) {
      alert(error.message);
    }
  });

  /* ── Seguridad ────────────────────────────────────── */
  async function loadSeguridad() {
    const seg = await safeLoad('/api/configuracion/seguridad');
    if (!seg) return;
    document.getElementById('pwdMinLen').value = String(seg.passwordMinLength || 8);
    document.getElementById('chk-pwdComplex').checked = !!(seg.requireUppercase || seg.requireDigit);
  }

  document.getElementById('btnGuardarSeg').addEventListener('click', async () => {
    const complex = document.getElementById('chk-pwdComplex').checked;
    const payload = {
      passwordMinLength: Number(document.getElementById('pwdMinLen').value),
      requireUppercase: complex,
      requireDigit: complex,
    };
    try {
      await PEIA.request('/api/configuracion/seguridad', { method: 'PUT', body: JSON.stringify(payload) });
      alert('Política de contraseñas guardada.');
    } catch (error) {
      alert(error.message);
    }
  });

  /* ── Sistema ──────────────────────────────────────── */
  async function loadSistema() {
    const salud = await safeLoad('/api/sistema/salud');
    if (!salud) {
      document.getElementById('sysDb').textContent = 'No disponible';
      document.getElementById('sysHub').textContent = 'No disponible';
      document.getElementById('sysMigracion').textContent = 'No disponible';
      return;
    }
    document.getElementById('sysEntorno').textContent = salud.ambiente;
    document.getElementById('sysDb').textContent = salud.baseDatos.conectada ? 'Conectada' : 'Sin conexión';
    document.getElementById('sysDb').classList.toggle('pending-value', !salud.baseDatos.conectada);
    document.getElementById('sysHub').textContent = salud.signalR.activo ? `Activo (${salud.signalR.hub})` : 'Inactivo';
    document.getElementById('sysHub').classList.toggle('pending-value', !salud.signalR.activo);
    const ultima = salud.baseDatos.migracionesAplicadas?.slice(-1)?.[0];
    document.getElementById('sysMigracion').textContent = ultima || 'Sin migraciones aplicadas';
    document.getElementById('sysMigracion').classList.toggle('pending-value', !ultima);
  }

  document.getElementById('btnRefreshSys').addEventListener('click', loadSistema);

  /* ── Reglas de Automatización ───────────────────────── */
  let reglasTableInstance = null;

  async function loadReglas() {
    const reglas = await safeLoad('/api/reglas-automatizacion');
    const tableContainer = document.getElementById('reglasTable');
    if (!tableContainer) return;

    if (!reglas) {
      tableContainer.innerHTML = '<p style="color:var(--text-muted);font-size:13px;">No se pudieron cargar las reglas.</p>';
      return;
    }

    const friendlyEventos = {
      stock_critico: 'Stock Crítico',
      nuevo_pedido: 'Nuevo Pedido',
      sla_vencido: 'SLA Vencido'
    };

    const friendlyAcciones = {
      notificar: 'Notificar en UI/SignalR',
      email: 'Enviar Correo'
    };

    if (reglasTableInstance) {
      reglasTableInstance.setData(reglas);
    } else {
      reglasTableInstance = new DataTable(tableContainer, {
        pageSize: 5,
        columns: [
          { key: 'nombre', label: 'Nombre' },
          { key: 'eventoOrigen', label: 'Evento', render: val => friendlyEventos[val] || val },
          { key: 'condicion', label: 'Condición' },
          { key: 'accion', label: 'Acción', render: val => friendlyAcciones[val] || val },
          { key: 'responsable', label: 'Responsable' },
          { key: 'activa', label: 'Estado', render: val => `<span class="status-badge ${val ? 'status-active' : 'status-inactive'}"><span class="status-dot-sm"></span>${val ? 'Activa' : 'Inactiva'}</span>` },
          { key: 'id', label: 'Acciones', sortable: false, render: val => `
            <div class="cell-actions">
              <button class="btn-icon btn-edit-regla" data-id="${val}" title="Editar">
                <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
              </button>
              <button class="btn-icon danger btn-delete-regla" data-id="${val}" title="Eliminar">
                <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/><line x1="10" y1="11" x2="10" y2="17"/><line x1="14" y1="11" x2="14" y2="17"/></svg>
              </button>
            </div>`
          }
        ],
        data: reglas
      });
    }

    // Enlazar eventos de editar y eliminar
    tableContainer.querySelectorAll('.btn-edit-regla').forEach(btn => {
      btn.addEventListener('click', async () => {
        const id = btn.dataset.id;
        try {
          const regla = await PEIA.request(`/api/reglas-automatizacion/${id}`);
          new ModalForm({
            title: `Editar regla — ${regla.nombre}`,
            fields: reglaFields(),
            initialData: regla,
            onSave: async data => {
              try {
                await PEIA.request(`/api/reglas-automatizacion/${id}`, {
                  method: 'PUT',
                  body: JSON.stringify({
                    nombre: data.nombre,
                    eventoOrigen: data.eventoOrigen,
                    condicion: data.condicion,
                    accion: data.accion,
                    responsable: data.responsable,
                    activa: data.activa !== false
                  })
                });
                await loadReglas();
              } catch (error) {
                alert(error.message);
              }
            }
          });
        } catch (error) {
          alert(error.message);
        }
      });
    });

    tableContainer.querySelectorAll('.btn-delete-regla').forEach(btn => {
      btn.addEventListener('click', async () => {
        const id = btn.dataset.id;
        if (confirm('¿Estás seguro de que deseas eliminar esta regla de automatización?')) {
          try {
            await PEIA.request(`/api/reglas-automatizacion/${id}`, { method: 'DELETE' });
            await loadReglas();
          } catch (error) {
            alert(error.message);
          }
        }
      });
    });
  }

  function reglaFields() {
    return [
      { key: 'nombre', label: 'Nombre de la regla', type: 'text', required: true },
      { key: 'eventoOrigen', label: 'Evento de origen', type: 'select', options: [
          { value: 'stock_critico', label: 'Stock Crítico' },
          { value: 'nuevo_pedido', label: 'Nuevo Pedido' },
          { value: 'sla_vencido', label: 'SLA Vencido' }
        ], required: true, half: true },
      { key: 'accion', label: 'Acción a ejecutar', type: 'select', options: [
          { value: 'notificar', label: 'Notificar en UI/SignalR' },
          { value: 'email', label: 'Enviar Correo Electrónico' }
        ], required: true, half: true },
      { key: 'condicion', label: 'Condición (Ej: Todos, o SKU=SKU123)', type: 'text', required: true, value: 'Todos' },
      { key: 'responsable', label: 'Responsable asignado', type: 'text', required: true },
      { key: 'activa', label: 'Activa', type: 'checkbox', value: true }
    ];
  }

  document.getElementById('btnNuevaRegla')?.addEventListener('click', () => {
    new ModalForm({
      title: 'Nueva regla de automatización',
      fields: reglaFields(),
      onSave: async data => {
        try {
          await PEIA.request('/api/reglas-automatizacion', {
            method: 'POST',
            body: JSON.stringify({
              nombre: data.nombre,
              eventoOrigen: data.eventoOrigen,
              condicion: data.condicion,
              accion: data.accion,
              responsable: data.responsable,
              activa: data.activa !== false
            })
          });
          await loadReglas();
        } catch (error) {
          alert(error.message);
        }
      }
    });
  });

  /* ── Logout ───────────────────────────────────────── */
  document.getElementById('btnLogout')?.addEventListener('click', () => {
    PEIA.logout();
  });

  await Promise.all([
    loadEmpresa(),
    loadCentros(),
    loadPreferencias(),
    loadNotificacionesPrefs(),
    loadSeguridad(),
    loadSistema(),
    loadReglas(),
  ]);
});
