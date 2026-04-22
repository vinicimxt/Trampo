document.addEventListener('DOMContentLoaded', function () {

    /* -----------------------------------------------
       REFERÊNCIAS
    ----------------------------------------------- */
    var pagina       = document.getElementById('pagina');
    var horInput     = document.getElementById('horaSelecionada');
    var btnConfirmar = document.getElementById('btnConfirmar');
    var hintConfirmar= document.getElementById('hintConfirmar');
    var resumoHora   = document.getElementById('resumoHora');
    var resumoAvatar = document.getElementById('resumoAvatar');
    var resumoNome   = document.getElementById('resumoNome');
    var enderecoDiv  = document.getElementById('enderecoDiv');

    if (!pagina) return;

    var atendimento = (pagina.getAttribute('data-atendimento') || '').toLowerCase().trim();

    /* -----------------------------------------------
       AVATAR — iniciais do nome do serviço
    ----------------------------------------------- */
    if (resumoAvatar && resumoNome) {
        var nome = resumoNome.textContent.trim();
        var iniciais = nome
            .split(' ')
            .slice(0, 2)
            .map(function (p) { return p[0] ? p[0].toUpperCase() : ''; })
            .join('');
        resumoAvatar.textContent = iniciais || '?';
    }

    /* -----------------------------------------------
       ENDEREÇO — só aparece no domicílio
    ----------------------------------------------- */
    if (enderecoDiv) {
        if (atendimento.includes('domicilio')) {
            enderecoDiv.classList.add('visivel');
        } else {
            enderecoDiv.classList.remove('visivel');
        }
    }

    /* -----------------------------------------------
       SELECIONAR HORÁRIO
    ----------------------------------------------- */
    window.selecionarHora = function (hora, elemento) {

        /* remove seleção anterior */
        document.querySelectorAll('.slot').forEach(function (el) {
            el.classList.remove('selecionado');
        });

        /* marca novo */
        elemento.classList.add('selecionado');

        /* atualiza input hidden */
        if (horInput) horInput.value = hora;

        /* atualiza resumo lateral */
        if (resumoHora) {
            resumoHora.textContent = hora;
            resumoHora.classList.remove('empty');
        }

        /* habilita botão */
        if (btnConfirmar) {
            btnConfirmar.disabled = false;
        }

        /* atualiza hint */
        if (hintConfirmar) {
            hintConfirmar.textContent = 'Tudo pronto! Clique para confirmar.';
            hintConfirmar.style.color = '#00a37a';
        }
    };

    /* -----------------------------------------------
       VALIDAÇÃO NO SUBMIT
    ----------------------------------------------- */
    var form = document.getElementById('formAgendamento');
    if (form) {
        form.addEventListener('submit', function (e) {

            /* horário obrigatório */
            if (!horInput || !horInput.value) {
                e.preventDefault();
                alert('Por favor, selecione um horário antes de confirmar.');
                return;
            }

            /* endereço obrigatório para domicílio */
            if (atendimento.includes('domicilio')) {
                var rua    = document.getElementById('rua');
                var numero = document.getElementById('numero');
                var cidade = document.getElementById('cidade');

                if (!rua?.value.trim() || !numero?.value.trim() || !cidade?.value.trim()) {
                    e.preventDefault();
                    alert('Preencha o endereço completo para atendimento em domicílio.');
                    return;
                }
            }

            /* loading no botão */
            if (btnConfirmar) {
                btnConfirmar.textContent = 'Confirmando...';
                btnConfirmar.disabled = true;
            }
        });
    }

});

// =======================
//  MENSAGEM SUCESSO
// =======================

setTimeout(() => {
    const toast = document.getElementById("toastSucesso");
    if (toast) {
        toast.style.opacity = "0";
        toast.style.transform = "translateX(50px)";
    }
}, 3000);