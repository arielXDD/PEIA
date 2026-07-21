// PEIA — Cámaras JS
// Mock data + interactividad

document.addEventListener('DOMContentLoaded', () => {

  // ─── Mock Camera Data ────────────────────────
  let refreshIntervals = {};
  let modalInterval = null;

  const camaras = [
    {
      id: 1,
      nombre: 'Cámara 1',
      zona: 'Zona A',
      zonaDesc: 'Plaza Principal de Bernal, Qro',
      ip: 'webcamsdemexico.net',
      online: true,
      activa: true,
      ultimaRevision: 'En vivo',
      playing: true,
      feedUrl: 'https://webcamsdemexico.net/bernal1/live.jpg',
      feedLabel: 'Bernal, Querétaro — En vivo',
    },
    {
      id: 2,
      nombre: 'Cámara 2',
      zona: 'Zona B',
      zonaDesc: 'Plaza Principal de Tequisquiapan, Qro',
      ip: 'webcamsdemexico.net',
      online: true,
      activa: true,
      ultimaRevision: 'En vivo',
      playing: true,
      feedUrl: 'https://webcamsdemexico.net/tequisquiapan1/live.jpg',
      feedLabel: 'Tequisquiapan, Querétaro — En vivo',
    },
    {
      id: 3,
      nombre: 'Cámara 3',
      zona: 'Entrada',
      zonaDesc: 'Centro de San Joaquín, Qro',
      ip: 'webcamsdemexico.net',
      online: true,
      activa: true,
      ultimaRevision: 'En vivo',
      playing: true,
      feedUrl: 'https://webcamsdemexico.net/sanjoaquin1/live.jpg',
      feedLabel: 'San Joaquín, Querétaro — En vivo',
    },
    {
      id: 4,
      nombre: 'Cámara 4',
      zona: 'Despacho',
      zonaDesc: 'Centro de Amealco, Qro',
      ip: 'webcamsdemexico.net',
      online: true,
      activa: true,
      ultimaRevision: 'En vivo',
      playing: true,
      feedUrl: 'https://webcamsdemexico.net/amealco1/live.jpg',
      feedLabel: 'Amealco, Querétaro — En vivo',
    },
  ];

  let selectedId = null;

  // ─── Render Camera Cards ─────────────────────
  const grid = document.getElementById('camarasGrid');

  function renderCamaras() {
    grid.innerHTML = '';
    camaras.forEach(cam => {
      const card = document.createElement('div');
      card.className = 'camara-card' + (cam.id === selectedId ? ' selected' : '');
      card.dataset.id = cam.id;

      const badgeClass = cam.online ? '' : 'offline';
      const badgeText = cam.online ? 'En vivo' : 'Desconectada';

      let feedHtml;
      if (cam.feedUrl) {
        const ts = Date.now();
        feedHtml = `
          <img class="camara-feed-img" src="${cam.feedUrl}?t=${ts}" alt="${cam.nombre}" loading="lazy" />
          <span class="camara-feed-label">${cam.feedLabel || 'Feed en vivo'}</span>`;
      }

      card.innerHTML = `
        <div class="camara-feed" data-feed-id="${cam.id}">
          ${feedHtml}
          <div class="camara-tag">
            <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M23 19a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h4l2-3h6l2 3h4a2 2 0 0 1 2 2z"/><circle cx="12" cy="13" r="4"/></svg>
            ${cam.nombre} — ${cam.zona}
          </div>
          <div class="camara-badge ${badgeClass}">${badgeText}</div>
        </div>
        <div class="camara-controls">
          <button class="ctrl-btn play-btn ${cam.playing ? 'playing' : ''}" data-id="${cam.id}" title="Reproducir / Pausar">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              ${cam.playing
                ? '<rect x="6" y="4" width="4" height="16"/><rect x="14" y="4" width="4" height="16"/>'
                : '<polygon points="5 3 19 12 5 21 5 3"/>'}
            </svg>
          </button>
          <button class="ctrl-btn" title="Micrófono">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="9" y="2" width="6" height="11" rx="3"/><path d="M5 10a7 7 0 0 0 14 0"/><line x1="12" y1="19" x2="12" y2="22"/></svg>
          </button>
          <span class="ctrl-spacer"></span>
          <span class="camara-name">${cam.nombre}</span>
          <button class="ctrl-btn fullscreen-btn" data-id="${cam.id}" title="Pantalla completa">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M8 3H5a2 2 0 0 0-2 2v3m18 0V5a2 2 0 0 0-2-2h-3m0 18h3a2 2 0 0 0 2-2v-3M3 16v3a2 2 0 0 0 2 2h3"/></svg>
          </button>
        </div>
      `;

      card.addEventListener('click', (e) => {
        if (e.target.closest('.ctrl-btn')) return;
        selectCamera(cam.id);
      });

      grid.appendChild(card);
    });

    // Play button events
    document.querySelectorAll('.play-btn').forEach(btn => {
      btn.addEventListener('click', (e) => {
        e.stopPropagation();
        const id = parseInt(btn.dataset.id);
        togglePlay(id);
      });
    });

    // Fullscreen button events
    document.querySelectorAll('.fullscreen-btn').forEach(btn => {
      btn.addEventListener('click', (e) => {
        e.stopPropagation();
        const id = parseInt(btn.dataset.id);
        openFullscreen(id);
      });
    });

    // Double-click on card -> fullscreen
    document.querySelectorAll('.camara-card').forEach(card => {
      card.addEventListener('dblclick', (e) => {
        if (e.target.closest('.ctrl-btn')) return;
        const id = parseInt(card.dataset.id);
        openFullscreen(id);
      });
    });

    // Start feed refresh for cameras with feedUrl
    startFeedRefresh();

    if (selectedId) scrollToSelected();
  }

  // ─── Feed Refresh (live image polling) ──────
  function startFeedRefresh() {
    stopFeedRefresh();
    camaras.forEach(cam => {
      if (!cam.feedUrl) return;
      if (!cam.playing) return;
      const id = cam.id;
      const interval = setInterval(() => {
        const img = grid.querySelector(`.camara-feed[data-feed-id="${id}"] .camara-feed-img`);
        if (img) {
          img.src = `${cam.feedUrl}?t=${Date.now()}`;
        }
      }, 2000);
      refreshIntervals[id] = interval;
    });
  }

  function stopFeedRefresh() {
    Object.values(refreshIntervals).forEach(clearInterval);
    refreshIntervals = {};
  }

  // ─── Select Camera ───────────────────────────
  function selectCamera(id) {
    selectedId = id;
    renderCamaras();

    const cam = camaras.find(c => c.id === id);
    if (!cam) return;

    const empty = document.getElementById('detailEmpty');
    const content = document.getElementById('detailContent');

    empty.style.display = 'none';
    content.style.display = 'block';

    document.getElementById('detailTitle').textContent = `${cam.nombre} — ${cam.zona}`;
    document.getElementById('detailNombre').textContent = cam.nombre;
    document.getElementById('detailZona').textContent = `${cam.zona} (${cam.zonaDesc})`;
    document.getElementById('detailIp').innerHTML = `
      ${cam.ip}
      <span class="badge-status ${cam.online ? 'badge-online' : 'badge-offline'}">${cam.online ? 'En línea' : 'Desconectada'}</span>
    `;
    document.getElementById('detailRevision').textContent = cam.ultimaRevision;
    document.getElementById('detailEstado').innerHTML = `
      <span class="badge-status ${cam.activa ? 'badge-active' : 'badge-offline'}">${cam.activa ? 'Activa' : 'Inactiva'}</span>
    `;
  }

  // ─── Toggle Play ────────────────────────────
  function togglePlay(id) {
    const cam = camaras.find(c => c.id === id);
    if (!cam) return;
    cam.playing = !cam.playing;
    renderCamaras();
    if (selectedId === id) selectCamera(id);
  }

  function scrollToSelected() {
    const card = grid.querySelector('.camara-card.selected');
    if (card) card.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
  }

  // ─── Fullscreen Modal ────────────────────────
  function openFullscreen(id) {
    const cam = camaras.find(c => c.id === id);
    if (!cam || !cam.feedUrl) return;

    const overlay = document.getElementById('modalOverlay');
    const img = document.getElementById('modalImg');
    const title = document.getElementById('modalTitle');
    const label = document.getElementById('modalLabel');

    title.textContent = `${cam.nombre} — ${cam.zona}`;
    label.textContent = cam.feedLabel || 'Feed en vivo';
    img.src = `${cam.feedUrl}?t=${Date.now()}`;
    img.alt = `${cam.nombre} — ${cam.zona}`;

    overlay.style.display = 'flex';
    document.body.style.overflow = 'hidden';

    if (modalInterval) clearInterval(modalInterval);
    modalInterval = setInterval(() => {
      img.src = `${cam.feedUrl}?t=${Date.now()}`;
    }, 2000);
  }

  function closeFullscreen() {
    document.getElementById('modalOverlay').style.display = 'none';
    document.body.style.overflow = '';
    if (modalInterval) {
      clearInterval(modalInterval);
      modalInterval = null;
    }
  }

  // ─── Modal Event Listeners ───────────────────
  document.getElementById('modalClose').addEventListener('click', closeFullscreen);
  document.getElementById('modalOverlay').addEventListener('click', (e) => {
    if (e.target === e.currentTarget) closeFullscreen();
  });
  document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') closeFullscreen();
  });

  // ─── Initial render ──────────────────────────
  renderCamaras();

  // ─── Detail Action Buttons (mock) ────────────
  document.getElementById('btnCapturar').addEventListener('click', () => {
    PEIA.toast.info('Captura de pantalla simulada');
  });

  document.getElementById('btnReportar').addEventListener('click', () => {
    PEIA.toast.success('Incidencia reportada correctamente');
  });

  document.getElementById('btnGrabaciones').addEventListener('click', () => {
    PEIA.toast.info('Listado de grabaciones disponible proximamente');
  });

});
