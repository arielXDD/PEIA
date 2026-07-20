'use strict';

document.addEventListener('DOMContentLoaded', async () => {
  if (!PEIA.requireAuth()) return;
  PEIA.hydrateShell();
  PEIA.bindWarehouseSelector();

  const grid = document.getElementById('camarasGrid');
  let cameras = [];
  let selectedId = null;
  let refreshTimer = null;
  const escape = value => String(value ?? '').replace(/[&<>"']/g, char => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' })[char]);

  function simulatedFrame(camera) {
    const now = new Date().toLocaleTimeString('es-MX');
    const seed = (Date.now() / 1000 + camera.id * 17) % 100;
    const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 800 450"><defs><linearGradient id="g" x1="0" x2="1"><stop stop-color="#132238"/><stop offset="1" stop-color="#25466a"/></linearGradient></defs><rect width="800" height="450" fill="url(#g)"/><path d="M0 355L190 240l132 85 155-165 323 205v85H0z" fill="#0b1728"/><rect x="70" y="185" width="145" height="150" rx="4" fill="#344c62"/><rect x="92" y="208" width="42" height="42" fill="#e7bd65" opacity=".8"/><rect x="149" y="208" width="42" height="42" fill="#e7bd65" opacity=".65"/><path d="M480 318c35-55 72-55 108 0" stroke="#87a5b7" stroke-width="17" fill="none"/><circle cx="${250 + seed}" cy="330" r="18" fill="#d3974a"/><text x="28" y="42" fill="#eff6ff" font-family="monospace" font-size="20">${escape(camera.nombre).toUpperCase()} · SIMULACIÓN LOCAL</text><text x="28" y="416" fill="#bed5e5" font-family="monospace" font-size="18">${now} · CANAL ${camera.id.toString().padStart(2, '0')}</text></svg>`;
    return `data:image/svg+xml;charset=utf-8,${encodeURIComponent(svg)}`;
  }

  function feedMarkup(camera) {
    if (camera.simulated) return `<img class="camara-feed-img" src="${simulatedFrame(camera)}" alt="Simulación de ${escape(camera.nombre)}">`;
    if (camera.streamType === 'video' && camera.streamUrl) return `<video class="camara-feed-img" src="${escape(camera.streamUrl)}" autoplay muted playsinline></video>`;
    if (camera.snapshotUrl) return `<img class="camara-feed-img" src="${escape(camera.snapshotUrl)}?t=${Date.now()}" alt="${escape(camera.nombre)}">`;
    return '<div class="camara-feed-bg">Sin fuente configurada</div>';
  }

  function render() {
    grid.innerHTML = cameras.map(camera => `<article class="camara-card ${camera.id === selectedId ? 'selected' : ''}" data-id="${camera.id}"><div class="camara-feed">${feedMarkup(camera)}<div class="camara-tag">${escape(camera.nombre)} — ${escape(camera.zona)}</div><div class="camara-badge ${camera.online ? '' : 'offline'}">${camera.online ? 'En vivo' : 'Sin conexión'}</div></div><div class="camara-controls"><button class="ctrl-btn fullscreen-btn" data-id="${camera.id}" title="Pantalla completa">⛶</button><span class="ctrl-spacer"></span><span class="camara-name">${camera.simulated ? 'Simulación local' : escape(camera.host)}</span></div></article>`).join('');
    grid.querySelectorAll('.camara-card').forEach(card => card.addEventListener('click', () => selectCamera(Number(card.dataset.id))));
    grid.querySelectorAll('.fullscreen-btn').forEach(button => button.addEventListener('click', event => { event.stopPropagation(); grid.querySelector(`.camara-card[data-id="${button.dataset.id}"]`)?.requestFullscreen?.(); }));
    grid.querySelectorAll('img.camara-feed-img').forEach(image => image.addEventListener('error', () => { const camera = cameras.find(item => item.id === Number(image.closest('.camara-card').dataset.id)); if (camera) { camera.online = false; render(); } }, { once: true }));
  }

  function selectCamera(id) {
    selectedId = id;
    const camera = cameras.find(item => item.id === id);
    if (!camera) return;
    document.getElementById('detailEmpty').style.display = 'none';
    document.getElementById('detailContent').style.display = 'block';
    document.getElementById('detailTitle').textContent = `${camera.nombre} — ${camera.zona}`;
    document.getElementById('detailNombre').textContent = camera.nombre;
    document.getElementById('detailZona').textContent = `${camera.zona} (${camera.zonaDesc})`;
    document.getElementById('detailIp').textContent = camera.host;
    document.getElementById('detailRevision').textContent = camera.ultimaRevision;
    document.getElementById('detailEstado').textContent = camera.simulated ? 'Simulación activa' : camera.online ? 'Conectada' : 'Sin conexión';
    render();
  }

  async function loadCameras() {
    try {
      cameras = await PEIA.request('/api/camaras');
      render();
      if (cameras.length) selectCamera(selectedId || cameras[0].id);
    } catch (error) { PEIA.toast.error(`No se pudieron cargar las cámaras: ${error.message}`); }
  }

  document.getElementById('btnCapturar')?.addEventListener('click', async () => {
    const camera = cameras.find(item => item.id === selectedId);
    if (!camera) return;
    const frame = camera.simulated ? simulatedFrame(camera) : camera.snapshotUrl;
    if (!frame) return PEIA.toast.error('La cámara no expone una URL de captura. Configura SnapshotUrl.');
    const link = document.createElement('a'); link.href = frame; link.target = '_blank'; link.rel = 'noopener'; link.click();
    await PEIA.request(`/api/camaras/${camera.id}/capturar`, { method: 'POST' }).catch(() => null);
  });
  document.getElementById('btnReportar')?.addEventListener('click', async () => {
    if (!selectedId) return;
    const description = window.prompt('Describe la incidencia detectada:');
    if (!description?.trim()) return;
    try { await PEIA.request(`/api/camaras/${selectedId}/reportar-incidencia`, { method: 'POST', body: JSON.stringify({ descripcion: description.trim() }) }); PEIA.toast.success('Incidencia registrada.'); } catch (error) { PEIA.toast.error(error.message); }
  });
  document.getElementById('btnGrabaciones')?.addEventListener('click', () => PEIA.toast.info('Las grabaciones se consultan desde el NVR o gateway configurado para la cámara.'));

  await loadCameras();
  refreshTimer = window.setInterval(() => { cameras.filter(item => item.simulated || item.snapshotUrl).forEach(camera => { const image = grid.querySelector(`.camara-card[data-id="${camera.id}"] img`); if (image) image.src = camera.simulated ? simulatedFrame(camera) : `${camera.snapshotUrl}?t=${Date.now()}`; }); }, 2500);
  window.addEventListener('beforeunload', () => clearInterval(refreshTimer));
});
