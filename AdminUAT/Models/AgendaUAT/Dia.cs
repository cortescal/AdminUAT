using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Models.AgendaUAT
{
    public class Dia
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int Numero { get; set; }
        public bool Activo { get; set; }

        public ICollection<HoraDia> HoraDia { get; set; }
    }
}
