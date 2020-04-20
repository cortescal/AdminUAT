using AdminUAT.Models.AdminUat;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Nombre { get; set; }
        public string PrimerApellido { get; set; }
        public string SegundoApellido { get; set; }
        public bool Estatus { get; set; }
        public int Rol { get; set; }
        public long MatchMP { get; set; }
        public DateTime AltaSistema { get; set; }
    }
}
