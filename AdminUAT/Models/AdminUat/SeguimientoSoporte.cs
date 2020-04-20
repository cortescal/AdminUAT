using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Models.AdminUat
{
    public class SeguimientoSoporte
    {
        public long OrdenSoporteId { get; set; } //FK-PK
        public string Usuario { get; set; } //PK
        public DateTime AltaSistema { get; set; } //PK
        public string Comentario { get; set; }
        public int Visto { get; set; }

        public OrdenSoporte OrdenSoporte { get; set; }
    }
}
