using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Models.ExtraModels.Admin
{
    public class EstadisticaObjetivos
    {
        public int EncuestaSi { get; set; }
        public int EncustaNo { get; set; }
        public int EncuestaTotal { get; set; }
        public string PorcientoSi { get; set; }
        public string PorcientoNo { get; set; }
        public int DenunciasAsignadas { get; set; }
        public int DenunciasSinAsignar { get; set; }
        public int DenunciasTotal { get; set; }
        public string PorcientoAsignadas { get; set; }
        public string PorcientoSinAsignar { get; set; }
    }
}
