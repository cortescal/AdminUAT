using AdminUAT.Models.Denuncias;
using System;
using System.Collections.Generic;

namespace AdminUAT.Models.MinisterioPublico
{
    public class Solucion
    {
        public long Id { get; set; }
        public string Nombre { get; set; }
        public DateTime? AltaSistema { get; set; }

        public ICollection<Denuncia> Denuncia { get; set; }
    }
}
