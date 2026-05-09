document.addEventListener('DOMContentLoaded', function () {

    /* -----------------------------------------------
       MOSTRAR / OCULTAR SENHA
    ----------------------------------------------- */
    window.toggleSenha = function (inputId, btn) {
        var input = document.getElementById(inputId);
        if (!input) return;
        var isPass = input.type === 'password';
        input.type = isPass ? 'text' : 'password';
        btn.querySelector('.material-symbols-rounded').textContent =
            isPass ? 'visibility_off' : 'visibility';
    };

    /* -----------------------------------------------
       FORÇA DA SENHA
    ----------------------------------------------- */
    window.avaliarForca = function (val) {
        var wrap = document.getElementById('forcaWrap');
        var b1 = document.getElementById('b1');
        var b2 = document.getElementById('b2');
        var b3 = document.getElementById('b3');
        var lbl = document.getElementById('forcaLabel');
        if (!wrap) return;

        if (!val) { wrap.style.display = 'none'; return; }
        wrap.style.display = 'flex';

        var forca = 0;
        if (val.length >= 8) forca++;
        if (/[A-Z]/.test(val) && /[a-z]/.test(val)) forca++;
        if (/[0-9]/.test(val) || /[^a-zA-Z0-9]/.test(val)) forca++;

        var barras = [b1, b2, b3];
        barras.forEach(function (b) {
            b.className = 'forca-barra';
        });

        if (forca === 1) {
            b1.classList.add('ativa-fraca');
            lbl.textContent = 'Fraca'; lbl.style.color = '#ef4444';
        } else if (forca === 2) {
            b1.classList.add('ativa-media');
            b2.classList.add('ativa-media');
            lbl.textContent = 'Média'; lbl.style.color = '#f59e0b';
        } else {
            barras.forEach(function (b) { b.classList.add('ativa-forte'); });
            lbl.textContent = 'Forte'; lbl.style.color = '#22c55e';
        }

        verificarConfirmacao();
    };

    /* -----------------------------------------------
       CONFIRMAR SENHA
    ----------------------------------------------- */
    window.verificarConfirmacao = function () {
        var nova = document.getElementById('novaSenha');
        var confirmar = document.getElementById('confirmarSenha');
        var msg = document.getElementById('confirmarMsg');
        if (!nova || !confirmar || !msg) return;

        if (!confirmar.value) {
            msg.textContent = ''; confirmar.classList.remove('valid', 'invalid'); return;
        }

        if (nova.value === confirmar.value) {
            confirmar.classList.add('valid');
            confirmar.classList.remove('invalid');
            msg.className = 'field-msg ok';
            msg.innerHTML = '<span class="material-symbols-rounded">check_circle</span> Senhas conferem';
        } else {
            confirmar.classList.add('invalid');
            confirmar.classList.remove('valid');
            msg.className = 'field-msg err';
            msg.innerHTML = '<span class="material-symbols-rounded">cancel</span> Senhas não conferem';
        }
    };

    /* -----------------------------------------------
       MÁSCARA DE TELEFONE
    ----------------------------------------------- */
    var telInput = document.getElementById('telefone');
    if (telInput) {
        telInput.addEventListener('input', function () {
            var v = telInput.value.replace(/\D/g, '').slice(0, 11);
            if (v.length <= 10) {
                v = v.replace(/^(\d{2})(\d{4})(\d{0,4})/, '($1) $2-$3');
            } else {
                v = v.replace(/^(\d{2})(\d{5})(\d{0,4})/, '($1) $2-$3');
            }
            telInput.value = v;
        });
    }

    /* -----------------------------------------------
       VALIDAÇÃO DO NOME
    ----------------------------------------------- */
    var nomeInput = document.getElementById('nome');
    var nomeMsg = document.getElementById('nomeMsg');
    if (nomeInput) {
        nomeInput.addEventListener('input', function () {
            var val = nomeInput.value.trim();
            if (!val) { nomeMsg.textContent = ''; nomeInput.classList.remove('valid', 'invalid'); return; }
            if (val.length < 3) {
                nomeInput.classList.add('invalid'); nomeInput.classList.remove('valid');
                nomeMsg.className = 'field-msg err';
                nomeMsg.innerHTML = '<span class="material-symbols-rounded">cancel</span> Mínimo 3 caracteres';
            } else {
                nomeInput.classList.add('valid'); nomeInput.classList.remove('invalid');
                nomeMsg.className = 'field-msg ok';
                nomeMsg.innerHTML = '<span class="material-symbols-rounded">check_circle</span>';
            }
        });
    }

});