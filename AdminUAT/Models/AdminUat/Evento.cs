using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Models.AdminUat
{
    public class Evento
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public DateTime? AltaSistema { get; set; }

        public ICollection<Bitacora> Bitacora { get; set; }
    }
}
