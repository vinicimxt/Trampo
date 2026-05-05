namespace BD_TRAMPO
{

    public class Notificacao
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }

        public string Titulo { get; set; }
        public string Mensagem { get; set; }
        public string Tipo { get; set; }

        public int? ReferenciaId { get; set; } 

        public bool Lida { get; set; }
        public DateTime DataCriacao { get; set; }
    }

}