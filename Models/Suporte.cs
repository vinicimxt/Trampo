namespace BD_TRAMPO.Models
{
    public class Suporte
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Tipo { get; set; }
        public string Assunto { get; set; }
        public string Mensagem { get; set; }
        public string Status { get; set; }
        public DateTime DataCriacao { get; set; }
    }
}