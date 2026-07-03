/**
 * PEIA — bitacora.js
 * Shell frontend del módulo de Bitácora / Auditoría.
 *
 * TODO (Backend pendiente):
 *  - GET  /api/bitacora                        → listado paginado con filtros:
 *      ?modulo=&nivel=&usuario=&desde=&hasta=&page=&pageSize=
 *  - GET  /api/bitacora/resumen                → KPIs del día
 *      { eventosHoy, erroresHoy, usuariosActivos, eventosMes }
 *  - GET  /api/bitacora/export?formato=csv     → descarga CSV del resultado filtrado
 *
 * La tabla actual muestra filas de demostración hardcodeadas en el HTML.
 * Al implementar el backend, reemplazar #tbodyBitacora con datos reales.
 */

'use strict';

/* ── Warehouse selector ───────────────────────────── */
const warehouseSelector = document.querySelector('.warehouse-selector');
const warehouseDropdown = document.getElementById('warehouseDropdown');
const activeCentroEl    = document.getElementById('activeCentro');

if (warehouseSelector) {
  warehouseSelector.addEventListener('click', e => {
    e.stopPropagation();
    warehouseDropdown?.classList.toggle('open');
  });

  document.querySelectorAll('.warehouse-opt').forEach(opt => {
    opt.addEventListener('click', () => {
      document.querySelectorAll('.warehouse-opt').forEach(o => o.classList.remove('active'));
      opt.classList.add('active');
      if (activeCentroEl) activeCentroEl.textContent = opt.textContent;
      warehouseDropdown?.classList.remove('open');
    });
  });

  document.addEventListener('click', () => warehouseDropdown?.classList.remove('open'));
}

/* ── Logout ───────────────────────────────────────── */
document.getElementById('btnLogout')?.addEventListener('click', () => {
  // TODO: invalidar token JWT en backend antes de redirigir
  window.location.href = 'login.html';
});

/* ── Filtro de búsqueda en tabla de demostración ─── */
const searchInput = document.getElementById('searchBitacora');
const filterModulo = document.getElementById('filterModulo');
const filterNivel  = document.getElementById('filterNivel');

function applyFilters() {
  // TODO: cuando exista el backend, enviar los filtros al API en lugar de filtrar el DOM
  const query  = searchInput?.value.toLowerCase() ?? '';
  const modulo = filterModulo?.value ?? '';
  const nivel  = filterNivel?.value ?? '';

  document.querySelectorAll('#tbodyBitacora tr').forEach(row => {
    const text     = row.textContent.toLowerCase();
    const rowMod   = row.querySelector('.module-badge')?.textContent ?? '';
    const rowLevel = row.querySelector('.level-badge')?.textContent ?? '';

    const matchQuery  = !query  || text.includes(query);
    const matchModulo = !modulo || rowMod.includes(modulo);
    const matchNivel  = !nivel  || rowLevel.includes(nivel);

    row.style.display = (matchQuery && matchModulo && matchNivel) ? '' : 'none';
  });
}

searchInput?.addEventListener('input', applyFilters);
filterModulo?.addEventListener('change', applyFilters);
filterNivel?.addEventListener('change', applyFilters);

/* ── Botones deshabilitados: tooltip informativo ──── */
document.querySelectorAll('[disabled]').forEach(btn => {
  btn.title = 'Pendiente: implementar endpoint de backend';
});
