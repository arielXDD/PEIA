(() => {
  window.PEIA = window.PEIA || {};

  const ensureContainer = () => {
    let container = document.getElementById('peiaToastContainer');
    if (!container) {
      container = document.createElement('div');
      container.id = 'peiaToastContainer';
      container.className = 'peia-toast-container';
      container.setAttribute('aria-live', 'polite');
      document.body.appendChild(container);
    }
    return container;
  };

  const show = (type, message, options = {}) => {
    const toast = document.createElement('div');
    toast.className = `peia-toast peia-toast--${type}`;
    toast.setAttribute('role', type === 'error' ? 'alert' : 'status');

    const text = document.createElement('span');
    text.className = 'peia-toast__message';
    text.textContent = String(message || 'Operación completada.');

    const close = document.createElement('button');
    close.type = 'button';
    close.className = 'peia-toast__close';
    close.setAttribute('aria-label', 'Cerrar');
    close.textContent = '×';

    const dismiss = () => {
      toast.classList.add('peia-toast--leaving');
      window.setTimeout(() => toast.remove(), 180);
    };

    close.addEventListener('click', dismiss);
    toast.append(text, close);
    ensureContainer().appendChild(toast);
    window.setTimeout(dismiss, options.duration ?? (type === 'error' ? 6000 : 4000));
    return toast;
  };

  const confirm = (message, options = {}) => new Promise(resolve => {
    const overlay = document.createElement('div');
    overlay.className = 'peia-confirm-overlay';
    overlay.innerHTML = `
      <div class="peia-confirm" role="dialog" aria-modal="true" aria-labelledby="peiaConfirmTitle">
        <h2 id="peiaConfirmTitle">${options.title || 'Confirmar acción'}</h2>
        <p></p>
        <div class="peia-confirm__actions">
          <button type="button" class="peia-confirm__cancel">${options.cancelText || 'Cancelar'}</button>
          <button type="button" class="peia-confirm__accept">${options.confirmText || 'Confirmar'}</button>
        </div>
      </div>`;
    overlay.querySelector('p').textContent = String(message);

    const finish = value => {
      document.removeEventListener('keydown', onKeydown);
      overlay.remove();
      resolve(value);
    };
    const onKeydown = event => {
      if (event.key === 'Escape') finish(false);
    };

    overlay.querySelector('.peia-confirm__cancel').addEventListener('click', () => finish(false));
    overlay.querySelector('.peia-confirm__accept').addEventListener('click', () => finish(true));
    overlay.addEventListener('click', event => {
      if (event.target === overlay) finish(false);
    });
    document.addEventListener('keydown', onKeydown);
    document.body.appendChild(overlay);
    overlay.querySelector('.peia-confirm__accept').focus();
  });

  PEIA.toast = {
    success: (message, options) => show('success', message, options),
    error: (message, options) => show('error', message, options),
    info: (message, options) => show('info', message, options),
    confirm
  };
})();
