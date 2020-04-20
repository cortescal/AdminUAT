using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Models.AdminUat
{
    public class TipoSoporte
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Nota { get; set; }
        public bool Activo { get; set; }

        public ICollection<OrdenSoporte> OrdenSoporte { get; set; }
    }
}
