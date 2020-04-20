using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Models.AgendaUAT.ViewModelAgenda
{
    public class ModelHorario
    {
        public List<DiaHorario> dias { get; set; }
        public List<HoraHorario> horas { get; set; }
    }

    public class DiaHorario {
        public int IdDias { get; set; }
        public string nombreDias { get; set; }
    }

    public class HoraHorario
    {
        public int IdHoras { get; set; }
        public TimeSpan campoHora { get; set; }
    }
}
