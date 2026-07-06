// PEIA — Predicción JS
// Mock data + Chart.js + Interactividad

document.addEventListener('DOMContentLoaded', () => {

  // ─── Mock Data ──────────────────────────────
  const historicoData = {
    labels: ['07 Jun','08 Jun','09 Jun','10 Jun','11 Jun','12 Jun','13 Jun',
             '14 Jun','15 Jun','16 Jun','17 Jun','18 Jun','19 Jun','20 Jun'],
    values: [520, 480, 610, 590, 630, 580, 650, 620, 680, 590, 700, 640, 720, 660],
  };

  const prediccion14 = {
    labels: ['21 Jun','22 Jun','23 Jun','24 Jun','25 Jun','26 Jun','27 Jun',
             '28 Jun','29 Jun','30 Jun','01 Jul','02 Jul','03 Jul','04 Jul'],
    values: [690, 720, 680, 750, 710, 780, 740, 810, 770, 830, 790, 860, 820, 890],
    confianza: [30, 32, 35, 40, 45, 50, 55, 60, 65, 68, 72, 75, 78, 80],
  };

  const prediccion7 = {
    labels: ['21 Jun','22 Jun','23 Jun','24 Jun','25 Jun','26 Jun','27 Jun'],
    values: [700, 730, 690, 760, 720, 790, 750],
    confianza: [15, 18, 20, 22, 25, 28, 30],
  };

  const prediccion30 = {
    labels: Array.from({length: 30}, (_, i) => {
      const d = new Date(2026, 5, 21 + i);
      return `${d.getDate()} ${d.toLocaleString('es', {month:'short'})}`;
    }),
    values: Array.from({length: 30}, () => Math.floor(Math.random() * 300) + 600),
    confianza: Array.from({length: 30}, (_, i) => Math.min(30 + i * 2, 120)),
  };

  const categoriasData = {
    labels: ['Tarima','Caja 60L','Pallet','Cinta','Film','Guantes'],
    values: [1250, 980, 760, 520, 640, 380],
  };

  const reabastecerData = {
    labels: ['Cinta Adhesiva','Guantes Nitrilo','Film Stretch','Caja 60L','Pallet'],
    values: [280, 220, 180, 150, 120],
  };

  const tendenciaData = {
    labels: ['Lun','Mar','Mié','Jue','Vie','Sáb','Dom'],
    values: [12, 8, 15, 10, 18, 5, 3],
    colors: ['#3b82f6','#22c55e','#f59e0b','#3b82f6','#ef4444','#22c55e','#8b5cf6'],
  };

  const productosPred = [
    { nombre: 'Tarima de Madera',     cat: 'Madera',   stock: 1250, estimado: 1120, recomendado: 0,   confianza: 96 },
    { nombre: 'Caja Plástica 60L',   cat: 'Plástico', stock: 980,  estimado: 720,  recomendado: 0,   confianza: 93 },
    { nombre: 'Pallet Plástico',     cat: 'Plástico', stock: 760,  estimado: 680,  recomendado: 0,   confianza: 90 },
    { nombre: 'Cinta Adhesiva 48mm', cat: 'Adhesivo', stock: 25,   estimado: 280,  recomendado: 255, confianza: 88 },
    { nombre: 'Film Stretch',        cat: 'Plástico', stock: 180,  estimado: 340,  recomendado: 160, confianza: 85 },
    { nombre: 'Guantes de Nitrilo',  cat: 'Seguridad',stock: 0,    estimado: 220,  recomendado: 220, confianza: 82 },
  ];

  // ─── Chart Config ───────────────────────────
  Chart.defaults.font.family = "'Inter', sans-serif";
  Chart.defaults.font.size   = 11;
  Chart.defaults.color       = '#9ca3af';

  let chartPrincipal = null;
  let chartCategorias = null;
  let chartReabastecer = null;
  let chartTendencia = null;

  function renderPrincipal(periodo) {
    const data = periodo === '7' ? prediccion7 : periodo === '30' ? prediccion30 : prediccion14;
    const ctx = document.getElementById('chartPrincipal').getContext('2d');
    if (chartPrincipal) chartPrincipal.destroy();

    const allLabels = [...historicoData.labels, ...data.labels];
    const historicValues = [...historicoData.values, ...Array(data.labels.length).fill(null)];
    const predValues = [...Array(historicoData.labels.length).fill(null), ...data.values];

    const upperCI = predValues.map((v, i) => v !== null ? v + (data.confianza[i - historicoData.labels.length] || 0) : null);
    const lowerCI = predValues.map((v, i) => v !== null ? Math.max(0, v - (data.confianza[i - historicoData.labels.length] || 0)) : null);

    chartPrincipal = new Chart(ctx, {
      type: 'line',
      data: {
        labels: allLabels,
        datasets: [
          {
            label: 'Intervalo confianza',
            data: upperCI,
            borderColor: 'transparent',
            backgroundColor: 'rgba(124,58,237,.08)',
            fill: '+1',
            pointRadius: 0,
            tension: 0.35,
          },
          {
            label: 'Intervalo confianza',
            data: lowerCI,
            borderColor: 'transparent',
            backgroundColor: 'transparent',
            fill: false,
            pointRadius: 0,
          },
          {
            label: 'Histórico',
            data: historicValues,
            borderColor: '#3b82f6',
            backgroundColor: 'rgba(59,130,246,.08)',
            tension: 0.35,
            fill: true,
            pointRadius: 4,
            pointBackgroundColor: '#3b82f6',
            borderWidth: 2,
          },
          {
            label: 'Predicción',
            data: predValues,
            borderColor: '#7c3aed',
            backgroundColor: 'rgba(124,58,237,.06)',
            tension: 0.35,
            fill: true,
            pointRadius: 4,
            pointBackgroundColor: '#7c3aed',
            borderWidth: 2,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: {
          x: { grid: { display: false }, ticks: { maxTicksLimit: 10 } },
          y: { grid: { color: '#f3f4f6' }, border: { display: false }, ticks: { maxTicksLimit: 5 } },
        },
      },
    });
  }

  function renderCategorias() {
    if (chartCategorias) chartCategorias.destroy();
    chartCategorias = new Chart(document.getElementById('chartCategorias'), {
      type: 'bar',
      data: {
        labels: categoriasData.labels,
        datasets: [{
          data: categoriasData.values,
          backgroundColor: ['#3b82f6','#22c55e','#f59e0b','#ef4444','#8b5cf6','#0891b2'],
          borderRadius: 4,
          barThickness: 18,
        }],
      },
      options: {
        indexAxis: 'y',
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: {
          x: { grid: { color: '#f3f4f6' }, border: { display: false }, ticks: { maxTicksLimit: 4 } },
          y: { grid: { display: false }, ticks: { font: { size: 10 } } },
        },
      },
    });
  }

  function renderReabastecer() {
    if (chartReabastecer) chartReabastecer.destroy();
    chartReabastecer = new Chart(document.getElementById('chartReabastecer'), {
      type: 'bar',
      data: {
        labels: reabastecerData.labels,
        datasets: [{
          data: reabastecerData.values,
          backgroundColor: '#ef4444',
          borderRadius: 4,
          barThickness: 14,
        }],
      },
      options: {
        indexAxis: 'y',
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: {
          x: { grid: { color: '#f3f4f6' }, border: { display: false }, ticks: { maxTicksLimit: 4 } },
          y: { grid: { display: false }, ticks: { font: { size: 10 } } },
        },
      },
    });
  }

  function renderTendencia() {
    if (chartTendencia) chartTendencia.destroy();
    const total = tendenciaData.values.reduce((a, b) => a + b, 0);
    chartTendencia = new Chart(document.getElementById('chartTendencia'), {
      type: 'doughnut',
      data: {
        labels: tendenciaData.labels,
        datasets: [{
          data: tendenciaData.values,
          backgroundColor: tendenciaData.colors,
          borderWidth: 0,
          hoverOffset: 6,
        }],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        cutout: '65%',
        plugins: { legend: { display: false } },
      },
    });

    const legend = document.getElementById('donutLegendPred');
    legend.innerHTML = '';
    tendenciaData.labels.forEach((lbl, i) => {
      const pct = Math.round(tendenciaData.values[i] / total * 100);
      legend.innerHTML += `
        <div class="donut-legend-item">
          <span><i class="dot" style="background:${tendenciaData.colors[i]};border-radius:50%;width:9px;height:9px;display:inline-block;margin-right:6px;"></i>${lbl}</span>
          <span style="font-weight:600;color:#374151">${tendenciaData.values[i]}% <span style="color:#9ca3af">var</span></span>
        </div>`;
    });
  }

  function renderTabla() {
    const tbody = document.getElementById('tbodyPrediccion');
    tbody.innerHTML = '';
    productosPred.forEach(p => {
      const urgente = p.recomendado > 0;
      tbody.innerHTML += `
        <tr>
          <td style="font-weight:500;color:#111827">${p.nombre}</td>
          <td>${p.cat}</td>
          <td style="font-weight:600">${p.stock}</td>
          <td>${p.estimado}</td>
          <td>
            ${p.recomendado > 0
              ? `<span style="color:#dc2626;font-weight:600">+${p.recomendado}</span>`
              : `<span style="color:#16a34a">Suficiente</span>`}
          </td>
          <td>
            <div style="display:flex;align-items:center;gap:6px">
              <div style="flex:1;max-width:60px;height:6px;background:#e5e7eb;border-radius:3px;overflow:hidden">
                <div style="width:${p.confianza}%;height:100%;background:${p.confianza >= 90 ? '#16a34a' : p.confianza >= 80 ? '#f59e0b' : '#ef4444'};border-radius:3px"></div>
              </div>
              ${p.confianza}%
            </div>
          </td>
        </tr>`;
    });
  }

  // ─── Init ───────────────────────────────────
  renderPrincipal('14');
  renderCategorias();
  renderReabastecer();
  renderTendencia();
  renderTabla();

  // Period selector
  document.getElementById('periodoSelect').addEventListener('change', (e) => {
    renderPrincipal(e.target.value);
  });

  // ─── Warehouse Selector ──────────────────────
  const warehouseSelector = document.querySelector('.warehouse-selector');
  warehouseSelector.addEventListener('click', (e) => {
    e.stopPropagation();
    warehouseSelector.classList.toggle('open');
  });

  document.querySelectorAll('.warehouse-opt').forEach(btn => {
    btn.addEventListener('click', (e) => {
      e.stopPropagation();
      document.querySelectorAll('.warehouse-opt').forEach(b => b.classList.remove('active'));
      btn.classList.add('active');
      document.getElementById('activeCentro').textContent = btn.textContent;
      warehouseSelector.classList.remove('open');
    });
  });

  document.addEventListener('click', () => warehouseSelector.classList.remove('open'));

  // ─── Logout ──────────────────────────────────
  document.getElementById('btnLogout').addEventListener('click', () => {
    localStorage.removeItem('peia_token');
    localStorage.removeItem('peia_user');
    window.location.href = '/login.html';
  });

  // ─── User Info ──────────────────────────────
  const user = JSON.parse(localStorage.getItem('peia_user') || '{}');
  if (user.nombreCompleto) {
    document.getElementById('userName').textContent = user.nombreCompleto;
    const initials = user.nombreCompleto.split(' ').map(n => n[0]).join('').slice(0, 2).toUpperCase();
    document.getElementById('userAvatar').textContent = initials;
  }

});
