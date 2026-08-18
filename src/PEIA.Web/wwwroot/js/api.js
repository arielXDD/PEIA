window.PEIA = (() => {
  const tokenKey = 'peia_token';
  const userKey = 'peia_user';
  const activeCentroKey = 'peia_centro_activo';

  function getToken() {
    return localStorage.getItem(tokenKey);
  }

  function getUser() {
    return JSON.parse(localStorage.getItem(userKey) || '{}');
  }

  function setSession(token, user) {
    localStorage.setItem(tokenKey, token);
    localStorage.setItem(userKey, JSON.stringify(user));
    if (user?.centros?.length) {
      const storedCentro = JSON.parse(localStorage.getItem(activeCentroKey) || 'null');
      const hasValidCentro = storedCentro?.id && user.centros.some(c => c.id === storedCentro.id);
      if (!hasValidCentro) {
        localStorage.setItem(activeCentroKey, JSON.stringify(user.centros[0]));
      }
    }
  }

  function clearSession() {
    localStorage.removeItem(tokenKey);
    localStorage.removeItem(userKey);
    localStorage.removeItem(activeCentroKey);
  }

  function requireAuth() {
    if (!getToken()) {
      window.location.href = '/Login';
      return false;
    }
    return true;
  }

  async function request(path, options = {}) {
    const headers = { ...(options.headers || {}) };
    if (!(options.body instanceof FormData)) {
      headers['Content-Type'] = headers['Content-Type'] || 'application/json';
    }

    const token = getToken();
    if (token) {
      headers.Authorization = `Bearer ${token}`;
    }

    const response = await fetch(path, { ...options, headers });
    if (response.status === 401) {
      clearSession();
      window.location.href = '/Login';
      throw new Error('Sesión expirada.');
    }

    const contentType = response.headers.get('content-type') || '';
    const payload = contentType.includes('application/json')
      ? await response.json().catch(() => null)
      : await response.text().catch(() => null);

    if (!response.ok) {
      const message = payload?.message || payload?.title || 'No se pudo completar la operación.';
      throw new Error(message);
    }

    return payload;
  }

  function logout() {
    clearSession();
    window.location.href = '/Login';
  }

  let hubConnection = null;

  async function connectHub() {
    if (typeof signalR === 'undefined') return null;
    if (hubConnection) return hubConnection;

    hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/peia', { accessTokenFactory: () => getToken() })
      .withAutomaticReconnect()
      .build();

    const joinActiveCentro = async () => {
      const centro = getActiveCentro();
      if (centro?.id) {
        await hubConnection.invoke('JoinCentro', centro.id).catch(() => null);
      }
    };

    hubConnection.onreconnected(joinActiveCentro);
    window.addEventListener('peia:centro-changed', joinActiveCentro);

    await hubConnection.start();
    await joinActiveCentro();

    return hubConnection;
  }

  async function updateNotificationBadge() {
    const centro = getActiveCentro();
    if (!centro?.id || !getToken()) return;
    try {
      const data = await request(`/api/notificaciones?centroId=${encodeURIComponent(centro.id)}`);
      if (Array.isArray(data)) {
        const unread = data.filter(item => !item.leida).length;
        const badges = document.querySelectorAll('a[href="/Notificaciones"] .badge, a[href*="Notificaciones"] .badge, #navNotifBadge');
        badges.forEach(badge => {
          badge.textContent = unread;
          badge.style.display = unread > 0 ? '' : 'none';
        });
      }
    } catch {
      // Silenciar errores de red en segundo plano
    }
  }

  function hydrateShell() {
    const user = getUser();
    if (user?.nombreCompleto) {
      const name = document.getElementById('userName');
      const avatar = document.getElementById('userAvatar');
      if (name) name.textContent = user.nombreCompleto;
      if (avatar) avatar.textContent = user.nombreCompleto.split(' ').map(n => n[0]).join('').slice(0, 2).toUpperCase();
    }

    const active = getActiveCentro();
    const activeEl = document.getElementById('activeCentro');
    if (activeEl && active?.nombre) activeEl.textContent = active.nombre;

    document.getElementById('btnLogout')?.addEventListener('click', logout);
    updateNotificationBadge();
  }

  function getActiveCentro() {
    const stored = localStorage.getItem(activeCentroKey);
    const user = getUser();
    if (stored) {
      const centro = JSON.parse(stored);
      const isValid = centro?.id && (user?.centros || []).some(c => c.id === centro.id);
      if (isValid) return centro;
    }

    const fallback = user?.centros?.[0] || null;
    if (fallback) localStorage.setItem(activeCentroKey, JSON.stringify(fallback));
    return fallback;
  }

  async function setActiveCentro(centro) {
    if (!centro) return;
    await request('/api/usuarios/centro-activo', {
      method: 'PUT',
      body: JSON.stringify({ centroId: centro.id })
    }).catch(() => null);
    localStorage.setItem(activeCentroKey, JSON.stringify(centro));
    const activeEl = document.getElementById('activeCentro');
    if (activeEl) activeEl.textContent = centro.nombre;
  }

  function bindWarehouseSelector() {
    const selector = document.querySelector('.warehouse-selector');
    const dropdown = document.getElementById('warehouseDropdown');
    if (!selector || !dropdown) return;
    if (selector.dataset.peiaBound === 'true') return;
    selector.dataset.peiaBound = 'true';

    const user = getUser();
    const active = getActiveCentro();
    if (Array.isArray(user.centros) && user.centros.length) {
      dropdown.innerHTML = user.centros.map((c, i) =>
        `<button class="warehouse-opt ${active?.id === c.id || (!active?.id && i === 0) ? 'active' : ''}" data-centro-id="${c.id}">${c.nombre}</button>`
      ).join('');
    }

    selector.addEventListener('click', e => {
      e.stopPropagation();
      selector.classList.toggle('open');
      dropdown.classList.toggle('open');
    });

    dropdown.querySelectorAll('.warehouse-opt').forEach(btn => {
      btn.addEventListener('click', async e => {
        e.stopPropagation();
        dropdown.querySelectorAll('.warehouse-opt').forEach(b => b.classList.remove('active'));
        btn.classList.add('active');
        const centro = (getUser().centros || []).find(c => c.id === btn.dataset.centroId) || { id: btn.dataset.centroId, nombre: btn.textContent };
        selector.classList.remove('open');
        dropdown.classList.remove('open');
        await setActiveCentro(centro);
        window.dispatchEvent(new CustomEvent('peia:centro-changed', { detail: centro }));
      });
    });

    document.addEventListener('click', () => {
      selector.classList.remove('open');
      dropdown.classList.remove('open');
    });
  }

  window.addEventListener('peia:centro-changed', () => updateNotificationBadge());

  return { request, setSession, clearSession, requireAuth, logout, hydrateShell, bindWarehouseSelector, getUser, getActiveCentro, connectHub, updateNotificationBadge };
})();
