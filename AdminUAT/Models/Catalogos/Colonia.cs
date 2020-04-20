using AdminUAT.Models.Denuncias;
using AdminUAT.Models.Responsables;
using AdminUAT.Models.Victimas;
using System;
using System.Collections.Generic;

namespace AdminUAT.Models.Catalogos
{
    public class Colonia
    {
        public long Id { get; set; }
        public string Nombre { get; set; }
        public string CP { get; set; }
        public DateTime? AltaSistema { get; set; }

        public Guid MunicipioId { get; set; }

        public ICollection<DireccionDenuncia> DireccionDenuncia { get; set; }
        public ICollection<DireccionResponsable> DireccionResponsable { get; set; }
        public ICollection<DireccionVictima> DireccionVictima { get; set; }
        public Municipio Municipio { get; set; }
    }
}
