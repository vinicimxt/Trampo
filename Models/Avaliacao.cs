namespace BD_TRAMPO
{
    public class Avaliacao
    {
        public int Id { get; set; }
        public int ProfissionalId { get; set; }

        public int UsuarioId { get; set; }

        public int AgendamentoId { get; set; }
        public int Nota { get; set; }
        public string Comentario { get; set; }
        public DateTime Data { get; set; }

        public string NomeUsuario { get; set; }
    }
}