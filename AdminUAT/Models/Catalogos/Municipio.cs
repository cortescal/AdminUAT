using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Models.Catalogos
{
    public class Municipio
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public DateTime? AltaSistema { get; set; }

        public Guid EstadoId { get; set; }

        public ICollection<Colonia> Colonia { get; set; }
        public Estado Estado { get; set; }
    }
}
