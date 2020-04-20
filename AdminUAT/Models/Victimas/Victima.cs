using AdminUAT.Models.Catalogos;
using AdminUAT.Models.Denuncias;
using System;
using System.Collections.Generic;

namespace AdminUAT.Models.Victimas
{
    public class Victima
    {
        public long Id { get; set; }
        public string Nombre { get; set; }
        public string PrimerApellido { get; set; }
        public string SegundoApellido { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string Email { get; set; }
        public bool EsVictima { get; set; }
        public bool Acepto { get; set; }
        public string TelFijo { get; set; }
        public string TelMovil { get; set; }
        public bool Abogado { get; set; }
        public string Cedula { get; set; }
        public string Despacho { get; set; }
        public string Nacionalidad { get; set; }
        public DateTime AltaSistema { get; set; }

        public int GeneroId { get; set; }
        public int EscolaridadId { get; set; }
        public long DenunciaId { get; set; }

        public Genero Genero { get; set; }
        public Escolaridad Escolaridad { get; set; }
        public Denuncia Denuncia { get; set; }
        public ICollection<DireccionVictima> DireccionVictima { get; set; }
    }
}
