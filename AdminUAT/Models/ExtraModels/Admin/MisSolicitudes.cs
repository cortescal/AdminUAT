using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Models.ExtraModels.Admin
{
    public class MisSolicitudes
    {
        public long Id { get; set; }
        public string Solicitud { get; set; }
        public DateTime AltaSistema { get; set; }
        public bool Atendido { get; set; }
        public int NumNotificaciones { get; set; }
        public string Usuario { get; set; }
    }
}
