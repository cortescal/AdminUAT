using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Models.AdminUat
{
    public class Bitacora
    {
        public int EventoId { get; set; }
        public DateTime FechaEvento { get; set; }
        public string Usuario { get; set; }

        public string Nota { get; set; }

        public Evento Evento { get; set; }
    }
}
