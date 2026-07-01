// PEIA — Notificaciones JS
// Mock notifications, filters, mark as read

document.addEventListener('DOMContentLoaded', () => {

  const notificaciones = [
    { id: 1, tipo: 'stock',  titulo: 'Stock bajo: Cinta Adhesiva 48mm',           desc: 'Quedan 25 unidades — el mínimo es 50. Se recomienda realizar un pedido de reabastecimiento.', tiempo: 'Hace 12 minutos',  leida: false },
    { id: 2, tipo: 'camara', titulo: 'Movimiento no autorizado — Cámara 2',       desc: 'Se detectó actividad fuera de horario en el Área C-07 a las 03:42 AM. Revisar grabación.', tiempo: 'Hace 35 minutos',  leida: false },
    { id: 3, tipo: 'sla',    titulo: 'SLA próximo a vencer: PED-0044',             desc: 'El pedido PED-0044 de LogExpress S.A. tiene plazo hasta mañana 12:00 PM. Estado actual: Pendiente.', tiempo: 'Hace 1 hora',  leida: false },
    { id: 4, tipo: 'stock',  titulo: 'Producto agotado: Guantes de Nitrilo L',     desc: 'El stock de Guantes de Nitrilo talla L está en 0 unidades. Prioridad alta de reabastecimiento.', tiempo: 'Hace 2 horas', leida: true },
    { id: 5, tipo: 'pedido', titulo: 'Nuevo pedido recibido: PED-0045',            desc: 'Distribuciones del Sur ha realizado un pedido con 8 items por un total de $12,500.', tiempo: 'Hace 3 horas', leida: true },
    { id: 6, tipo: 'stock',  titulo: 'Stock bajo: Desengrasante Industrial',       desc: 'Quedan 18 unidades — el mínimo es 25. Ubicación: F-03-02.', tiempo: 'Hace 4 horas', leida: true },
    { id: 7, tipo: 'camara', titulo: 'Temperatura elevada en Zona B',              desc: 'Sensor 03 reporta 28°C en la Zona B del almacén. Temperatura máxima permitida: 25°C.', tiempo: 'Hace 5 horas', leida: true },
    { id: 8, tipo: 'pedido', titulo: 'Pedido completado: PED-0041',                desc: 'El pedido de SuperMarket Plus ha sido entregado exitosamente. 7 items por $15,300.', tiempo: 'Hace 6 horas', leida: true },
    { id: 9, tipo: 'sla',    titulo: 'SLA cumplido: PED-0040',                     desc: 'El pedido PED-0040 se entregó dentro del plazo acordado. Cumplimiento: 100%.', tiempo: 'Ayer 16:30', leida: true },
    { id: 10, tipo: 'sistema', titulo: 'Respaldo de base de datos completado',     desc: 'El respaldo automático de la base de datos peiadb se completó exitosamente a las 02:00 AM.', tiempo: 'Ayer 02:00', leida: true },
  ];

  const iconSVGs = {
    stock: '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>',
    sla: '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>',
    pedido: '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M6 2 3 6v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6l-3-4z"/><line x1="3" y1="6" x2="21" y2="6"/><path d="M16 10a4 4 0 0 1-8 0"/></svg>',
    camara: '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M23 19a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h4l2-3h6l2 3h4a2 2 0 0 1 2 2z"/><circle cx="12" cy="13" r="4"/></svg>',
    sistema: '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="3"/><path d="M19.07 4.93a10 10 0 0 1 0 14.14M4.93 4.93a10 10 0 0 0 0 14.14"/></svg>',
  };

  let activeFilter = 'todas';

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
            ${n.tiempo}
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
    const badge = document.getElementById('navBadge');
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
  document.getElementById('btnMarkAll').addEventListener('click', () => {
    notificaciones.forEach(n => n.leida = true);
    renderNotifications();
  });

  // Event delegation for mark read / delete
  document.getElementById('notifList').addEventListener('click', (e) => {
    const markBtn = e.target.closest('.btn-mark-read');
    const delBtn = e.target.closest('.btn-delete');

    if (markBtn) {
      const id = parseInt(markBtn.dataset.id);
      const notif = notificaciones.find(n => n.id === id);
      if (notif) { notif.leida = true; renderNotifications(); }
    }

    if (delBtn) {
      const id = parseInt(delBtn.dataset.id);
      const idx = notificaciones.findIndex(n => n.id === id);
      if (idx > -1) { notificaciones.splice(idx, 1); renderNotifications(); }
    }
  });

  // Initial render
  renderNotifications();

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
