using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Models.ExtraModels.Agenda
{
    public class AuxCita
    {
        public DateTime Fecha { get; set; }
        public string Dia { get; set; }
        public ICollection<AuxHorario> AuxHorario { get; set; }
    }
}
