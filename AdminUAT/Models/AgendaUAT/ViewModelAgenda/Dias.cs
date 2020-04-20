using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Models.AgendaUAT.ViewModelAgenda
{
    public class Dias
    {
        public int idDia { get; set; }
        public List<Horas> horas { get; set; }
        public string Dia { get; set; }
        public bool Activo { get; set; }
    }

    public class Horas
    {
        public int idHora { get; set; }
        public TimeSpan Hora { get; set; }
    }
}
