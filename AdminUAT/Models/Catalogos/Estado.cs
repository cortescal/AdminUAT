using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Models.Catalogos
{
    public class Estado
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public DateTime? AltaSistema { get; set; }

        public ICollection<Municipio> Municipio { get; set; }
    }
}
