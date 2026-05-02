namespace BD_TRAMPO
{
    public class Agendamento
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int UsuarioId { get; set; }
        public int ProfissionalId { get; set; }
        public int ServicoId { get; set; }
        public DateTime Data { get; set; }
        public TimeSpan Hora { get; set; }
        public string Status { get; set; } // Legado, antigo uso para metodo
        public string Descricao { get; set; }
        public string NomeCliente { get; set; }
        public string NomeProfissional { get; set; }
        public string Servico { get; set; }
        public string LinkOnline { get; set; }
        public string EnderecoCliente { get; set; }
        public string Atendimento { get; set; }
        public int? LocalId { get; set; }
        public string EnderecoLocal { get; set; } // só pra exibir
        public bool ConfirmadoProfissional { get; set; }
        public bool FinalizadoProfissional { get; set; }
        public bool ConfirmadoCliente { get; set; }
        public DateTime? DataFinalizacao { get; set; }
        public bool JaAvaliado {get; set; }
        public bool PodeAvaliar()
        {
            return FinalizadoProfissional && ConfirmadoCliente;
        }

        public string StatusAtual()
        {
            if (ConfirmadoProfissional || FinalizadoProfissional || ConfirmadoCliente)
            {
                if (!ConfirmadoProfissional)
                    return "Pendente";

                if (!FinalizadoProfissional)
                    return "Confirmado";

                if (!ConfirmadoCliente)
                    return "AguardandoCliente"; 

                return "Finalizado"; 
            }

            return Status ?? "Pendente";
        }


        public string StatusCalculado()
        {
            // CANCELADOS primeiro
            if (Status == "CanceladoCliente") return "CanceladoCliente";
            if (Status == "CanceladoProfissional") return "CanceladoProfissional";

            // FLUXO NORMAL
            if (!ConfirmadoProfissional)
                return "Pendente";

            if (!FinalizadoProfissional)
                return "Confirmado";

            if (!ConfirmadoCliente)
                return "AguardandoCliente";

            return "Finalizado";
        }

    }
}