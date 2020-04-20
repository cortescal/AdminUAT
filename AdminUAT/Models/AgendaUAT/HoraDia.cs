using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Models.AgendaUAT
{
    public class HoraDia
    {
        public long Id { get; set; }
        public long MP { get; set; }
        public bool Activo { get; set; }
        public DateTime AltaSistema { get; set; }

        public int HoraId { get; set; }
        public int DiaId { get; set; }

        public Hora Hora { get; set; }
        public Dia Dia { get; set; }
        public ICollection<Cita> Cita { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFinal { get; set; }
    }
}
