using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Models.AdminUat
{
    public class Noticia
    {
        public long Id { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public bool Activo { get; set; }
        public DateTime AltaSistema { get; set; }
    }
}
