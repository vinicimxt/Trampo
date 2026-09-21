using BD_TRAMPO.Contracts;
using BD_TRAMPO.DAO;
using Microsoft.Data.SqlClient;

namespace BD_TRAMPO.Services;

public sealed class AgendamentoService(AgendamentoDAO agendamentos, ServicoDAO servicos,
    ClienteDAO clientes, ProfissionalDAO profissionais, DisponibilidadeDAO disponibilidades,
    AvaliacaoDAO avaliacoes, UsuarioDAO usuarios)
{
    public List<Agendamento> Listar(UsuarioContexto usuario, string visao) => visao switch {
        "meus" => Meus(usuario), "recebidos" => Recebidos(usuario),
        _ => throw new FalhaOperacao(TipoFalha.Validacao, "Visão deve ser meus ou recebidos.")
    };

    public void Avaliar(UsuarioContexto usuario, int id, int nota, string? comentario)
    {
        var ag = BuscarParticipante(usuario, id, false);
        Avaliar(usuario, new AvaliarAgendamentoRequest {
            AgendamentoId=id, ProfissionalId=ag.ProfissionalId, Nota=nota, Comentario=comentario
        });
    }
    private void Autenticar(UsuarioContexto usuario)
    {
        var persistido = usuarios.BuscarPorId(usuario.UsuarioId);
        if (usuario.UsuarioId <= 0 || persistido == null)
            throw new FalhaOperacao(TipoFalha.NaoAutenticado, "Entre na sua conta.");
        if (!string.Equals(persistido.Tipo, usuario.Tipo, StringComparison.OrdinalIgnoreCase) ||
            !(string.Equals(usuario.Tipo, "cliente", StringComparison.OrdinalIgnoreCase) ||
              string.Equals(usuario.Tipo, "profissional", StringComparison.OrdinalIgnoreCase)))
            throw new FalhaOperacao(TipoFalha.SemPermissao, "Perfil sem acesso a agendamentos.");
    }

    private int Profissional(UsuarioContexto usuario)
    {
        Autenticar(usuario);
        int id = profissionais.BuscarPorUsuario(usuario.UsuarioId);
        if (!string.Equals(usuario.Tipo, "profissional", StringComparison.OrdinalIgnoreCase) || id <= 0)
            throw new FalhaOperacao(TipoFalha.SemPermissao, "Perfil profissional necessário.");
        return id;
    }

    public Agendamento BuscarParticipante(UsuarioContexto usuario, int id, bool? comoProfissional = null)
    {
        Autenticar(usuario);
        var ag = agendamentos.BuscarPorId(id)
            ?? throw new FalhaOperacao(TipoFalha.NaoEncontrado, "Agendamento não encontrado.");
        bool cliente = ag.UsuarioId == usuario.UsuarioId;
        bool profissional = string.Equals(usuario.Tipo, "profissional", StringComparison.OrdinalIgnoreCase) &&
            profissionais.BuscarPorUsuario(usuario.UsuarioId) == ag.ProfissionalId;
        if (comoProfissional == true ? !profissional : comoProfissional == false ? !cliente : !(cliente || profissional))
            throw new FalhaOperacao(TipoFalha.SemPermissao, "Você não participa deste agendamento.");
        return ag;
    }

    public AgendaDisponivel ConsultarAgenda(UsuarioContexto usuario, int servicoId, DateTime dia)
    {
        Autenticar(usuario);
        var servico = servicos.BuscarPorId(servicoId)
            ?? throw new FalhaOperacao(TipoFalha.NaoEncontrado, "Serviço não encontrado.");
        if (!servico.Ativo) throw new FalhaOperacao(TipoFalha.Validacao, "Serviço inativo.");
        if (dia.Date < DateTime.Today || dia.Date > DateTime.Today.AddMonths(3))
            throw new FalhaOperacao(TipoFalha.Validacao, "Data inválida.");
        var regras = disponibilidades.BuscarPorServico(servicoId);
        var intervalos = agendamentos.IntervalosIndisponiveis(servico.ProfissionalId, dia);
        var horarios = RegrasAgenda.Horarios(regras, dia)
            .Where(h => h > DateTime.Now && !intervalos.Any(i => RegrasAgenda.Sobrepoe(h, h.AddHours(1), i.Inicio, i.Fim)))
            .Select(h => h.TimeOfDay).Distinct().OrderBy(h => h).ToList();
        return new(servico, profissionais.BuscarPorId(servico.ProfissionalId)?.Nome ?? "",
            dia.Date, regras, horarios);
    }

    public int Criar(UsuarioContexto usuario, CriarAgendamentoRequest dados)
    {
        Autenticar(usuario);
        var servico = servicos.BuscarPorId(dados.ServicoId)
            ?? throw new FalhaOperacao(TipoFalha.NaoEncontrado, "Serviço não encontrado.");
        if (!servico.Ativo) throw new FalhaOperacao(TipoFalha.Validacao, "Serviço inativo.");
        if (profissionais.BuscarPorUsuario(usuario.UsuarioId) == servico.ProfissionalId)
            throw new FalhaOperacao(TipoFalha.Validacao, "Você não pode agendar seu próprio serviço.");
        if (!dados.Hora.HasValue || dados.Hora < TimeSpan.Zero || dados.Hora >= TimeSpan.FromDays(1) ||
            dados.Data.Date + dados.Hora.Value <= DateTime.Now || dados.Data.Date > DateTime.Today.AddMonths(3))
            throw new FalhaOperacao(TipoFalha.Validacao, "Data ou horário inválido.");
        string? endereco = null;
        EnderecoDados? snapshot = null;
        if (servico.Atendimento == "Domicilio")
        {
            if(dados.Endereco!=null) { snapshot=EnderecoService.Validar(dados.Endereco); endereco=snapshot.EnderecoFormatado; }
            else {
            if (new[] { dados.Rua, dados.Numero, dados.Bairro, dados.Cidade }.Any(string.IsNullOrWhiteSpace))
                throw new FalhaOperacao(TipoFalha.Validacao, "Preencha o endereço completo.");
            endereco = $"{dados.Rua}, {dados.Numero} - {dados.Bairro}, {dados.Cidade}";
            }
        }
        if ((dados.Descricao?.Length ?? 0) > 255 || (endereco?.Length ?? 0) > 255)
            throw new FalhaOperacao(TipoFalha.Validacao, "Descrição ou endereço muito longo.");
        int cliente = clientes.ObterOuCriar(usuario.UsuarioId);
        // O DAO revalida disponibilidade, bloqueios, participante e conflito sob os bloqueios SQL.
        return agendamentos.Inserir(new Agendamento {
            ClienteId = cliente, ServicoId = servico.Id, ProfissionalId = servico.ProfissionalId,
            Data = dados.Data.Date, Hora = dados.Hora.Value, Descricao = dados.Descricao ?? "",
            EnderecoAtendimento = snapshot, EnderecoCliente = endereco, LocalId = servico.LocalId, Status = "Pendente"
        });
    }

    public List<Agendamento> Meus(UsuarioContexto usuario)
    {
        Autenticar(usuario);
        return agendamentos.ListarPorCliente(clientes.BuscarClienteIdPorUsuario(usuario.UsuarioId), usuario.UsuarioId);
    }

    public List<Agendamento> Recebidos(UsuarioContexto usuario) =>
        agendamentos.ListarPorProfissional(Profissional(usuario));

    private static void ExigirAlteracao(bool alterou)
    {
        if (!alterou) throw new FalhaOperacao(TipoFalha.Conflito, "O pedido já foi alterado ou não permite esta operação.");
    }

    private static Notificacao Aviso(int usuario, int id, string titulo, string mensagem, string tipo) =>
        new() { UsuarioId = usuario, ReferenciaId = id, Titulo = titulo, Mensagem = mensagem, Tipo = tipo };

    public void Confirmar(UsuarioContexto usuario, int id)
    {
        var ag = BuscarParticipante(usuario, id, true);
        ExigirAlteracao(agendamentos.Confirmar(id, ag.ProfissionalId, [
            Aviso(ag.UsuarioId, id, "Agendamento confirmado ✔",
                $"Seu agendamento para {ag.Data:dd/MM} às {ag.Hora} foi confirmado", "Agendamento")]));
    }

    public void Recusar(UsuarioContexto usuario, int id)
    {
        var ag = BuscarParticipante(usuario, id, true);
        ExigirAlteracao(agendamentos.Cancelar(id, "CanceladoProfissional", usuario.UsuarioId, [
            Aviso(ag.UsuarioId, id, "Agendamento recusado ❌", "O profissional recusou seu agendamento.", "Cancelamento")],
            somentePendente: true));
    }

    public void Cancelar(UsuarioContexto usuario, int id)
    {
        var ag = BuscarParticipante(usuario, id);
        bool cliente = ag.UsuarioId == usuario.UsuarioId;
        int destinatario = cliente ? profissionais.BuscarUsuarioId(ag.ProfissionalId) : ag.UsuarioId;
        ExigirAlteracao(agendamentos.Cancelar(id, cliente ? "CanceladoCliente" : "CanceladoProfissional", usuario.UsuarioId, [
            Aviso(destinatario, id, "Agendamento cancelado ❌", cliente ? "Um cliente cancelou um agendamento." :
                "Seu agendamento foi cancelado pelo profissional.", "Cancelamento"),
            Aviso(usuario.UsuarioId, id, "Agendamento cancelado ❌", "Você cancelou o agendamento.", "Cancelamento")]));
    }

    public void Finalizar(UsuarioContexto usuario, int id, decimal valor)
    {
        var ag = BuscarParticipante(usuario, id, true);
        var servico = servicos.BuscarPorId(ag.ServicoId)
            ?? throw new FalhaOperacao(TipoFalha.NaoEncontrado, "Serviço não encontrado.");
        var profissional = profissionais.BuscarPorId(ag.ProfissionalId)
            ?? throw new FalhaOperacao(TipoFalha.NaoEncontrado, "Profissional não encontrado.");
        if (servico.TipoPreco == "Fixo") valor = servico.PrecoBase ?? 0;
        if (valor <= 0 || valor > 99999999.99m)
            throw new FalhaOperacao(TipoFalha.Validacao, "Informe um valor válido.");
        if (ag.Data.Date + ag.Hora > DateTime.Now)
            throw new FalhaOperacao(TipoFalha.Validacao, "O atendimento ainda não começou.");
        decimal taxa = Math.Round(valor * (profissional.Plano == "Premium" ? .04m : .10m), 2);
        ExigirAlteracao(agendamentos.Finalizar(id, ag.ProfissionalId, valor, taxa, valor - taxa, [
            Aviso(ag.UsuarioId, id, "Atendimento finalizado ✔",
                "O profissional marcou o atendimento como concluído.", "Finalizacao")]));
    }

    public void ConfirmarConclusao(UsuarioContexto usuario, int id)
    {
        var ag = BuscarParticipante(usuario, id, false);
        ExigirAlteracao(agendamentos.ConfirmarCliente(id, usuario.UsuarioId, [
            Aviso(profissionais.BuscarUsuarioId(ag.ProfissionalId), id, "Serviço finalizado ✔",
                "O cliente confirmou a conclusão do atendimento.", "Finalizacao")]));
    }

    public void PermitirAvaliacao(UsuarioContexto usuario, int id)
    {
        var ag = BuscarParticipante(usuario, id, false);
        if (!ag.PodeAvaliar()) throw new FalhaOperacao(TipoFalha.Validacao, "O atendimento ainda não foi concluído.");
        if (avaliacoes.JaAvaliou(id, usuario.UsuarioId))
            throw new FalhaOperacao(TipoFalha.Conflito, "Este atendimento já foi avaliado.");
    }

    public void Avaliar(UsuarioContexto usuario, AvaliarAgendamentoRequest dados)
    {
        var ag = BuscarParticipante(usuario, dados.AgendamentoId, false);
        PermitirAvaliacao(usuario, dados.AgendamentoId);
        if (dados.ProfissionalId != ag.ProfissionalId || dados.Nota < 1 || dados.Nota > 5 ||
            (dados.Comentario?.Length ?? 0) > 500)
            throw new FalhaOperacao(TipoFalha.Validacao, "Avaliação inválida.");
        try { avaliacoes.Inserir(new Avaliacao { AgendamentoId = ag.Id, UsuarioId = usuario.UsuarioId,
            ProfissionalId = ag.ProfissionalId, Nota = dados.Nota, Comentario = dados.Comentario ?? "" }); }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        { throw new FalhaOperacao(TipoFalha.Conflito, "Este atendimento já foi avaliado."); }
    }
}