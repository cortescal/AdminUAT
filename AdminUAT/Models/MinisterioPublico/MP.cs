using AdminUAT.Models.Denuncias;
using System;
using System.Collections.Generic;

namespace AdminUAT.Models.MinisterioPublico
{
    public class MP
    {
        public long Id { get; set; }
        public string Nombre { get; set; }
        public string PrimerApellido { get; set; }
        public string SegundoApellido { get; set; }
        public bool Activo { get; set; }
        public long Stock { get; set; }
        public long Resuelto { get; set; }
        public DateTime? AltaSistema { get; set; }

        public long URId { get; set; }

        public UR UR { get; set; }
        public ICollection<Denuncia> Denuncia { get; set; }
    }
}
