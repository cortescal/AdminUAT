using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Models.AgendaUAT
{
    public class Hora
    {
        public int Id { get; set; }
        public TimeSpan CampoHora { get; set; }
        public bool Activo { get; set; }

        public ICollection<HoraDia> HoraDia { get; set; }
    }
}
