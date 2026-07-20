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
  const eyeIconVisible = `<path d="M2 12s3.5-6 10-6 10 6 10 6-3.5 6-10 6S2 12 2 12Z"/><circle cx="12" cy="12" r="2.5"/>`;
  const eyeIconHidden = `
    <path d="M3 3l18 18M10.6 6.1c.5-.1.9-.1 1.4-.1 6.5 0 10 6 10 6a16 16 0 0 1-3 3.6M6.2 6.2C3.4 8.1 2 12 2 12s3.5 6 10 6c1.5 0 2.8-.3 3.9-.7"/>
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
    btnText.hidden = loading;
    btnSpinner.hidden = !loading;
    document.querySelector('.button-arrow').hidden = loading;
  }

  function showError(msg) {
    errorText.textContent = msg;
    errorBox.hidden = false;
  }

  function hideError() {
    errorBox.hidden = true;
  }
});
