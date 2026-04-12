namespace BD_TRAMPO
{
    public class Agendamento
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int ProfissionalId { get; set; }
        public int ServicoId { get; set; }
        public DateTime Data { get; set; }
        public TimeSpan Hora { get; set; }   
        public string Status { get; set; }   
        public string Descricao { get; set; }
        public string NomeCliente { get; set; }
        public string NomeProfissional { get; set; }
        public string Servico { get; set; }
        public string LinkOnline { get; set;}
        public string EnderecoCliente { get; set; }

    }
}