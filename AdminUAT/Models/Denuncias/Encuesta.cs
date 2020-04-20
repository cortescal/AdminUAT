using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Models.Denuncias
{
    public class Encuesta
    {
        public long Id { get; set; }
        public bool Respuesta { get; set; }
        public string Comentario { get; set; }
        public long NumeroDenuncia { get; set; }
    }
}
