// PEIA — Notificaciones JS
// Conectado a /api/notificaciones + SignalR en vivo

document.addEventListener('DOMContentLoaded', async () => {
  if (!PEIA.requireAuth()) return;
  PEIA.hydrateShell();
  PEIA.bindWarehouseSelector();

  let notificaciones = [];
  let activeFilter = 'todas';

  const iconSVGs = {
    stock: '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>',
    sla: '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>',
    pedido: '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M6 2 3 6v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6l-3-4z"/><line x1="3" y1="6" x2="21" y2="6"/><path d="M16 10a4 4 0 0 1-8 0"/></svg>',
    camara: '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M23 19a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h4l2-3h6l2 3h4a2 2 0 0 1 2 2z"/><circle cx="12" cy="13" r="4"/></svg>',
    sistema: '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="3"/><path d="M19.07 4.93a10 10 0 0 1 0 14.14M4.93 4.93a10 10 0 0 0 0 14.14"/></svg>',
  };

  function timeAgo(fecha) {
    const diffMs = Date.now() - new Date(fecha).getTime();
    const min = Math.floor(diffMs / 60000);
    if (min < 1) return 'Justo ahora';
    if (min < 60) return `Hace ${min} minuto${min === 1 ? '' : 's'}`;
    const hrs = Math.floor(min / 60);
    if (hrs < 24) return `Hace ${hrs} hora${hrs === 1 ? '' : 's'}`;
    const dias = Math.floor(hrs / 24);
    if (dias === 1) return 'Ayer';
    return `Hace ${dias} días`;
  }

  function mapNotificacion(n) {
    return {
      id: n.id,
      tipo: n.tipo,
      titulo: n.titulo,
      desc: n.descripcion || '',
      fecha: n.fecha || n.fechaCreacion,
      leida: n.leida ?? false,
    };
  }

  async function loadData() {
    const centro = PEIA.getActiveCentro();
    if (!centro?.id) throw new Error('Selecciona un centro activo.');
    const data = await PEIA.request(`/api/notificaciones?centroId=${centro.id}`);
    notificaciones = data.map(mapNotificacion);
    renderNotifications();
  }

  function renderNotifications() {
    const list = document.getElementById('notifList');
    let filtered = [...notificaciones];

    if (activeFilter === 'no-leidas') filtered = filtered.filter(n => !n.leida);
    else if (activeFilter !== 'todas') filtered = filtered.filter(n => n.tipo === activeFilter);

    if (filtered.length === 0) {
      list.innerHTML = `
        <div class="empty-state">
          <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9"/><path d="M13.73 21a2 2 0 0 1-3.46 0"/></svg>
          <h3>Sin notificaciones</h3>
          <p>No hay notificaciones que coincidan con el filtro seleccionado.</p>
        </div>`;
      updateBadge();
      return;
    }

    list.innerHTML = filtered.map(n => `
      <div class="notif-item ${n.leida ? '' : 'unread'}" data-id="${n.id}">
        <div class="notif-icon ${n.tipo}">${iconSVGs[n.tipo] || iconSVGs.sistema}</div>
        <div class="notif-body">
          <div class="notif-title">${n.titulo}</div>
          <div class="notif-desc">${n.desc}</div>
          <div class="notif-time">
            <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>
            ${timeAgo(n.fecha)}
          </div>
        </div>
        <div class="notif-actions">
          ${!n.leida ? `<button class="btn-icon btn-mark-read" data-id="${n.id}" title="Marcar como leída">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polyline points="20 6 9 17 4 12"/></svg>
          </button>` : ''}
          <button class="btn-icon danger btn-delete" data-id="${n.id}" title="Eliminar">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/></svg>
          </button>
        </div>
      </div>
    `).join('');

    updateBadge();
  }

  function updateBadge() {
    const unread = notificaciones.filter(n => !n.leida).length;
    const badge = document.querySelector('a[href="/Notificaciones"] .badge');
    if (badge) {
      badge.textContent = unread;
      badge.style.display = unread > 0 ? '' : 'none';
    }
  }

  // Filter tabs
  document.querySelectorAll('[data-filter]').forEach(btn => {
    btn.addEventListener('click', () => {
      document.querySelectorAll('[data-filter]').forEach(b => b.classList.remove('active'));
      btn.classList.add('active');
      activeFilter = btn.dataset.filter;
      renderNotifications();
    });
  });

  // Mark all as read
  document.getElementById('btnMarkAll').addEventListener('click', async () => {
    const centro = PEIA.getActiveCentro();
    try {
      await PEIA.request(`/api/notificaciones/marcar-todas?centroId=${centro.id}`, { method: 'PUT' });
      notificaciones.forEach(n => n.leida = true);
      renderNotifications();
    } catch (error) {
      PEIA.toast.error(error.message);
    }
  });

  // Event delegation for mark read / delete
  document.getElementById('notifList').addEventListener('click', async (e) => {
    const markBtn = e.target.closest('.btn-mark-read');
    const delBtn = e.target.closest('.btn-delete');

    if (markBtn) {
      const id = markBtn.dataset.id;
      try {
        await PEIA.request(`/api/notificaciones/${id}/leer`, { method: 'PUT' });
        const notif = notificaciones.find(n => n.id === id);
        if (notif) { notif.leida = true; renderNotifications(); }
      } catch (error) {
        PEIA.toast.error(error.message);
      }
    }

    if (delBtn) {
      const id = delBtn.dataset.id;
      try {
        await PEIA.request(`/api/notificaciones/${id}`, { method: 'DELETE' });
        notificaciones = notificaciones.filter(n => n.id !== id);
        renderNotifications();
      } catch (error) {
        PEIA.toast.error(error.message);
      }
    }
  });

  window.addEventListener('peia:centro-changed', () => loadData().catch(error => PEIA.toast.error(error.message)));

  async function connectLive() {
    const hub = await PEIA.connectHub().catch(() => null);
    if (!hub) return;
    hub.on('notificacion', payload => {
      const centro = PEIA.getActiveCentro();
      if (centro?.id && payload.centroId && payload.centroId !== centro.id) return;
      notificaciones.unshift(mapNotificacion({
        id: payload.id,
        tipo: payload.tipo,
        titulo: payload.titulo,
        descripcion: payload.descripcion,
        fecha: payload.fecha,
        leida: false,
      }));
      renderNotifications();
    });
  }

  try {
    await loadData();
    await connectLive();
  } catch (error) {
    PEIA.toast.error(error.message);
  }
});
