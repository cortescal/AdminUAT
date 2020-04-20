using AdminUAT.Models.Catalogos;
using AdminUAT.Models.Denuncias;
using System;
using System.Collections.Generic;

namespace AdminUAT.Models.Responsables
{
    public class Responsable
    {
        public long Id { get; set; }
        public string Nombre { get; set; }
        public string PrimerApellido { get; set; }
        public string SegundoApellido { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string Alias { get; set; }
        public string Nacionalidad { get; set; }
        public DateTime AltaSistema { get; set; }

        public int GeneroId { get; set; }
        public long DenunciaId { get; set; }

        public Denuncia Denuncia { get; set; }
        public Genero Genero { get; set; }
        public ICollection<DireccionResponsable> DireccionResponsable { get; set; }
        public ICollection<DescResponsable> DescResponsable { get; set; }
    }
}
