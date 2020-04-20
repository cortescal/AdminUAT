using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Models.MinisterioPublico
{
    public class Region
    {
        public long Id { get; set; }
        public string Nombre { get; set; }

        ICollection<UR> UR { get; set; }
    }
}
