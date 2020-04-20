using AdminUAT.Models.Catalogos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Models.MinisterioPublico
{
    public class MunicipioUR
    {
        public Guid MunicipioId { get; set; }
        public long URId { get; set; }

        public UR UR { get; set; }
        public Municipio Municipio { get; set; }
    }
}
