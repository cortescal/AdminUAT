using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Models.ExtraModels.UAT
{
    public class AuxEncuesta
    {
        public Guid IdMunicipio { get; set; }
        public string Municipio { get; set; }
        public bool Respuesta { get; set; }
        public long TotalSi { get; set; }
        public long TotalNo { get; set; }
    }
}
