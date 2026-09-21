(() => {
    function iniciar(grupo) {
        const campo = nome => grupo.querySelector(`[data-campo="${nome}"]`);
        const status = grupo.querySelector('[data-status]');
        const resultados = grupo.querySelector('[data-resultados]');
        let sequencia = 0;
        let ativo;
        const resumo = () => {
            grupo.querySelector('[data-resumo]').textContent = ['logradouro', 'numero', 'complemento', 'bairro', 'cidade', 'uf', 'cep']
                .map(n => campo(n).value.trim()).filter(Boolean).join(' - ') || 'Preencha os campos acima.';
        };
        grupo.addEventListener('input', () => { sequencia++; ativo?.abort(); resumo(); });
        campo('cep').addEventListener('blur', () => {
            const cep = campo('cep').value.trim();
            if (/^\d{5}-?\d{3}$/.test(cep)) campo('cep').value = cep.replace('-', '').replace(/^(\d{5})(\d{3})$/, '$1-$2');
        });
        const preencher = dados => {
            for (const nome of ['cep', 'logradouro', 'bairro', 'cidade', 'uf']) campo(nome).value = dados[nome] || '';
            resumo(); campo('numero').focus();
            status.textContent = 'Endereço encontrado. Confira e complete número e complemento.';
        };
        async function consultar(pesquisa) {
            ativo?.abort(); ativo = new AbortController(); const atual = ++sequencia;
            resultados.replaceChildren();
            const cep = campo('cep').value.trim();
            const uf = campo('uf').value.trim().toUpperCase();
            const cidade = campo('cidade').value.trim();
            const logradouro = campo('logradouro').value.trim();
            if (!pesquisa && !/^\d{5}-?\d{3}$/.test(cep)) { status.textContent = 'Informe um CEP com oito dígitos.'; campo('cep').focus(); return; }
            if (pesquisa && (uf.length !== 2 || cidade.length < 3 || logradouro.length < 3)) { status.textContent = 'Informe UF, cidade e logradouro com ao menos três caracteres.'; return; }
            const qs = new URLSearchParams(pesquisa ? { uf, cidade, logradouro } : { cep });
            grupo.setAttribute('aria-busy', 'true'); status.textContent = 'Consultando endereço…';
            const botoes = grupo.querySelectorAll('[data-consultar],[data-pesquisar]'); botoes.forEach(b => b.disabled = true);
            try {
                const resposta = await fetch(`/Endereco/${pesquisa ? 'Pesquisar' : 'Cep'}?${qs}`, { signal: ativo.signal, credentials: 'same-origin', headers: { Accept: 'application/json' } });
                const dados = await resposta.json();
                if (atual !== sequencia) return;
                if (!resposta.ok) { status.textContent = dados.erro?.mensagem || 'Não foi possível consultar. Preencha manualmente.'; return; }
                if (!pesquisa) preencher(dados);
                else {
                    status.textContent = dados.length ? 'Escolha um endereço abaixo e confira os campos.' : 'Nenhum endereço encontrado. Refine a pesquisa ou preencha manualmente.';
                    dados.forEach(d => {
                        const li = document.createElement('li'); const botao = document.createElement('button'); botao.type = 'button';
                        botao.textContent = `${d.logradouro} - ${d.bairro}, ${d.cidade}/${d.uf} - ${d.cep}`;
                        botao.addEventListener('click', () => { preencher(d); resultados.replaceChildren(); }); li.append(botao); resultados.append(li);
                    });
                }
            } catch (erro) {
                if (erro.name !== 'AbortError' && atual === sequencia) status.textContent = 'Consulta indisponível. Você pode preencher todos os campos manualmente.';
            } finally {
                grupo.removeAttribute('aria-busy'); botoes.forEach(b => b.disabled = false);
            }
        }
        grupo.querySelector('[data-consultar]').addEventListener('click', () => consultar(false));
        grupo.querySelector('[data-pesquisar]').addEventListener('click', () => consultar(true));
        grupo.preencherEndereco = dados => {
            sequencia++; ativo?.abort();
            for (const n of ['cep','logradouro','numero','complemento','bairro','cidade','uf']) campo(n).value = dados?.[n] || '';
            status.textContent = ''; resultados.replaceChildren(); resumo();
        };
    }
    document.querySelectorAll('[data-endereco]').forEach(iniciar);
    const modo = document.getElementById('localEstruturado');
    if (modo) {
        window.modoEnderecoLocal = () => {
            const grupo = document.querySelector('#formLocal [data-endereco]');
            grupo.disabled = !modo.checked; grupo.hidden = !modo.checked;
            const legado = document.getElementById('inputEndereco');
            legado.required = !modo.checked; legado.disabled = modo.checked;
            legado.closest('.endereco-legado').hidden = modo.checked;
        };
        modo.addEventListener('change', window.modoEnderecoLocal);
        window.modoEnderecoLocal();
    }
})();
