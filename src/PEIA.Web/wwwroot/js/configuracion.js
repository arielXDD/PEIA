/**
 * PEIA — configuracion.js
 * Shell frontend del módulo de Configuración.
 *
 * TODO (Backend pendiente):
 *  - GET  /api/configuracion/empresa           → cargar datos de la empresa
 *  - PUT  /api/configuracion/empresa           → guardar datos de la empresa
 *  - GET  /api/centros                         → listar centros
 *  - POST /api/centros                         → crear nuevo centro
 *  - PUT  /api/centros/{id}                    → editar centro
 *  - GET  /api/configuracion/preferencias      → cargar preferencias del sistema
 *  - PUT  /api/configuracion/preferencias      → guardar preferencias
 *  - GET  /api/configuracion/notificaciones    → cargar preferencias de notificaciones
 *  - PUT  /api/configuracion/notificaciones    → guardar preferencias de notificaciones
 *  - PUT  /api/configuracion/seguridad         → guardar política de contraseñas
 *  - GET  /api/auth/sesiones                   → listar sesiones activas
 *  - DELETE /api/auth/sesiones/{id}            → cerrar sesión específica
 *  - GET  /api/sistema/salud                   → estado del sistema (DB, SignalR, migraciones)
 */

'use strict';

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

/* ── Botones deshabilitados: tooltip informativo ──── */
document.querySelectorAll('[disabled]').forEach(btn => {
  btn.title = 'Pendiente: implementar endpoint de backend';
});
