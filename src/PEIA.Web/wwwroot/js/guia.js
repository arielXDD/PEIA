document.addEventListener('DOMContentLoaded', () => {
  if (!PEIA.requireAuth()) return;
  PEIA.hydrateShell();
  PEIA.bindWarehouseSelector();

  const popularArticles = [
    { href: '/Guia/exportacion', title: 'Como exportar un reporte en PDF', text: 'Personaliza columnas, filtros y rango de fechas antes de generar el archivo.', time: 'Lectura: 3 min' },
    { href: '/Guia/reportes', title: 'Entendiendo la tasa de rotacion', text: 'Compara entradas y salidas para detectar productos con alta demanda.', time: 'Lectura: 5 min' },
    { href: '/Guia/reglas', title: 'Configurar alertas de stock bajo', text: 'Define umbrales por categoria y asigna responsables de seguimiento.', time: 'Lectura: 4 min' },
    { href: '/Guia/intro', title: 'Gestion de permisos de usuarios', text: 'Diferencias entre roles Administrador, Operador, Reportes y Supervisor.', time: 'Lectura: 6 min' },
    { href: '/Guia/inventario', title: 'Registrar salida de mercancia', text: 'Evita stock negativo y documenta motivo, referencia y ubicacion.', time: 'Lectura: 4 min' }
  ];

  const searchInput = document.getElementById('guideSearch');

  function renderPopular(filter = '') {
    const list = document.getElementById('popularList');
    const term = filter.trim().toLowerCase();
    const data = popularArticles.filter(article => {
      if (!term) return true;
      return `${article.title} ${article.text}`.toLowerCase().includes(term);
    });

    list.innerHTML = data.length
      ? data.map(article => `
        <a class="popular-item" href="${article.href}">
          <div class="popular-icon">
            <svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><path d="M14 2v6h6"/></svg>
          </div>
          <div>
            <h3>${article.title}</h3>
            <p>${article.text}</p>
            <span>${article.time}</span>
          </div>
        </a>
      `).join('')
      : '<div class="guide-empty">No encontramos articulos para esa busqueda.</div>';
  }

  function searchGuide(term) {
    const normalized = term.trim().toLowerCase();
    if (!normalized) {
      document.querySelectorAll('.guide-category-card').forEach(card => card.style.display = '');
      renderPopular();
      return;
    }

    document.querySelectorAll('.guide-category-card').forEach(card => {
      card.style.display = card.textContent.toLowerCase().includes(normalized) ? '' : 'none';
    });

    renderPopular(normalized);
  }

  document.getElementById('guideSearchForm').addEventListener('submit', event => {
    event.preventDefault();
    searchGuide(searchInput.value);
  });

  searchInput.addEventListener('input', () => searchGuide(searchInput.value));

  document.querySelectorAll('.guide-task-grid button').forEach(button => {
    button.addEventListener('click', () => {
      const routes = {
        intro: '/Guia/intro',
        inventario: '/Guia/inventario',
        reportes: '/Guia/reportes',
        reglas: '/Guia/reglas',
        exportacion: '/Guia/exportacion'
      };
      window.location.href = routes[button.dataset.topic] || '/Guia';
    });
  });

  renderPopular();
});
