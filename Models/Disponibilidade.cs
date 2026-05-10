

namespace BD_TRAMPO
{


    public class Disponibilidade
    {
        public int Id { get; set; }
        public int ProfissionalId { get; set; }
        public int ServicoId { get; set; }
        public int DiaSemana { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFim { get; set; }
        public bool Ativo { get; set; }
    }

}