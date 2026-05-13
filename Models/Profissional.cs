namespace BD_TRAMPO
{
    public class Profissional
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public int UsuarioId { get; set; }
        public string Servico { get; set; } = "";
        public string Descricao { get; set; } = "";
        public string Atendimento { get; set; } = "";
        public int Raio { get; set; }
        public string? Contato { get; set; }
        public string? Email { get; set; }
        public string Telefone { get; set; }

        public string Plano { get; set; }

        public DateTime? DataAssinatura { get; set; }

        public string StatusAssinatura { get; set; }

        

    }

}
