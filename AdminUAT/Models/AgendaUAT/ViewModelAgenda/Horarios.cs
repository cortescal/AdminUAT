using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Models.AgendaUAT.ViewModelAgenda
{
    public class Horarios
    {
        public List<Horario> horario{ get; set; }
        public long? idMP { get; set; }
        public ModelHorario ModelHorario { get; set; }

        /*public List<ModelDias> dias { get; set; }
        public List<ModelHoras> horas { get; set; }*/
    }
}
