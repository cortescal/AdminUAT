using AdminUAT.Models.Catalogos;
using System;

namespace AdminUAT.Models.Responsables
{
    public class DireccionResponsable
    {
        public long Id { get; set; }
        public string Calle { get; set; }
        public string NumExterior { get; set; }
        public string NumInterior { get; set; }
        public DateTime AltaSistema { get; set; }

        public long ResponsableId { get; set; }
        public long? ColoniaId { get; set; }

        public Responsable Responsable { get; set; }
        public Colonia Colonia { get; set; }
    }
}
