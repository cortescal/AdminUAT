using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Models.AgendaUAT
{
    public class Cita
    {
        public long Id { get; set; }
        public DateTime Dia { get; set; }
        public int Asistencia { get; set; }
        public long NumDenuncia { get; set; }
        public bool Activo { get; set; }
        public string Comentario { get; set; }
        public long MP { get; set; }
        public bool Notificado { get; set; }
        public int SendEmail { get; set; }
        public DateTime AltaSistema { get; set; }
        public long HoraDiaId { get; set; }

        public HoraDia HoraDia { get; set; }
    }
}
