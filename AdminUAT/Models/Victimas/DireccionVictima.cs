using AdminUAT.Models.Catalogos;
using System;

namespace AdminUAT.Models.Victimas
{
    public class DireccionVictima
    {
        public long Id { get; set; }
        public string Calle { get; set; }
        public string NumExterior { get; set; }
        public string NumInterior { get; set; }
        public DateTime AltaSistema { get; set; }

        public long VictimaId { get; set; }
        public long ColoniaId { get; set; }

        public Colonia Colonia { get; set; }
        public Victima Victima { get; set; }
    }
}
