// PEIA — Modal Form Reusable Component
// Dynamic modal with form fields

class ModalForm {
  /**
   * @param {Object} config
   * @param {string} config.title - Modal title
   * @param {Array<Object>} config.fields - Form fields config
   *   Each field: { key, label, type, placeholder, required, options, hint, value, half }
   *   type: 'text'|'email'|'password'|'select'|'textarea'|'checkbox'|'number'
   *   options: [{value, label}] for select
   *   half: true to use half-width in a row
   * @param {Function} config.onSave - Callback(formData)
   * @param {Function} [config.onClose] - Callback on close
   * @param {Object} [config.initialData] - Pre-fill form with this data
   */
  constructor(config) {
    this.config = config;
    this.overlay = null;
    this.formData = { ...(config.initialData || {}) };
    this.open();
  }

  open() {
    // Create overlay
    this.overlay = document.createElement('div');
    this.overlay.className = 'modal-overlay';
    this.overlay.addEventListener('click', (e) => {
      if (e.target === this.overlay) this.close();
    });

    // Container
    const container = document.createElement('div');
    container.className = 'modal-container';

    // Header
    container.innerHTML = `
      <div class="modal-header">
        <h2>${this.config.title}</h2>
        <button class="modal-close" aria-label="Cerrar">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round">
            <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
          </svg>
        </button>
      </div>
    `;

    // Body with form fields
    const body = document.createElement('div');
    body.className = 'modal-body';
    this._renderFields(body);
    container.appendChild(body);

    // Footer
    const footer = document.createElement('div');
    footer.className = 'modal-footer';
    footer.innerHTML = `
      <button class="btn btn-secondary modal-cancel-btn">Cancelar</button>
      <button class="btn btn-primary modal-save-btn">
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polyline points="20 6 9 17 4 12"/></svg>
        Guardar
      </button>
    `;
    container.appendChild(footer);

    this.overlay.appendChild(container);
    document.body.appendChild(this.overlay);

    // Prevent body scroll
    document.body.style.overflow = 'hidden';

    // Bind events
    container.querySelector('.modal-close').addEventListener('click', () => this.close());
    container.querySelector('.modal-cancel-btn').addEventListener('click', () => this.close());
    container.querySelector('.modal-save-btn').addEventListener('click', () => this._handleSave());

    // Escape key
    this._escHandler = (e) => { if (e.key === 'Escape') this.close(); };
    document.addEventListener('keydown', this._escHandler);

    // Focus first input
    setTimeout(() => {
      const firstInput = body.querySelector('input, select, textarea');
      if (firstInput) firstInput.focus();
    }, 100);
  }

  _renderFields(body) {
    const fields = this.config.fields || [];
    let i = 0;

    while (i < fields.length) {
      const field = fields[i];

      // Check if this field and the next should be in a row
      if (field.half && i + 1 < fields.length && fields[i + 1].half) {
        const row = document.createElement('div');
        row.className = 'form-row';
        row.appendChild(this._createField(fields[i]));
        row.appendChild(this._createField(fields[i + 1]));
        body.appendChild(row);
        i += 2;
      } else {
        body.appendChild(this._createField(field));
        i++;
      }
    }
  }

  _createField(field) {
    const group = document.createElement('div');
    group.className = 'form-field';

    const val = this.formData[field.key] ?? field.value ?? '';

    if (field.type === 'checkbox') {
      group.innerHTML = `
        <label class="form-checkbox">
          <input type="checkbox" data-key="${field.key}" ${val ? 'checked' : ''} />
          ${field.label}
        </label>
      `;
    } else if (field.type === 'select') {
      const options = (field.options || []).map(opt =>
        `<option value="${opt.value}" ${opt.value === val ? 'selected' : ''}>${opt.label}</option>`
      ).join('');
      group.innerHTML = `
        <label>${field.label}${field.required ? '<span class="required">*</span>' : ''}</label>
        <select data-key="${field.key}">
          <option value="">— Seleccionar —</option>
          ${options}
        </select>
      `;
    } else if (field.type === 'textarea') {
      group.innerHTML = `
        <label>${field.label}${field.required ? '<span class="required">*</span>' : ''}</label>
        <textarea data-key="${field.key}" placeholder="${field.placeholder || ''}" rows="3">${val}</textarea>
      `;
    } else {
      group.innerHTML = `
        <label>${field.label}${field.required ? '<span class="required">*</span>' : ''}</label>
        <input type="${field.type || 'text'}" data-key="${field.key}" value="${val}" placeholder="${field.placeholder || ''}" 
          ${field.required ? 'required' : ''} ${field.min != null ? `min="${field.min}"` : ''} ${field.max != null ? `max="${field.max}"` : ''} />
      `;
    }

    if (field.hint) {
      const hint = document.createElement('span');
      hint.className = 'field-hint';
      hint.textContent = field.hint;
      group.appendChild(hint);
    }

    return group;
  }

  _collectData() {
    const data = {};
    this.overlay.querySelectorAll('[data-key]').forEach(el => {
      const key = el.dataset.key;
      if (el.type === 'checkbox') {
        data[key] = el.checked;
      } else if (el.type === 'number') {
        data[key] = el.value ? Number(el.value) : null;
      } else {
        data[key] = el.value;
      }
    });
    return data;
  }

  _handleSave() {
    // Basic required validation
    let valid = true;
    this.overlay.querySelectorAll('[data-key]').forEach(el => {
      if (el.hasAttribute('required') && !el.value.trim()) {
        el.style.borderColor = 'var(--red)';
        el.style.boxShadow = '0 0 0 3px rgba(239,68,68,.12)';
        valid = false;
        el.addEventListener('input', () => {
          el.style.borderColor = '';
          el.style.boxShadow = '';
        }, { once: true });
      }
    });

    if (!valid) return;

    const data = this._collectData();
    if (this.config.onSave) {
      this.config.onSave(data);
    }
    this.close();
  }

  close() {
    if (this.overlay) {
      this.overlay.style.animation = 'modalOverlayIn .15s ease reverse';
      setTimeout(() => {
        this.overlay.remove();
        document.body.style.overflow = '';
      }, 140);
    }
    document.removeEventListener('keydown', this._escHandler);
    if (this.config.onClose) this.config.onClose();
  }
}
