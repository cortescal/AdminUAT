using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using AdminUAT.Models.Denuncias;
using AdminUAT.Models.MinisterioPublico;

namespace AdminUAT.Models.MinisterioPublico
{
    public class Fiscalia : BaseCatalog
    {
        public string Name { get; set; }

        public ICollection<Delito> Delitos { get; set; }
    }
}
