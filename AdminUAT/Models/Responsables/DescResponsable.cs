using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Models.Responsables
{
    public class DescResponsable
    {
        public long Id { get; set; }
        public string Barba { get; set; }
        public string ColorPiel { get; set; }
        public double? Altura { get; set; }
        public string TipoCabello { get; set; }
        public string ColorCabello { get; set; }
        public string ColorOjos { get; set; }
        public string Complexion { get; set; }
        public bool? Tatuajes { get; set; }
        public DateTime AltaSistema { get; set; }

        public long ResponsableId { get; set; }

        public Responsable Responsable { get; set; }
    }
}
