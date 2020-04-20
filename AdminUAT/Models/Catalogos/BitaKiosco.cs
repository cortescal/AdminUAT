using AdminUAT.Models.Denuncias;
using AdminUAT.Models.MinisterioPublico;
using System;
using System.Collections.Generic;

namespace AdminUAT.Models.Catalogos
{
    public class BitaKiosco
    {
        public long Id { get; set; }
        public string Ip { get; set; }
        public string Nombre { get; set; }
        public string Ubicacion { get; set; }
        public bool Activo { get; set; }
        public DateTime? AltaSistema { get; set; }

        public long URId { get; set; }

        public UR UR { get; set; }
        public ICollection<Denuncia> Denuncia { get; set; }
    }
}
