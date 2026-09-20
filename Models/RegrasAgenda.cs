namespace BD_TRAMPO;

// A agenda existente oferece atendimentos de uma hora.
// Um intervalo noturno pertence ao dia em que começa, inclusive sua parte após meia-noite.
public static class RegrasAgenda
{
    public static IEnumerable<DateTime> Horarios(IEnumerable<Disponibilidade> regras, DateTime dia)
    {
        foreach (var origem in new[] { dia.Date.AddDays(-1), dia.Date })
        foreach (var r in regras.Where(r => r.Ativo && r.DiaSemana == (int)origem.DayOfWeek))
        {
            if (r.HoraInicio == r.HoraFim) continue;
            var inicio = origem + r.HoraInicio;
            var fim = origem + r.HoraFim;
            if (fim < inicio) fim = fim.AddDays(1);
            for (var hora = inicio; hora.AddHours(1) <= fim; hora = hora.AddHours(1))
                if (hora.Date == dia.Date) yield return hora;
        }
    }

    public static bool Sobrepoe(DateTime inicio, DateTime fim, DateTime outroInicio, DateTime outroFim)
        => inicio < outroFim && outroInicio < fim;
}