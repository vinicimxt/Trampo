namespace BD_TRAMPO
{

    public class Servico
    {
        public int Id { get; set; }
        public int ProfissionalId { get; set; }
        public int ServicoId { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public string Atendimento { get; set; }
        public int? RaioAtendimento { get; set; }
        public string Contato { get; set; }

        public string Categoria { get; set; }
        public string Subcategoria { get; set; }
        public int SubcategoriaId { get; set; }
        // 🔥 NOVO CAMPO
        public string NomeProfissional { get; set; }
    }

}