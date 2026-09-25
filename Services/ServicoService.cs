using BD_TRAMPO.Contracts;
using BD_TRAMPO.DAO;

namespace BD_TRAMPO.Services;

public sealed class ServicoService(ServicoDAO servicos,
    ProfissionalDAO profissionais, LocalDAO locais, SubcategoriaDAO subcategorias,
    CategoriaDAO categorias, AgendamentoDAO agendamentos, UsuarioDAO usuarios)
{
    public List<Servico> ListarPublicos(int pagina, int tamanho, string? busca = null, string? atendimento = null)
    {
        if (pagina < 1 || pagina > 1000000 || tamanho < 1 || tamanho > 100)
            throw new FalhaOperacao(TipoFalha.Validacao, "Paginação inválida.");
        if ((busca?.Length ?? 0)>100 || (atendimento != null && !new[]{"Online","Local","Domicilio"}.Contains(atendimento)))
            throw new FalhaOperacao(TipoFalha.Validacao,"Filtro inválido.");
        return servicos.ListarPublicos(pagina, tamanho, busca?.Trim(), atendimento);
    }

    public Servico BuscarPublico(int id)
    {
        var servico = servicos.ListarPublicos(1, 1, id: id).FirstOrDefault();
        if (servico == null || !servico.Ativo)
            throw new FalhaOperacao(TipoFalha.NaoEncontrado, "Serviço não encontrado.");
        return servico;
    }
    public int Profissional(UsuarioContexto usuario)
    {
        var persistido = usuarios.BuscarPorId(usuario.UsuarioId);
        if (usuario.UsuarioId <= 0 || persistido == null)
            throw new FalhaOperacao(TipoFalha.NaoAutenticado, "Entre na sua conta.");
        if (!string.Equals(persistido.Tipo, "profissional", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(usuario.Tipo, "profissional", StringComparison.OrdinalIgnoreCase))
            throw new FalhaOperacao(TipoFalha.SemPermissao, "Perfil profissional necessário.");
        int id = profissionais.BuscarPorUsuario(usuario.UsuarioId);
        if (id <= 0) throw new FalhaOperacao(TipoFalha.SemPermissao, "Cadastro profissional necessário.");
        return id;
    }

    public Servico BuscarProprio(UsuarioContexto usuario, int id)
    {
        int profissional = Profissional(usuario);
        var servico = servicos.BuscarPorId(id)
            ?? throw new FalhaOperacao(TipoFalha.NaoEncontrado, "Serviço não encontrado.");
        if (servico.ProfissionalId != profissional)
            throw new FalhaOperacao(TipoFalha.SemPermissao, "Este serviço pertence a outro profissional.");
        return servico;
    }

    public (List<Local> Locais, List<Categoria> Categorias, int Total, int Pendentes) PrepararCriacao(UsuarioContexto usuario)
    {
        int id = Profissional(usuario);
        return (locais.ListarPorProfissional(id), categorias.Listar(),
            servicos.ContarPorProfissional(id), agendamentos.ContarPendentesProfissional(id));
    }

    public List<Subcategoria> Subcategorias(int categoria) => subcategorias.ListarPorCategoria(categoria);

    public int Criar(UsuarioContexto usuario, SalvarServicoRequest dados) => Salvar(usuario, dados, false);
    public void Editar(UsuarioContexto usuario, SalvarServicoRequest dados) => Salvar(usuario, dados, true);

    private int Salvar(UsuarioContexto usuario, SalvarServicoRequest d, bool editar)
    {
        int profissional = Profissional(usuario);
        var original = editar ? BuscarProprio(usuario, d.Id) : null;
        if (string.IsNullOrWhiteSpace(d.Nome) || d.Nome.Length > 100 || (d.Descricao?.Length ?? 0) > 255 ||
            !new[] { "Local", "Domicilio", "Online" }.Contains(d.Atendimento) ||
            !new[] { "Fixo", "Combinar" }.Contains(d.TipoPreco))
            throw new FalhaOperacao(TipoFalha.Validacao, "Dados do serviço inválidos.");
        if (d.TipoPreco == "Fixo" && (!d.PrecoBase.HasValue || d.PrecoBase <= 0 || d.PrecoBase > 99999999.99m))
            throw new FalhaOperacao(TipoFalha.Validacao, "Preço inválido.");
        if (d.Atendimento == "Online" && (!Seguranca.UrlHttpValida(d.LinkOnline ?? "") || (d.LinkOnline?.Length ?? 0) > 255))
            throw new FalhaOperacao(TipoFalha.Validacao, "Informe um link HTTP ou HTTPS válido.");
        if (d.Atendimento == "Local" && (!d.LocalId.HasValue || locais.BuscarPorId(d.LocalId.Value)?.ProfissionalId != profissional))
            throw new FalhaOperacao(TipoFalha.Validacao, "O local deve pertencer ao profissional.");
        if (!d.HoraInicio.HasValue || !d.HoraFim.HasValue ||
            d.HoraInicio < TimeSpan.Zero || d.HoraInicio >= TimeSpan.FromDays(1) ||
            d.HoraFim < TimeSpan.Zero || d.HoraFim >= TimeSpan.FromDays(1) || d.HoraInicio == d.HoraFim ||
            string.IsNullOrWhiteSpace(d.DiasSemana) ||
            d.DiasSemana.Split(',').Any(x => !int.TryParse(x, out int dia) || dia < 0 || dia > 6))
            throw new FalhaOperacao(TipoFalha.Validacao, "Disponibilidade inválida.");
        int subcategoria = original?.SubcategoriaId ?? d.SubcategoriaId;
        if (!subcategorias.ListarTodas().Any(x => x.Id == subcategoria))
            throw new FalhaOperacao(TipoFalha.Validacao, "Subcategoria inválida.");
        var servico = new Servico {
            Id = original?.Id ?? 0, ProfissionalId = profissional, SubcategoriaId = subcategoria,
            Nome = d.Nome, Descricao = d.Descricao ?? "", Atendimento = d.Atendimento,
            LocalId = d.Atendimento == "Local" ? d.LocalId : null,
            LinkOnline = d.Atendimento == "Online" ? d.LinkOnline ?? "" : "",
            TipoPreco = d.TipoPreco, PrecoBase = d.TipoPreco == "Fixo" ? d.PrecoBase : null
        };
        return servicos.SalvarComDisponibilidade(servico, d.DiasSemana.Split(',').Select(int.Parse),
            d.HoraInicio.Value, d.HoraFim.Value);
    }

    public bool Remover(UsuarioContexto usuario, int id)
    {
        BuscarProprio(usuario, id);
        return servicos.Excluir(id); // A decisão entre excluir/desativar continua dentro da transação.
    }

    // Mantém a semântica da interface atual: remover da oferta preservando histórico.
    public void Desativar(UsuarioContexto usuario, int id) => Remover(usuario, id);
}