using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Models.ExtraModels.Agenda
{
    public class AuxHorario
    {
        public long CitaId { get; set; }
        public long IdDenuncia { get; set; }
        public string Folio { get; set; }
        public string Denunciante { get; set; }
        public TimeSpan Hora { get; set; }
    }
}