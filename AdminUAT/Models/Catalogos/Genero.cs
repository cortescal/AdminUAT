using System;
using System.Collections.Generic;
using AdminUAT.Models.Responsables;
using AdminUAT.Models.Victimas;

namespace AdminUAT.Models.Catalogos
{
    public class Genero
    {
        public int Id { get; set; }
        public string Sexo { get; set; }
        public DateTime? AltaSistema { get; set; }

        public ICollection<Responsable> Responsable { get; set; }
        public ICollection<Victima> Victima { get; set; }
    }
}
