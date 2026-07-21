'use strict';

document.addEventListener('DOMContentLoaded', async () => {
  if (!PEIA.requireAuth()) return;
  PEIA.hydrateShell();
  PEIA.bindWarehouseSelector();

  const elements = {
    body: document.getElementById('tbodyBitacora'), info: document.getElementById('bitacoraInfo'),
    previous: document.getElementById('btnPrevBit'), next: document.getElementById('btnNextBit'),
    search: document.getElementById('searchBitacora'), module: document.getElementById('filterModulo'),
    level: document.getElementById('filterNivel'), from: document.getElementById('fechaDesde'), to: document.getElementById('fechaHasta'),
    export: document.getElementById('btnExportCSV')
  };
  let page = 1;
  const pageSize = 10;

  const escape = value => String(value ?? '').replace(/[&<>'"]/g, char => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' })[char]);
  const initials = name => name.split(' ').map(part => part[0]).join('').slice(0, 2).toUpperCase();
  const moduleClass = value => ({ 'Inventario': 'inventario', 'Pedidos': 'pedidos', 'Notificaciones': 'reportes' }[value] || 'config');
  const formatDate = value => new Intl.DateTimeFormat('es-MX', { dateStyle: 'short', timeStyle: 'medium' }).format(new Date(value));

  function filters() {
    const params = new URLSearchParams({ page, pageSize });
    const values = { modulo: elements.module.value, nivel: elements.level.value, query: elements.search.value.trim(), desde: elements.from.value, hasta: elements.to.value };
    Object.entries(values).forEach(([key, value]) => { if (value) params.set(key, value); });
    return params;
  }

  function render(items) {
    if (!items.length) {
      elements.body.innerHTML = '<tr><td colspan="8" class="empty-cell">No hay eventos con los filtros seleccionados.</td></tr>';
      return;
    }
    elements.body.innerHTML = items.map(item => `<tr>
      <td class="cell-mono">${formatDate(item.fecha)}</td><td><div class="user-cell"><div class="avatar-sm">${escape(initials(item.usuario))}</div><span>${escape(item.usuario)}</span></div></td>
      <td><span class="module-badge module-${moduleClass(item.modulo)}">${escape(item.modulo)}</span></td><td class="cell-primary">${escape(item.accion)}</td>
      <td class="cell-detail" title="${escape(item.detalle)}">${escape(item.detalle)}</td><td>${escape(item.centro)}</td>
      <td><span class="level-badge level-${item.nivel.toLowerCase()}">${escape(item.nivel)}</span></td><td class="cell-mono small">—</td></tr>`).join('');
  }

  async function load() {
    try {
      const result = await PEIA.request(`/api/bitacora?${filters()}`);
      render(result.items);
      const first = result.total ? ((result.page - 1) * result.pageSize) + 1 : 0;
      elements.info.textContent = `Mostrando ${first}-${Math.min(result.page * result.pageSize, result.total)} de ${result.total} eventos`;
      elements.previous.disabled = result.page <= 1;
      elements.next.disabled = result.page * result.pageSize >= result.total;
      elements.export.disabled = result.total === 0;
    } catch (error) {
      elements.body.innerHTML = '<tr><td colspan="8" class="empty-cell">No se pudo cargar la bitácora.</td></tr>';
      PEIA.toast.error(error.message);
    }
  }

  try {
    const summary = await PEIA.request('/api/bitacora/resumen');
    document.getElementById('kpi-eventos-hoy').textContent = summary.eventosHoy;
    document.getElementById('kpi-errores-hoy').textContent = summary.erroresHoy;
    document.getElementById('kpi-usuarios-activos').textContent = summary.usuariosActivos;
    document.getElementById('kpi-eventos-mes').textContent = summary.eventosMes;
    document.querySelectorAll('.pending-kpi').forEach(element => element.classList.remove('pending-kpi'));
  } catch { PEIA.toast.error('No se pudo cargar el resumen de bitácora.'); }

  let searchTimer;
  elements.search.addEventListener('input', () => { clearTimeout(searchTimer); searchTimer = setTimeout(() => { page = 1; load(); }, 250); });
  [elements.module, elements.level, elements.from, elements.to].forEach(element => element.addEventListener('change', () => { page = 1; load(); }));
  elements.previous.addEventListener('click', () => { page--; load(); });
  elements.next.addEventListener('click', () => { page++; load(); });
  elements.export.addEventListener('click', async () => {
    try {
      const originalText = elements.export.innerHTML;
      elements.export.innerHTML = '<span class="spinner"></span> Exportando...';
      elements.export.disabled = true;

      const csvContent = await PEIA.request(`/api/bitacora/export?${filters()}`);
      
      // Añadir BOM (Byte Order Mark) para que Excel interprete correctamente los acentos (UTF-8)
      const blob = new Blob(['\uFEFF' + csvContent], { type: 'text/csv;charset=utf-8;' });
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `bitacora-${new Date().toISOString().split('T')[0].replace(/-/g, '')}.csv`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      URL.revokeObjectURL(url);

      elements.export.innerHTML = originalText;
      elements.export.disabled = false;
    } catch (e) {
      elements.export.innerHTML = 'Exportar Excel';
      elements.export.disabled = false;
      PEIA.toast.error('Error al exportar: ' + e.message);
    }
  });
  await load();
});
