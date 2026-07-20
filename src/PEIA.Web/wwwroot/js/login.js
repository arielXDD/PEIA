// PEIA — Login JS
// Gestiona toggle de contraseña, loading state y submit

document.addEventListener('DOMContentLoaded', () => {
  const form       = document.getElementById('loginForm');
  const btnLogin   = document.getElementById('btnLogin');
  const btnSpinner = document.getElementById('btnSpinner');
  const btnText    = btnLogin.querySelector('.btn-text');
  const togglePwd  = document.getElementById('togglePassword');
  const pwdInput   = document.getElementById('password');
  const eyeIcon    = document.getElementById('eyeIcon');
  const errorBox   = document.getElementById('loginError');
  const errorText  = document.getElementById('loginErrorText');

  // ─── Toggle contraseña visible/oculto ──────────
  const eyeIconVisible = `
    <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/>
    <circle cx="12" cy="12" r="3"/>
  `;
  const eyeIconHidden = `
    <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94"/>
    <path d="M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19"/>
    <line x1="1" y1="1" x2="23" y2="23"/>
  `;

  let passwordVisible = false;

  togglePwd.addEventListener('click', () => {
    passwordVisible = !passwordVisible;
    pwdInput.type = passwordVisible ? 'text' : 'password';
    eyeIcon.innerHTML = passwordVisible ? eyeIconVisible : eyeIconHidden;
    togglePwd.setAttribute('aria-label', passwordVisible ? 'Ocultar contraseña' : 'Mostrar contraseña');
  });

  // ─── Manejo del submit ─────────────────────────
  form.addEventListener('submit', async (e) => {
    e.preventDefault();
    hideError();
    setLoading(true);

    const email    = document.getElementById('email').value.trim();
    const password = pwdInput.value;

    try {
      const response = await fetch('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ emailOrUsername: email, password }),
      });

      if (response.ok) {
        const data = await response.json();
        PEIA.setSession(data.token, data.user);
        window.location.href = '/Inicio';
      } else {
        const err = await response.json().catch(() => ({}));
        showError(err.message || 'Usuario o contraseña incorrectos.');
      }
    } catch (_) {
      showError('No se pudo conectar con el servidor. Intenta de nuevo.');
    } finally {
      setLoading(false);
    }
  });

  // ─── Helpers ───────────────────────────────────
  function setLoading(loading) {
    btnLogin.disabled = loading;
    btnText.style.display   = loading ? 'none' : 'inline';
    btnSpinner.style.display = loading ? 'inline-flex' : 'none';
  }

  function showError(msg) {
    errorText.textContent = msg;
    errorBox.style.display = 'flex';
  }

  function hideError() {
    errorBox.style.display = 'none';
  }
});
