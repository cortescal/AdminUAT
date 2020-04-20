using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Models.AdminUat
{
    public class OrdenSoporte
    {
        public long Id { get; set; }
        public string Solicitud { get; set; }
        public bool Atendido { get; set; }
        public string AtendioUsuario { get; set; }
        public DateTime FechaAtencion { get; set; }
        public DateTime AltaSistema { get; set; }
        public int SolicitudCerrada { get; set; }
        public bool Activo { get; set; }
        public string Usuario { get; set; }
        public int TipoSoporteId { get; set; }

        public TipoSoporte TipoSoporte { get; set; }
        public ICollection<SeguimientoSoporte> SeguimientoSoporte { get; set; }
    }
}
