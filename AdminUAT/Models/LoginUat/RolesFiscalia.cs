using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Models.LoginUat
{
    public class RolFiscalia
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public Guid? FiscaliaId { get; set; }
    }
}
