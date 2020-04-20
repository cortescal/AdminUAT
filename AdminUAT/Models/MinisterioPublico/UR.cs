using AdminUAT.Models.Catalogos;
using System;
using System.Collections.Generic;

namespace AdminUAT.Models.MinisterioPublico
{
    public class UR
    {
        public long Id { get; set; }
        public string Nombre { get; set; }
        public string Nota { get; set; }
        public DateTime? AltaSistema { get; set; }

        public long RegionId { get; set; }

        public ICollection<MP> MP { get; set; }
        public ICollection<BitaKiosco> BitaKiosco { get; set; }
        public ICollection<MunicipioUR> MunicipioUR { get; set; }
        public Region Region { get; set; }
    }
}
