document.addEventListener('DOMContentLoaded', function () {

    /* -----------------------------------------------
       REFERÊNCIAS
    ----------------------------------------------- */
    var selCategoria   = document.getElementById('categoria');
    var selSub         = document.getElementById('subcategoria');
    var selAtend       = document.getElementById('atendimento');
    var inputNome      = document.getElementById('nome');
    var inputDesc      = document.getElementById('descricao');
    var inputContato   = document.getElementById('contato');
    var inputLink      = document.getElementById('linkOnline');
    var nomeMsg        = document.getElementById('nomeMsg');
    var charCount      = document.getElementById('charCount');
    var localDiv       = document.getElementById('localDiv');
    var linkOnlineDiv  = document.getElementById('linkOnlineDiv');
    var form           = document.getElementById('formServico');
    var btnSubmit      = document.getElementById('btnSubmit');
    var btnText        = document.getElementById('btnText');
    var btnLoader      = document.getElementById('btnLoader');

    // preview
    var prevNome       = document.getElementById('previewNome');
    var prevDesc       = document.getElementById('previewDesc');
    var prevCat        = document.getElementById('previewCategoria');
    var prevAtend      = document.getElementById('previewAtendimento');
    var prevContato    = document.getElementById('previewContato');
    var prevContatoVal = document.getElementById('previewContatoVal');

    /* -----------------------------------------------
       SUBCATEGORIAS DINÂMICAS
    ----------------------------------------------- */
    if (selCategoria) {
        selCategoria.addEventListener('change', function () {
            var catId = this.value;
            var catNome = this.options[this.selectedIndex].text;

            // atualiza preview
            if (prevCat) prevCat.textContent = catId ? catNome : 'Categoria';

            if (!catId) {
                selSub.innerHTML = '<option value="">Selecione a categoria primeiro</option>';
                return;
            }

            fetch('/Servico/GetSubcategorias?categoriaId=' + catId)
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    selSub.innerHTML = '<option value="">Selecione a subcategoria</option>';
                    data.forEach(function (s) {
                        var opt = document.createElement('option');
                        opt.value = s.id;
                        opt.textContent = s.nome;
                        selSub.appendChild(opt);
                    });
                })
                .catch(function () {
                    selSub.innerHTML = '<option value="">Erro ao carregar subcategorias</option>';
                });
        });
    }

    /* -----------------------------------------------
       CAMPOS DINÂMICOS (atendimento)
    ----------------------------------------------- */
    function toggleCampos() {
        if (!selAtend) return;
        var tipo = selAtend.value;

        // local
        if (localDiv) localDiv.classList.toggle('hidden', tipo !== 'Local');

        // link online
        if (linkOnlineDiv) linkOnlineDiv.classList.toggle('hidden', tipo !== 'Online');

        // aviso sem local
        if (tipo === 'Local') {
            var hasLocais = document.querySelectorAll("select[name='localId'] option").length > 1;
            if (!hasLocais) {
                // não há locais — já exibe o alerta-local via Razor, não precisa de alert
            }
        }

        // preview atendimento
        if (prevAtend) {
            var icons = { Domicilio: '🏠 Domicílio', Local: '📍 Local', Online: '💻 Online' };
            prevAtend.textContent = icons[tipo] || tipo;
        }
    }

    if (selAtend) {
        selAtend.addEventListener('change', toggleCampos);
        toggleCampos(); // inicializa
    }

    /* -----------------------------------------------
       VALIDAÇÃO DO NOME
    ----------------------------------------------- */
    if (inputNome) {
        inputNome.addEventListener('input', function () {
            var val = inputNome.value.trim();

            if (val.length === 0) {
                inputNome.classList.remove('input-error', 'input-valid');
                if (nomeMsg) { nomeMsg.textContent = ''; nomeMsg.className = 'input-msg'; }
                if (prevNome) prevNome.textContent = 'Nome do serviço';
                return;
            }

            if (val.length < 3) {
                inputNome.classList.add('input-error');
                inputNome.classList.remove('input-valid');
                if (nomeMsg) { nomeMsg.textContent = 'Mínimo 3 caracteres'; nomeMsg.className = 'input-msg'; }
            } else {
                inputNome.classList.remove('input-error');
                inputNome.classList.add('input-valid');
                if (nomeMsg) { nomeMsg.textContent = ''; nomeMsg.className = 'input-msg ok'; }
            }

            // preview
            if (prevNome) prevNome.textContent = val || 'Nome do serviço';
        });
    }

    /* -----------------------------------------------
       CONTADOR DE DESCRIÇÃO
    ----------------------------------------------- */
    if (inputDesc) {
        inputDesc.addEventListener('input', function () {
            var len = inputDesc.value.length;
            if (charCount) {
                charCount.textContent = len + ' / 200';
                charCount.className = 'char-count';
                if (len > 160) charCount.classList.add('quase');
                if (len >= 200) charCount.classList.add('cheio');
            }
            // preview
            if (prevDesc) prevDesc.textContent = inputDesc.value || 'A descrição aparecerá aqui...';
        });
    }

    /* -----------------------------------------------
       PREVIEW — contato
    ----------------------------------------------- */
    if (inputContato) {
        inputContato.addEventListener('input', function () {
            var val = inputContato.value.trim();
            if (prevContato && prevContatoVal) {
                prevContato.style.display = val ? 'flex' : 'none';
                prevContatoVal.textContent = val;
            }
        });
    }

    /* -----------------------------------------------
       LOADING NO SUBMIT
    ----------------------------------------------- */
    if (form) {
        form.addEventListener('submit', function () {
            // valida nome mínimo
            if (inputNome && inputNome.value.trim().length < 3) {
                inputNome.classList.add('input-error');
                if (nomeMsg) nomeMsg.textContent = 'Mínimo 3 caracteres';
                event.preventDefault();
                return;
            }

            if (btnSubmit) btnSubmit.disabled = true;
            if (btnText)   btnText.textContent = 'Criando...';
            if (btnLoader) btnLoader.classList.remove('hidden');
        });
    }

});