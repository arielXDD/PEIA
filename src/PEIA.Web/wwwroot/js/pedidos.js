// PEIA — Lógica del Módulo de Pedidos, Rutas y SLAs

document.addEventListener('DOMContentLoaded', () => {
  const token = localStorage.getItem('peia_token');
  const user = JSON.parse(localStorage.getItem('peia_user') || '{}');

  // Redirigir al login si no está autenticado
  if (!token) {
    window.location.href = '/login.html';
    return;
  }

  // Elementos globales de datos
  let activeCentroId = '';
  let rutasCache = [];
  let transportistasCache = [];

  // Configurar Cabecera y Nombre de Usuario
  if (user.nombreCompleto) {
    document.getElementById('userName').textContent = user.nombreCompleto;
    const initials = user.nombreCompleto.split(' ').map(n => n[0]).join('').slice(0, 2).toUpperCase();
    document.getElementById('userAvatar').textContent = initials;
  }

  // ══════════════════════════════════
  // GESTIÓN DE CENTROS (BODEGAS)
  // ══════════════════════════════════
  const activeCentroEl = document.getElementById('activeCentro');
  const dropdownCentroEl = document.getElementById('warehouseDropdown');

  function initCentros() {
    dropdownCentroEl.innerHTML = '';
    if (user.centros && user.centros.length > 0) {
      user.centros.forEach((centro, index) => {
        const btn = document.createElement('button');
        btn.className = `warehouse-opt ${index === 0 ? 'active' : ''}`;
        btn.textContent = centro.nombre;
        btn.setAttribute('data-id', centro.id);
        dropdownCentroEl.appendChild(btn);

        if (index === 0) {
          activeCentroId = centro.id;
          activeCentroEl.textContent = centro.nombre;
        }
      });
    } else {
      activeCentroEl.textContent = "Sin centros asignados";
    }
  }

  initCentros();

  // Selector de bodega toggle
  const warehouseSelector = document.querySelector('.warehouse-selector');
  warehouseSelector.addEventListener('click', (e) => {
    e.stopPropagation();
    warehouseSelector.classList.toggle('open');
  });

  document.addEventListener('click', () => warehouseSelector.classList.remove('open'));

  // Cambiar bodega activa
  dropdownCentroEl.addEventListener('click', (e) => {
    if (e.target.classList.contains('warehouse-opt')) {
      document.querySelectorAll('.warehouse-opt').forEach(b => b.classList.remove('active'));
      e.target.classList.add('active');
      activeCentroEl.textContent = e.target.textContent;
      activeCentroId = e.target.getAttribute('data-id');
      warehouseSelector.classList.remove('open');
      loadAllData(); // Recargar datos del nuevo almacén
    }
  });

  // ══════════════════════════════════
  // MODALS CONTROL
  // ══════════════════════════════════
  function openModal(id) {
    document.getElementById(id).classList.add('open');
  }

  function closeModal(id) {
    document.getElementById(id).classList.remove('open');
  }

  // Escuchar botones de cierre
  document.querySelectorAll('[data-close]').forEach(btn => {
    btn.addEventListener('click', () => {
      closeModal(btn.getAttribute('data-close'));
    });
  });

  // Botones para abrir modals de creación
  document.getElementById('btnOpenNewPedido').addEventListener('click', () => {
    document.getElementById('formNewPedido').reset();
    openModal('modalNewPedido');
  });

  document.getElementById('btnOpenNewRuta').addEventListener('click', () => {
    document.getElementById('formNewRuta').reset();
    openModal('modalNewRuta');
  });

  // ══════════════════════════════════
  // ENLACES API / FETCH DATA
  // ══════════════════════════════════
  async function apiFetch(url, options = {}) {
    const headers = {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
      ...options.headers
    };
    const response = await fetch(url, { ...options, headers });
    
    if (response.status === 401) {
      localStorage.clear();
      window.location.href = '/login.html';
      return null;
    }

    if (!response.ok) {
      const err = await response.json().catch(() => ({ message: 'Error desconocido' }));
      throw new Error(err.message || 'Error en la petición');
    }

    if (response.status === 204) return null;
    return response.json();
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
      alert(`Error al cargar datos: ${err.message}`);
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
    const pedidos = await apiFetch(`/api/pedidos?centroId=${activeCentroId}`) || [];
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
  document.getElementById('formNewPedido').addEventListener('submit', async (e) => {
    e.preventDefault();
    const data = {
      codigo: document.getElementById('pedCodigo').value.trim(),
      cliente: document.getElementById('pedCliente').value.trim(),
      direccionEntrega: document.getElementById('pedDireccion').value.trim(),
      fechaEstimadaEntrega: new Date(document.getElementById('pedFechaSla').value).toISOString(),
      centroId: activeCentroId
    };

    try {
      await apiFetch('/api/pedidos', {
        method: 'POST',
        body: JSON.stringify(data)
      });
      closeModal('modalNewPedido');
      loadPedidos();
    } catch (err) {
      alert(`Error al registrar el pedido: ${err.message}`);
    }
  });

  // 2. Submit: Crear Ruta
  document.getElementById('formNewRuta').addEventListener('submit', async (e) => {
    e.preventDefault();
    const data = {
      nombre: document.getElementById('rutaNombre').value.trim(),
      origen: document.getElementById('rutaOrigen').value.trim(),
      destino: document.getElementById('rutaDestino').value.trim(),
      distanciaKm: parseFloat(document.getElementById('rutaDistancia').value)
    };

    try {
      await apiFetch('/api/rutas', {
        method: 'POST',
        body: JSON.stringify(data)
      });
      closeModal('modalNewRuta');
      loadRutas();
    } catch (err) {
      alert(`Error al crear la ruta: ${err.message}`);
    }
  });

  // 3. Inicializar y Submit: Asignación de Pedido
  function initAssignModal(pedidoId) {
    document.getElementById('assignPedidoId').value = pedidoId;

    // Poblar rutas
    const rSelect = document.getElementById('assignRuta');
    rSelect.innerHTML = '<option value="">Seleccione una ruta...</option>';
    rutasCache.forEach(r => {
      rSelect.innerHTML += `<option value="${r.id}">${r.nombre} (${r.origen} - ${r.destino})</option>`;
    });

    // Poblar transportistas
    const dSelect = document.getElementById('assignDriver');
    dSelect.innerHTML = '<option value="">Seleccione un transportista...</option>';
    transportistasCache.forEach(d => {
      dSelect.innerHTML += `<option value="${d.id}">${d.nombreCompleto} (${d.email})</option>`;
    });

    openModal('modalAssignPedido');
  }

  document.getElementById('formAssignPedido').addEventListener('submit', async (e) => {
    e.preventDefault();
    const pedidoId = document.getElementById('assignPedidoId').value;
    const data = {
      rutaId: document.getElementById('assignRuta').value,
      transportistaId: document.getElementById('assignDriver').value
    };

    try {
      await apiFetch(`/api/pedidos/${pedidoId}/asignar`, {
        method: 'PUT',
        body: JSON.stringify(data)
      });
      closeModal('modalAssignPedido');
      loadPedidos();
    } catch (err) {
      alert(`Error al asignar pedido: ${err.message}`);
    }
  });

  // 4. Inicializar y Submit: Actualizar Estado
  function initStatusModal(pedidoId, estadoActual) {
    document.getElementById('updatePedidoId').value = pedidoId;
    document.getElementById('formUpdateEstado').reset();

    // Establecer opciones lógicas de estado
    const select = document.getElementById('updateEstadoSelect');
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

  document.getElementById('formUpdateEstado').addEventListener('submit', async (e) => {
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

    try {
      await apiFetch(`/api/pedidos/${pedidoId}/estado`, {
        method: 'PUT',
        body: JSON.stringify(data)
      });
      closeModal('modalUpdateEstado');
      loadPedidos();
    } catch (err) {
      alert(`Error al actualizar estado: ${err.message}`);
    }
  });

  // 5. Cargar detalles y abrir línea de tiempo
  async function showTrackingDetail(pedidoId) {
    try {
      const p = await apiFetch(`/api/pedidos/${pedidoId}`);
      if (!p) return;

      // Código y SLA
      document.getElementById('trackPedidoCode').textContent = `Rastreo del Pedido: ${p.codigo}`;
      
      const badge = document.getElementById('trackSlaBadge');
      badge.textContent = p.sla?.estadoSLA || 'EnRiesgo';
      badge.className = `sla-status-badge sla-status-${p.sla?.estadoSLA || 'EnRiesgo'}`;
      
      const limitDate = new Date(p.sla?.tiempoLimite).toLocaleString();
      document.getElementById('trackSlaDetail').textContent = `Fecha límite de SLA: ${limitDate}`;

      // Línea de tiempo
      const timeline = document.getElementById('trackTimeline');
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
      alert(`Error al cargar seguimiento: ${err.message}`);
    }
  }

  // ══════════════════════════════════
  // CIERRE DE SESIÓN
  // ══════════════════════════════════
  document.getElementById('btnLogout').addEventListener('click', () => {
    localStorage.clear();
    window.location.href = '/login.html';
  });

  // Ejecución inicial de carga
  loadAllData();
});
