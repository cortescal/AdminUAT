using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Models.Victimas
{
    public class Escolaridad
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }
        public DateTime? AltaSistema { get; set; }

        public ICollection<Victima> Victima { get; set; }
    }
}
