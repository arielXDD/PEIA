// PEIA — DataTable Reusable Component
// Renders a table with pagination, search, and sorting

class DataTable {
  /**
   * @param {HTMLElement} container - DOM element to render into
   * @param {Object} config
   * @param {Array<{key:string, label:string, render?:Function, sortable?:boolean}>} config.columns
   * @param {Array<Object>} config.data
   * @param {number} [config.pageSize=8]
   * @param {string} [config.searchPlaceholder='Buscar...']
   * @param {boolean} [config.searchable=true]
   */
  constructor(container, config) {
    this.container = container;
    this.columns = config.columns || [];
    this.allData = config.data || [];
    this.filteredData = [...this.allData];
    this.pageSize = config.pageSize || 8;
    this.currentPage = 1;
    this.searchPlaceholder = config.searchPlaceholder || 'Buscar...';
    this.searchable = config.searchable !== false;
    this.searchTerm = '';
    this.sortKey = null;
    this.sortAsc = true;

    this.render();
  }

  setData(data) {
    this.allData = data;
    this.applyFilters();
  }

  applyFilters() {
    let data = [...this.allData];

    // Search
    if (this.searchTerm) {
      const term = this.searchTerm.toLowerCase();
      data = data.filter(row =>
        this.columns.some(col => {
          const val = row[col.key];
          if (val == null) return false;
          return String(val).toLowerCase().includes(term);
        })
      );
    }

    // Sort
    if (this.sortKey) {
      data.sort((a, b) => {
        const va = a[this.sortKey] ?? '';
        const vb = b[this.sortKey] ?? '';
        if (typeof va === 'number' && typeof vb === 'number') {
          return this.sortAsc ? va - vb : vb - va;
        }
        const cmp = String(va).localeCompare(String(vb), 'es', { numeric: true });
        return this.sortAsc ? cmp : -cmp;
      });
    }

    this.filteredData = data;
    this.currentPage = 1;
    this.renderTable();
    this.renderPagination();
  }

  get totalPages() {
    return Math.max(1, Math.ceil(this.filteredData.length / this.pageSize));
  }

  get pageData() {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.filteredData.slice(start, start + this.pageSize);
  }

  render() {
    this.container.innerHTML = '';
    this.container.classList.add('dt-wrapper');

    // Table scroll
    const scroll = document.createElement('div');
    scroll.className = 'dt-scroll';
    this.tableEl = document.createElement('table');
    this.tableEl.className = 'dt-table';
    scroll.appendChild(this.tableEl);
    this.container.appendChild(scroll);

    // Footer
    this.footerEl = document.createElement('div');
    this.footerEl.className = 'dt-footer';
    this.container.appendChild(this.footerEl);

    this.renderTable();
    this.renderPagination();
  }

  renderTable() {
    let html = '<thead><tr>';
    this.columns.forEach(col => {
      const sortIcon = this.sortKey === col.key
        ? (this.sortAsc ? ' ▲' : ' ▼')
        : '';
      const sortable = col.sortable !== false ? 'style="cursor:pointer"' : '';
      html += `<th ${sortable} data-key="${col.key}">${col.label}${sortIcon}</th>`;
    });
    html += '</tr></thead><tbody>';

    if (this.pageData.length === 0) {
      html += `<tr><td colspan="${this.columns.length}" style="text-align:center;padding:32px;color:var(--text-muted);">No se encontraron resultados</td></tr>`;
    } else {
      this.pageData.forEach((row, idx) => {
        html += '<tr>';
        this.columns.forEach(col => {
          const val = col.render ? col.render(row[col.key], row, idx) : (row[col.key] ?? '');
          html += `<td>${val}</td>`;
        });
        html += '</tr>';
      });
    }

    html += '</tbody>';
    this.tableEl.innerHTML = html;

    // Bind sort
    this.tableEl.querySelectorAll('th[data-key]').forEach(th => {
      th.addEventListener('click', () => {
        const key = th.dataset.key;
        const col = this.columns.find(c => c.key === key);
        if (col && col.sortable === false) return;
        if (this.sortKey === key) {
          this.sortAsc = !this.sortAsc;
        } else {
          this.sortKey = key;
          this.sortAsc = true;
        }
        this.applyFilters();
      });
    });
  }

  renderPagination() {
    const total = this.filteredData.length;
    const start = total === 0 ? 0 : (this.currentPage - 1) * this.pageSize + 1;
    const end = Math.min(this.currentPage * this.pageSize, total);

    let html = `<span class="dt-info">Mostrando ${start}–${end} de ${total}</span>`;
    html += '<div class="dt-pagination">';

    // Prev
    html += `<button class="dt-prev" ${this.currentPage <= 1 ? 'disabled' : ''}>
      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M15 18l-6-6 6-6"/></svg>
    </button>`;

    // Pages
    const pages = this._getPageNumbers();
    pages.forEach(p => {
      if (p === '...') {
        html += `<button disabled>…</button>`;
      } else {
        html += `<button class="dt-page ${p === this.currentPage ? 'active' : ''}" data-page="${p}">${p}</button>`;
      }
    });

    // Next
    html += `<button class="dt-next" ${this.currentPage >= this.totalPages ? 'disabled' : ''}>
      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M9 18l6-6-6-6"/></svg>
    </button>`;
    html += '</div>';

    this.footerEl.innerHTML = html;

    // Bind pagination
    this.footerEl.querySelector('.dt-prev')?.addEventListener('click', () => this.goToPage(this.currentPage - 1));
    this.footerEl.querySelector('.dt-next')?.addEventListener('click', () => this.goToPage(this.currentPage + 1));
    this.footerEl.querySelectorAll('.dt-page').forEach(btn => {
      btn.addEventListener('click', () => this.goToPage(parseInt(btn.dataset.page)));
    });
  }

  goToPage(page) {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.renderTable();
    this.renderPagination();
  }

  search(term) {
    this.searchTerm = term;
    this.applyFilters();
  }

  _getPageNumbers() {
    const total = this.totalPages;
    const current = this.currentPage;
    if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1);

    const pages = [];
    pages.push(1);
    if (current > 3) pages.push('...');
    for (let i = Math.max(2, current - 1); i <= Math.min(total - 1, current + 1); i++) {
      pages.push(i);
    }
    if (current < total - 2) pages.push('...');
    pages.push(total);
    return pages;
  }
}
