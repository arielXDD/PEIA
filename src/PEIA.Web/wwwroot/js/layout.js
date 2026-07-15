document.addEventListener('DOMContentLoaded', () => {
  if (!window.PEIA?.requireAuth()) return;

  PEIA.hydrateShell();
  PEIA.bindWarehouseSelector();

  const sidebar = document.getElementById('sidebar');
  const toggle = document.getElementById('menuToggle');
  const close = document.getElementById('sidebarClose');
  const backdrop = document.getElementById('sidebarBackdrop');

  const setSidebar = open => {
    sidebar?.classList.toggle('open', open);
    document.body.classList.toggle('sidebar-open', open);
    toggle?.setAttribute('aria-expanded', String(open));
  };

  toggle?.addEventListener('click', () => setSidebar(!sidebar?.classList.contains('open')));
  close?.addEventListener('click', () => setSidebar(false));
  backdrop?.addEventListener('click', () => setSidebar(false));
  sidebar?.querySelectorAll('a').forEach(link => link.addEventListener('click', () => setSidebar(false)));
  window.addEventListener('resize', () => {
    if (window.innerWidth > 768) setSidebar(false);
  });
});
