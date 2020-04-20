using AdminUAT.Models.Catalogos;
using System;

namespace AdminUAT.Models.Denuncias
{
    public class DireccionDenuncia
    {
        public long Id { get; set; }
        public string Calle { get; set; }
        public string NumExterior { get; set; }
        public string NumInterior { get; set; }
        public double? Longitud { get; set; }
        public double? Latitud { get; set; }
        public DateTime AltaSistema { get; set; }

        public long DenunciaId { get; set; }
        public long ColoniaId { get; set; }

        public Denuncia Denuncia { get; set; }
        public Colonia Colonia { get; set; }
    }
}
