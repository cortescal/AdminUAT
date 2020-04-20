using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Models.AdminUat
{
    public class Token
    {
        public long Id { get; set; }
        public long Denuncia { get; set; }
        public string Codigo { get; set; }
        public DateTime AltaSistema { get; set; }
    }
}
