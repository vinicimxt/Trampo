document.addEventListener('DOMContentLoaded', function () {

    /* -----------------------------------------------
       REFERÊNCIAS
    ----------------------------------------------- */
    var cards          = Array.from(document.querySelectorAll('.card-servico'));
    var inputBusca     = document.getElementById('filtro-busca');
    var selectCat      = document.getElementById('filtro-categoria');
    var selectSub      = document.getElementById('filtro-subcategoria');
    var pillsContainer = document.getElementById('filtro-atendimento-pills');
    var contador       = document.getElementById('contadorResultados');
    var totalEl        = document.getElementById('totalServicos');
    var emptyState     = document.getElementById('emptyState');
    var btnLimpar      = document.getElementById('btnLimparFiltros');

    /* estado atual dos filtros */
    var filtros = {
        busca:         '',
        categoria:     '',
        subcategoria:  '',
        atendimento:   ''
    };

    /* -----------------------------------------------
       FILTRO PRINCIPAL — roda toda vez que algo muda
    ----------------------------------------------- */
    function aplicarFiltros() {
        var visiveis = 0;

        cards.forEach(function (card) {
            var nome          = card.dataset.nome         || '';
            var profissional  = card.dataset.profissional || '';
            var categoria     = card.dataset.categoria    || '';
            var subcategoria  = card.dataset.subcategoria || '';
            var atendimento   = card.dataset.atendimento  || '';

            var passaBusca    = !filtros.busca
                || nome.includes(filtros.busca)
                || profissional.includes(filtros.busca);

            var passaCat      = !filtros.categoria    || categoria    === filtros.categoria;
            var passaSub      = !filtros.subcategoria || subcategoria === filtros.subcategoria;
            var passaAtend    = !filtros.atendimento  || atendimento  === filtros.atendimento;

            var visivel = passaBusca && passaCat && passaSub && passaAtend;

            card.style.display = visivel ? '' : 'none';
            if (visivel) visiveis++;
        });

        /* atualiza contador */
        if (contador)  contador.textContent  = visiveis;
        if (totalEl)   totalEl.textContent   = visiveis;

        /* estado vazio */
        if (emptyState) {
            emptyState.classList.toggle('visible', visiveis === 0);
        }

        /* botão limpar */
        var filtroAtivo = filtros.busca || filtros.categoria || filtros.subcategoria || filtros.atendimento;
        if (btnLimpar) btnLimpar.classList.toggle('visible', !!filtroAtivo);

        /*
        ================================================
        GANCHO PARA BACKEND — substituir bloco acima por:

        var params = new URLSearchParams();
        if (filtros.busca)        params.set('q',            filtros.busca);
        if (filtros.categoria)    params.set('categoria',    filtros.categoria);
        if (filtros.subcategoria) params.set('subcategoria', filtros.subcategoria);
        if (filtros.atendimento)  params.set('atendimento',  filtros.atendimento);

        fetch('/Profissional/Filtrar?' + params.toString())
            .then(function(r) { return r.json(); })
            .then(function(data) {
                renderizarCards(data);  // função que reconstrói o grid com os dados
            });

        Atualizar também a URL sem recarregar:
        history.replaceState(null, '', '?' + params.toString());
        ================================================
        */
    }

    /* -----------------------------------------------
       EVENTOS — busca (debounce 300ms)
    ----------------------------------------------- */
    var debounceTimer;
    if (inputBusca) {
        inputBusca.addEventListener('input', function () {
            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(function () {
                filtros.busca = inputBusca.value.toLowerCase().trim();
                aplicarFiltros();
            }, 300);
        });
    }

    /* -----------------------------------------------
       EVENTOS — selects
    ----------------------------------------------- */
    if (selectCat) {
        selectCat.addEventListener('change', function () {
            filtros.categoria = selectCat.value;
            aplicarFiltros();
        });
    }

    if (selectSub) {
        selectSub.addEventListener('change', function () {
            filtros.subcategoria = selectSub.value;
            aplicarFiltros();
        });
    }

    /* -----------------------------------------------
       EVENTOS — pills de atendimento
    ----------------------------------------------- */
    if (pillsContainer) {
        pillsContainer.addEventListener('click', function (e) {
            var pill = e.target.closest('.filter-pill');
            if (!pill) return;

            pillsContainer.querySelectorAll('.filter-pill').forEach(function (p) {
                p.classList.remove('active');
            });
            pill.classList.add('active');

            filtros.atendimento = pill.dataset.atendimento || '';
            aplicarFiltros();
        });
    }

    /* -----------------------------------------------
       LIMPAR FILTROS
    ----------------------------------------------- */
    if (btnLimpar) {
        btnLimpar.addEventListener('click', function () {
            filtros = { busca: '', categoria: '', subcategoria: '', atendimento: '' };

            if (inputBusca)  inputBusca.value  = '';
            if (selectCat)   selectCat.value   = '';
            if (selectSub)   selectSub.value   = '';

            if (pillsContainer) {
                pillsContainer.querySelectorAll('.filter-pill').forEach(function (p) {
                    p.classList.remove('active');
                });
                var first = pillsContainer.querySelector('.filter-pill');
                if (first) first.classList.add('active');
            }

            aplicarFiltros();
        });
    }

    /* -----------------------------------------------
       ANIMAÇÃO STAGGER nos cards ao carregar
    ----------------------------------------------- */
    cards.forEach(function (card, i) {
        card.style.animationDelay = (i * 0.05) + 's';
    });

    /* -----------------------------------------------
       LEITURA DE QUERY PARAMS NA URL
       (útil quando o backend redirecionar com filtros)

       Exemplo: /Profissional?categoria=Beleza&atendimento=Online
    ----------------------------------------------- */
    var urlParams = new URLSearchParams(window.location.search);

    if (urlParams.get('q'))            { filtros.busca        = urlParams.get('q').toLowerCase();    if (inputBusca) inputBusca.value = urlParams.get('q'); }
    if (urlParams.get('categoria'))    { filtros.categoria    = urlParams.get('categoria');           if (selectCat)  selectCat.value  = filtros.categoria; }
    if (urlParams.get('subcategoria')) { filtros.subcategoria = urlParams.get('subcategoria');        if (selectSub)  selectSub.value  = filtros.subcategoria; }
    if (urlParams.get('atendimento'))  {
        filtros.atendimento = urlParams.get('atendimento');
        if (pillsContainer) {
            pillsContainer.querySelectorAll('.filter-pill').forEach(function (p) {
                p.classList.toggle('active', p.dataset.atendimento === filtros.atendimento);
            });
        }
    }

    if (urlParams.toString()) aplicarFiltros();

});