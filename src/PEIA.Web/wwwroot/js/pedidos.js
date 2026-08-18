// PEIA — Lógica del Módulo de Pedidos, Rutas y SLAs

document.addEventListener('DOMContentLoaded', () => {
  if (!PEIA.requireAuth()) return;

  // Elementos globales de datos
  let rutasCache = [];
  let transportistasCache = [];

  PEIA.hydrateShell();
  PEIA.bindWarehouseSelector();

  // ══════════════════════════════════
  // MODALS CONTROL
  // ══════════════════════════════════
  function openModal(id) {
    document.getElementById(id)?.classList.add('open');
  }

  function closeModal(id) {
    document.getElementById(id)?.classList.remove('open');
  }

  // Escuchar botones de cierre
  document.querySelectorAll('[data-close]').forEach(btn => {
    btn.addEventListener('click', () => {
      closeModal(btn.getAttribute('data-close'));
    });
  });

  // Cerrar al hacer clic en el backdrop oscuro
  document.querySelectorAll('.modal-overlay').forEach(overlay => {
    overlay.addEventListener('click', (e) => {
      if (e.target === overlay) {
        overlay.classList.remove('open');
      }
    });
  });

  // Cerrar modal con la tecla Escape
  document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') {
      document.querySelectorAll('.modal-overlay.open').forEach(modal => {
        modal.classList.remove('open');
      });
    }
  });

  // Botones para abrir modals de creación
  document.getElementById('btnOpenNewPedido')?.addEventListener('click', () => {
    document.getElementById('formNewPedido')?.reset();
    const deadline = new Date(Date.now() + 24 * 60 * 60 * 1000);
    document.getElementById('pedFechaSla').value = deadline.toISOString().slice(0, 16);
    openModal('modalNewPedido');
  });

  document.getElementById('btnOpenNewRuta')?.addEventListener('click', () => {
    document.getElementById('formNewRuta')?.reset();
    openModal('modalNewRuta');
  });

  // ══════════════════════════════════
  // ENLACES API / FETCH DATA
  // ══════════════════════════════════
  async function apiFetch(url, options = {}) {
    return PEIA.request(url, options);
  }

  // Carga inicial
  async function loadAllData() {
    try {
      await Promise.all([
        loadRutas(),
        loadTransportistas()
      ]);
      await loadPedidos(); // Requiere rutas y transportistas para mostrar nombres correctos
    } catch (err) {
      PEIA.toast.error(`Error al cargar datos: ${err.message}`);
    }
  }

  async function loadRutas() {
    rutasCache = await apiFetch('/api/rutas') || [];
    renderRutas();
  }

  async function loadTransportistas() {
    transportistasCache = await apiFetch('/api/pedidos/transportistas') || [];
  }

  async function loadPedidos() {
    const centro = PEIA.getActiveCentro();
    if (!centro?.id) {
      PEIA.toast.info('Selecciona un almacén para ver los pedidos.');
      renderPedidos([]);
      calculateKPIs([]);
      return;
    }

    const pedidos = await apiFetch(`/api/pedidos?centroId=${centro.id}`) || [];
    renderPedidos(pedidos);
    calculateKPIs(pedidos);
  }

  // ══════════════════════════════════
  // RENDERIZADO DE TABLAS Y KPIS
  // ══════════════════════════════════

  function calculateKPIs(pedidos) {
    document.getElementById('kpi-total').textContent = pedidos.length;
    document.getElementById('kpi-pendientes').textContent = pedidos.filter(p => p.estado === 'Creado').length;
    document.getElementById('kpi-asignados').textContent = pedidos.filter(p => p.estado === 'Asignado').length;
    document.getElementById('kpi-enruta').textContent = pedidos.filter(p => p.estado === 'EnRuta').length;
    document.getElementById('kpi-entregados').textContent = pedidos.filter(p => p.estado === 'Entregado').length;
  }

  function renderRutas() {
    const tbody = document.getElementById('tbodyRutas');
    tbody.innerHTML = '';
    if (rutasCache.length === 0) {
      tbody.innerHTML = `<tr><td colspan="3" style="text-align:center; color:#9ca3af;">No hay rutas activas</td></tr>`;
      return;
    }
    rutasCache.forEach(r => {
      tbody.innerHTML += `
        <tr>
          <td style="font-weight:600; color:#374151;">${r.nombre}</td>
          <td>${r.origen} ➔ ${r.destino}</td>
          <td style="font-weight:500;">${r.distanciaKm} km</td>
        </tr>`;
    });
  }

  function renderPedidos(pedidos) {
    const tbody = document.getElementById('tbodyPedidos');
    tbody.innerHTML = '';
    if (pedidos.length === 0) {
      tbody.innerHTML = `<tr><td colspan="7" style="text-align:center; color:#9ca3af; padding: 20px;">No hay pedidos registrados en esta bodega</td></tr>`;
      return;
    }

    const estadoPillMap = {
      'Creado': 'pill-orange',
      'Asignado': 'pill-blue',
      'EnRuta': 'pill-blue',
      'Entregado': 'pill-green',
      'Cancelado': 'pill-red'
    };

    pedidos.forEach(p => {
      const pillClass = estadoPillMap[p.estado] || 'pill-blue';
      const slaClass = `sla-status-${p.sla?.estadoSLA || 'EnRiesgo'}`;
      const rutaName = p.ruta ? p.ruta.nombre : `<span style="color:#9ca3af; font-style:italic;">No asignada</span>`;
      const driverName = p.transportista ? p.transportista.nombreCompleto : `<span style="color:#9ca3af; font-style:italic;">No asignado</span>`;

      // Determinar qué botones mostrar
      let actionButtons = `
        <button class="btn-action-sm btn-track" data-id="${p.id}">Detalles</button>
      `;

      if (p.estado === 'Creado') {
        actionButtons += `
          <button class="btn-action-sm btn-assign" data-id="${p.id}">Asignar</button>
        `;
      } else if (p.estado !== 'Entregado' && p.estado !== 'Cancelado') {
        actionButtons += `
          <button class="btn-action-sm btn-status" data-id="${p.id}" data-estado="${p.estado}">Actualizar</button>
        `;
      }

      tbody.innerHTML += `
        <tr>
          <td style="font-weight:600; color:#1d4ed8;">${p.codigo}</td>
          <td>${p.cliente}</td>
          <td>${rutaName}</td>
          <td>${driverName}</td>
          <td><span class="sla-status-badge ${slaClass}">${p.sla?.estadoSLA || 'EnRiesgo'}</span></td>
          <td><span class="pill ${pillClass}">${p.estado}</span></td>
          <td>
            <div class="actions-cell">
              ${actionButtons}
            </div>
          </td>
        </tr>`;
    });

    // Agregar Listeners a los botones de acción
    tbody.querySelectorAll('.btn-assign').forEach(btn => {
      btn.addEventListener('click', () => initAssignModal(btn.getAttribute('data-id')));
    });

    tbody.querySelectorAll('.btn-status').forEach(btn => {
      btn.addEventListener('click', () => initStatusModal(btn.getAttribute('data-id'), btn.getAttribute('data-estado')));
    });

    tbody.querySelectorAll('.btn-track').forEach(btn => {
      btn.addEventListener('click', () => showTrackingDetail(btn.getAttribute('data-id')));
    });
  }

  // ══════════════════════════════════
  // OPERACIONES Y SUBMITS
  // ══════════════════════════════════

  // 1. Submit: Nuevo Pedido
  document.getElementById('formNewPedido')?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const data = {
      codigo: document.getElementById('pedCodigo').value.trim(),
      cliente: document.getElementById('pedCliente').value.trim(),
      direccionEntrega: document.getElementById('pedDireccion').value.trim(),
      fechaEstimadaEntrega: new Date(document.getElementById('pedFechaSla').value).toISOString(),
      centroId: PEIA.getActiveCentro()?.id
    };

    closeModal('modalNewPedido');
    try {
      await apiFetch('/api/pedidos', {
        method: 'POST',
        body: JSON.stringify(data)
      });
      PEIA.toast.success('Pedido registrado correctamente.');
      loadPedidos();
    } catch (err) {
      PEIA.toast.error(`Error al registrar el pedido: ${err.message}`);
    }
  });

  // 2. Submit: Crear Ruta
  document.getElementById('formNewRuta')?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const data = {
      nombre: document.getElementById('rutaNombre').value.trim(),
      origen: document.getElementById('rutaOrigen').value.trim(),
      destino: document.getElementById('rutaDestino').value.trim(),
      distanciaKm: parseFloat(document.getElementById('rutaDistancia').value)
    };

    closeModal('modalNewRuta');
    try {
      await apiFetch('/api/rutas', {
        method: 'POST',
        body: JSON.stringify(data)
      });
      PEIA.toast.success('Ruta creada correctamente.');
      await loadRutas();
    } catch (err) {
      PEIA.toast.error(`Error al crear la ruta: ${err.message}`);
    }
  });

  // 3. Inicializar y Submit: Asignación de Pedido
  function initAssignModal(pedidoId) {
    const assignPedidoId = document.getElementById('assignPedidoId');
    const rSelect = document.getElementById('assignRuta');
    const dSelect = document.getElementById('assignDriver');
    if (!assignPedidoId || !rSelect || !dSelect) {
      PEIA.toast.info('Formulario de asignación no disponible en esta vista.');
      return;
    }

    assignPedidoId.value = pedidoId;

    // Poblar rutas
    rSelect.innerHTML = '<option value="">Seleccione una ruta...</option>';
    rutasCache.forEach(r => {
      rSelect.innerHTML += `<option value="${r.id}">${r.nombre} (${r.origen} - ${r.destino})</option>`;
    });

    // Poblar transportistas
    dSelect.innerHTML = '<option value="">Seleccione un transportista...</option>';
    transportistasCache.forEach(d => {
      dSelect.innerHTML += `<option value="${d.id}">${d.nombreCompleto} (${d.email})</option>`;
    });

    openModal('modalAssignPedido');
  }

  document.getElementById('formAssignPedido')?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const pedidoId = document.getElementById('assignPedidoId').value;
    const data = {
      rutaId: document.getElementById('assignRuta').value,
      transportistaId: document.getElementById('assignDriver').value
    };

    closeModal('modalAssignPedido');
    try {
      await apiFetch(`/api/pedidos/${pedidoId}/asignar`, {
        method: 'PUT',
        body: JSON.stringify(data)
      });
      PEIA.toast.success('Pedido asignado correctamente.');
      loadPedidos();
    } catch (err) {
      PEIA.toast.error(`Error al asignar pedido: ${err.message}`);
    }
  });

  // 4. Inicializar y Submit: Actualizar Estado
  function initStatusModal(pedidoId, estadoActual) {
    const updatePedidoId = document.getElementById('updatePedidoId');
    const form = document.getElementById('formUpdateEstado');
    const select = document.getElementById('updateEstadoSelect');
    if (!updatePedidoId || !form || !select) {
      PEIA.toast.info('Formulario de actualización no disponible en esta vista.');
      return;
    }

    updatePedidoId.value = pedidoId;
    form.reset();

    // Establecer opciones lógicas de estado
    select.innerHTML = '';
    
    if (estadoActual === 'Asignado') {
      select.innerHTML += `<option value="EnRuta">En Ruta</option>`;
    }
    select.innerHTML += `
      <option value="Entregado">Entregado</option>
      <option value="Cancelado">Cancelado</option>
    `;

    openModal('modalUpdateEstado');
  }

  document.getElementById('formUpdateEstado')?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const pedidoId = document.getElementById('updatePedidoId').value;
    const lat = document.getElementById('updateLat').value;
    const lon = document.getElementById('updateLon').value;

    const data = {
      estado: document.getElementById('updateEstadoSelect').value,
      descripcion: document.getElementById('updateDesc').value.trim(),
      latitud: lat ? parseFloat(lat) : null,
      longitud: lon ? parseFloat(lon) : null
    };

    closeModal('modalUpdateEstado');
    try {
      await apiFetch(`/api/pedidos/${pedidoId}/estado`, {
        method: 'PUT',
        body: JSON.stringify(data)
      });
      PEIA.toast.success('Estado actualizado correctamente.');
      loadPedidos();
    } catch (err) {
      PEIA.toast.error(`Error al actualizar estado: ${err.message}`);
    }
  });

  // 5. Cargar detalles y abrir línea de tiempo
  async function showTrackingDetail(pedidoId) {
    try {
      const p = await apiFetch(`/api/pedidos/${pedidoId}`);
      if (!p) return;

      // Código y SLA
      const trackPedidoCode = document.getElementById('trackPedidoCode');
      const badge = document.getElementById('trackSlaBadge');
      const trackSlaDetail = document.getElementById('trackSlaDetail');
      const timeline = document.getElementById('trackTimeline');
      if (!trackPedidoCode || !badge || !trackSlaDetail || !timeline) {
        PEIA.toast.info(`Pedido ${p.codigo}: ${p.cliente} (${p.estado})`);
        return;
      }

      trackPedidoCode.textContent = `Rastreo del Pedido: ${p.codigo}`;
      
      badge.textContent = p.sla?.estadoSLA || 'EnRiesgo';
      badge.className = `sla-status-badge sla-status-${p.sla?.estadoSLA || 'EnRiesgo'}`;
      
      const limitDate = new Date(p.sla?.tiempoLimite).toLocaleString();
      trackSlaDetail.textContent = `Fecha límite de SLA: ${limitDate}`;

      // Línea de tiempo
      timeline.innerHTML = '';

      if (!p.estadosRastreo || p.estadosRastreo.length === 0) {
        timeline.innerHTML = '<p style="color:#9ca3af; font-style:italic;">No hay historial de movimientos</p>';
      } else {
        p.estadosRastreo.forEach(h => {
          const formattedDate = new Date(h.fechaActualizacion).toLocaleString();
          let trackingClass = '';
          if (h.estado === 'Entregado') trackingClass = 'delivered';
          else if (h.estado === 'Cancelado') trackingClass = 'canceled';
          else if (h.estado === 'Creado') trackingClass = 'pending';

          let gpsText = '';
          if (h.latitud && h.longitud) {
            gpsText = `<div style="font-size:11px; color:#3b82f6; margin-top:2px;">📍 GPS: ${h.latitud}, ${h.longitud}</div>`;
          }

          timeline.innerHTML += `
            <div class="timeline-item ${trackingClass}">
              <div class="timeline-time">${formattedDate}</div>
              <div class="timeline-title">${h.estado}</div>
              <div class="timeline-desc">${h.descripcion || ''}</div>
              ${gpsText}
              <div style="font-size:11px; color:#9ca3af; margin-top:1px;">Actualizado por: ${h.actualizadoPor}</div>
            </div>
          `;
        });
      }

      openModal('modalTrackDetail');
    } catch (err) {
      PEIA.toast.error(`Error al cargar seguimiento: ${err.message}`);
    }
  }

  // ══════════════════════════════════
  // CIERRE DE SESIÓN
  // ══════════════════════════════════
  window.addEventListener('peia:centro-changed', () => {
    loadAllData();
  });

  // Ejecución inicial de carga
  loadAllData();
});
