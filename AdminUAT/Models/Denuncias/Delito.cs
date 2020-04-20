using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Models.Denuncias
{
    public class Delito
    {
        public long Id { get; set; }
        public string Tipo { get; set; }
        public string Descripcion { get; set; }
        public DateTime? AltaSistema { get; set; }

        public ICollection<Denuncia> Denuncia { get; set; }
    }
}
