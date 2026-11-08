using System.Text.Json;
using BD_TRAMPO;
using BD_TRAMPO.Api.Contracts;

var agendamento = new Agendamento
{
    Id = 12,
    ServicoId = 215,
    ProfissionalId = 9,
    Data = new DateTime(2026, 11, 10),
    Hora = new TimeSpan(10, 0, 0),
    Status = "Pendente",
    Descricao = "",
    Servico = "Design de landing page",
    NomeProfissional = "Ana",
    NomeCliente = "Maria",
    Atendimento = "Online"
};

var resposta = AgendamentoResponse.De(agendamento);
using var json = JsonDocument.Parse(JsonSerializer.Serialize(resposta,
    new JsonSerializerOptions(JsonSerializerDefaults.Web)));
var raiz = json.RootElement;

if (raiz.GetProperty("servicoNome").GetString() != agendamento.Servico ||
    raiz.GetProperty("profissionalNome").GetString() != agendamento.NomeProfissional ||
    raiz.GetProperty("clienteNome").GetString() != agendamento.NomeCliente ||
    raiz.GetProperty("atendimento").GetString() != agendamento.Atendimento)
    throw new Exception("Os dados exibidos no agendamento não foram enviados no contrato HTTP.");

Console.WriteLine("PASSOU: o contrato do agendamento inclui nome do serviço e participantes.");

var servico = ServicoResponse.De(new Servico { Id = 215, ProfissionalId = 9,
    Nome = "Design de landing page", Descricao = "", Atendimento = "Online", TipoPreco = "Fixo",
    PrecoBase = 234m, Ativo = true, MediaAvaliacoes = 4.5, TotalAvaliacoes = 12 });
using var servicoJson = JsonDocument.Parse(JsonSerializer.Serialize(servico,
    new JsonSerializerOptions(JsonSerializerDefaults.Web)));
if (servicoJson.RootElement.GetProperty("mediaAvaliacoes").GetDouble() != 4.5 ||
    servicoJson.RootElement.GetProperty("totalAvaliacoes").GetInt32() != 12)
    throw new Exception("O catálogo não retornou média e quantidade reais de avaliações.");
Console.WriteLine("PASSOU: o catálogo inclui média e quantidade de avaliações.");
