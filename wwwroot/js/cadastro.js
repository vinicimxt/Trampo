document.addEventListener('DOMContentLoaded', function () {

    /* -----------------------------------------------
       REFERÊNCIAS
    ----------------------------------------------- */
    var form       = document.getElementById('formCadastro');
    var tipoSelect = document.getElementById('tipo');
    var telInput   = document.getElementById('telefone');
    var senhaInput = document.getElementById('senha');
    var senhaMsg   = document.getElementById('senhaMsg');
    var docInput   = document.getElementById('documento');
    var tipoDoc    = document.querySelector('select[name="tipoDocumento"]');
    var docMsg     = document.getElementById('docMsg');
    var btnCadastro = document.getElementById('btnCadastro');
    var toggleBtn  = document.getElementById('toggleSenha');

    /* -----------------------------------------------
       MOSTRAR/OCULTAR senha
    ----------------------------------------------- */
    if (toggleBtn && senhaInput) {
        toggleBtn.addEventListener('click', function () {
            var isPass = senhaInput.type === 'password';
            senhaInput.type = isPass ? 'text' : 'password';
            toggleBtn.textContent = isPass ? '🙈' : '👁';
        });
    }

    /* -----------------------------------------------
       MOSTRAR campos profissional
    ----------------------------------------------- */
    window.mostrarCampos = function () {
        var campos = document.getElementById('camposProfissional');
        if (!campos) return;
        campos.style.display = (tipoSelect.value === 'profissional') ? 'block' : 'none';
        controlarBotao();
    };

    /* -----------------------------------------------
       MÁSCARA TELEFONE
    ----------------------------------------------- */
    if (telInput) {
        telInput.addEventListener('input', function () {
            var v = telInput.value.replace(/\D/g, '').slice(0, 11);
            v = v.replace(/^(\d{2})(\d)/, '($1) $2');
            v = v.replace(/(\d{5})(\d)/, '$1-$2');
            telInput.value = v;
        });
    }

    /* -----------------------------------------------
       MÁSCARA CPF / CNPJ
    ----------------------------------------------- */
    function aplicarMascara() {
        if (!docInput || !tipoDoc) return;
        var v = docInput.value.replace(/\D/g, '');

        if (tipoDoc.value === 'CPF') {
            v = v.slice(0, 11);
            v = v.replace(/(\d{3})(\d)/, '$1.$2');
            v = v.replace(/(\d{3})(\d)/, '$1.$2');
            v = v.replace(/(\d{3})(\d{1,2})$/, '$1-$2');
            docInput.maxLength = 14;
        } else {
            v = v.slice(0, 14);
            v = v.replace(/^(\d{2})(\d)/, '$1.$2');
            v = v.replace(/^(\d{2})\.(\d{3})(\d)/, '$1.$2.$3');
            v = v.replace(/\.(\d{3})(\d)/, '.$1/$2');
            v = v.replace(/(\d{4})(\d)/, '$1-$2');
            docInput.maxLength = 18;
        }
        docInput.value = v;
    }

    if (docInput) { docInput.addEventListener('input', function () { aplicarMascara(); validarDocRealtime(); }); }
    if (tipoDoc)  {
        tipoDoc.addEventListener('change', function () {
            docInput.value = '';
            resetInput(docInput, docMsg);
            aplicarMascara();
            controlarBotao();
        });
    }

    /* -----------------------------------------------
       VALIDAR CPF
    ----------------------------------------------- */
    function validarCPF(cpf) {
        cpf = cpf.replace(/\D/g, '');
        if (cpf.length !== 11 || /^(\d)\1+$/.test(cpf)) return false;
        var soma = 0, resto;
        for (var i = 1; i <= 9; i++) soma += parseInt(cpf[i - 1]) * (11 - i);
        resto = (soma * 10) % 11;
        if (resto === 10 || resto === 11) resto = 0;
        if (resto !== parseInt(cpf[9])) return false;
        soma = 0;
        for (var j = 1; j <= 10; j++) soma += parseInt(cpf[j - 1]) * (12 - j);
        resto = (soma * 10) % 11;
        if (resto === 10 || resto === 11) resto = 0;
        return resto === parseInt(cpf[10]);
    }

    /* -----------------------------------------------
       VALIDAR CNPJ
    ----------------------------------------------- */
    function validarCNPJ(cnpj) {
        cnpj = cnpj.replace(/\D/g, '');
        if (cnpj.length !== 14 || /^(\d)\1+$/.test(cnpj)) return false;
        var tam = cnpj.length - 2;
        var nums = cnpj.substring(0, tam);
        var digs = cnpj.substring(tam);
        var soma = 0, pos = tam - 7;
        for (var i = tam; i >= 1; i--) { soma += nums.charAt(tam - i) * pos--; if (pos < 2) pos = 9; }
        var res = soma % 11 < 2 ? 0 : 11 - (soma % 11);
        if (res != digs.charAt(0)) return false;
        tam++; nums = cnpj.substring(0, tam); soma = 0; pos = tam - 7;
        for (var k = tam; k >= 1; k--) { soma += nums.charAt(tam - k) * pos--; if (pos < 2) pos = 9; }
        res = soma % 11 < 2 ? 0 : 11 - (soma % 11);
        return res == digs.charAt(1);
    }

    /* -----------------------------------------------
       VALIDAÇÃO EM TEMPO REAL — documento
    ----------------------------------------------- */
    function validarDocRealtime() {
        if (!docInput || !tipoDoc) return;
        var val = docInput.value;
        if (val.length < 5) { resetInput(docInput, docMsg); controlarBotao(); return; }
        var ok = tipoDoc.value === 'CPF' ? validarCPF(val) : validarCNPJ(val);
        ok ? setSuccess(docInput, docMsg, 'Documento válido ✓') : setError(docInput, docMsg, 'Documento inválido');
        controlarBotao();
    }

    /* -----------------------------------------------
       VALIDAÇÃO SENHA EM TEMPO REAL
    ----------------------------------------------- */
    if (senhaInput) {
        senhaInput.addEventListener('input', function () {
            if (senhaInput.value.length === 0) { resetInput(senhaInput, senhaMsg); return; }
            senhaInput.value.length >= 6
                ? setSuccess(senhaInput, senhaMsg, '')
                : setError(senhaInput, senhaMsg, 'Mínimo 6 caracteres');
        });
    }

    /* -----------------------------------------------
       CONTROLAR BOTÃO
    ----------------------------------------------- */
    function controlarBotao() {
        if (!btnCadastro) return;
        if (!tipoSelect || tipoSelect.value !== 'profissional') {
            btnCadastro.disabled = false;
            return;
        }
        btnCadastro.disabled = !(docInput && docInput.classList.contains('input-success'));
    }

    /* -----------------------------------------------
       SUBMIT
    ----------------------------------------------- */
    if (form) {
        form.addEventListener('submit', function (e) {
            var ok = true;

            if (senhaInput && senhaInput.value.length < 6) {
                setError(senhaInput, senhaMsg, 'Senha muito curta (mín. 6 caracteres)');
                ok = false;
            }

            if (tipoSelect && tipoSelect.value === 'profissional' && docInput && tipoDoc) {
                var docValido = tipoDoc.value === 'CPF'
                    ? validarCPF(docInput.value)
                    : validarCNPJ(docInput.value);
                if (!docValido) {
                    setError(docInput, docMsg, 'Documento inválido');
                    ok = false;
                }
            }

            if (!ok) { e.preventDefault(); return; }

            btnCadastro.textContent = 'Criando conta...';
            btnCadastro.disabled = true;
        });
    }

    /* -----------------------------------------------
       HELPERS
    ----------------------------------------------- */
    function setError(input, msgEl, msg) {
        if (input) { input.classList.add('input-error'); input.classList.remove('input-success'); }
        if (msgEl) { msgEl.textContent = msg; msgEl.classList.remove('success'); }
    }

    function setSuccess(input, msgEl, msg) {
        if (input) { input.classList.remove('input-error'); input.classList.add('input-success'); }
        if (msgEl) { msgEl.textContent = msg; msgEl.classList.add('success'); }
    }

    function resetInput(input, msgEl) {
        if (input) { input.classList.remove('input-error', 'input-success'); }
        if (msgEl) { msgEl.textContent = ''; msgEl.classList.remove('success'); }
    }

});