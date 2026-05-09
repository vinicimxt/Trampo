namespace BD_TRAMPO.Models.ViewModels
{
    public class PerfilViewModel
    {
        public int UsuarioId { get; set; }

        public string Nome { get; set; }

        public string Email { get; set; }

        public string Telefone { get; set; }

        public string Tipo { get; set; }

        // somente profissional
        public string ContatoPublico { get; set; }
    }
}