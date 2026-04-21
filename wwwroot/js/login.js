document.addEventListener('DOMContentLoaded', function () {

    var form       = document.getElementById('loginForm');
    var emailInput = document.getElementById('email');
    var senhaInput = document.getElementById('senha');
    var btnLogin   = document.getElementById('btnLogin');
    var emailError = document.getElementById('emailError');
    var senhaError = document.getElementById('senhaError');
    var toggleBtn  = document.getElementById('toggleSenha');

    // --- mostrar/ocultar senha ---
    if (toggleBtn && senhaInput) {
        toggleBtn.addEventListener('click', function () {
            var isPass = senhaInput.type === 'password';
            senhaInput.type = isPass ? 'text' : 'password';
            toggleBtn.textContent = isPass ? '🙈' : '👁';
        });
    }

    // --- validação em tempo real: e-mail ---
    if (emailInput) {
        emailInput.addEventListener('input', function () {
            var ok = emailInput.value.includes('@') && emailInput.value.includes('.');
            if (emailInput.value.length === 0) {
                resetInput(emailInput, emailError);
            } else if (ok) {
                setSuccess(emailInput, emailError, '');
            } else {
                setError(emailInput, emailError, 'E-mail inválido');
            }
        });
    }

    // --- validação em tempo real: senha ---
    if (senhaInput) {
        senhaInput.addEventListener('input', function () {
            if (senhaInput.value.length === 0) {
                resetInput(senhaInput, senhaError);
            } else if (senhaInput.value.length >= 6) {
                setSuccess(senhaInput, senhaError, '');
            } else {
                setError(senhaInput, senhaError, 'Mínimo 6 caracteres');
            }
        });
    }

    // --- submit ---
    if (form) {
        form.addEventListener('submit', function (e) {
            var valid = true;

            resetInput(emailInput, emailError);
            resetInput(senhaInput, senhaError);

            if (!emailInput.value.includes('@') || !emailInput.value.includes('.')) {
                setError(emailInput, emailError, 'Informe um e-mail válido');
                valid = false;
            }

            if (senhaInput.value.length < 6) {
                setError(senhaInput, senhaError, 'Mínimo 6 caracteres');
                valid = false;
            }

            if (!valid) { e.preventDefault(); return; }

            btnLogin.textContent = 'Entrando...';
            btnLogin.disabled = true;
        });
    }

    // --- helpers ---
    function setError(input, msgEl, msg) {
        input.classList.add('input-error');
        input.classList.remove('input-success');
        if (msgEl) { msgEl.textContent = msg; msgEl.classList.remove('success'); }
    }

    function setSuccess(input, msgEl, msg) {
        input.classList.remove('input-error');
        input.classList.add('input-success');
        if (msgEl) { msgEl.textContent = msg; msgEl.classList.add('success'); }
    }

    function resetInput(input, msgEl) {
        if (input) { input.classList.remove('input-error', 'input-success'); }
        if (msgEl) { msgEl.textContent = ''; msgEl.classList.remove('success'); }
    }

});