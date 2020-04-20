using AdminUAT.Models.Denuncias;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Dependencias
{
    public interface IQueryDenuncias
    {
        List<Denuncia> DSS(long mpId);
        List<Denuncia> DCS(long mpId);
        List<Denuncia> DenunciaPorId(long id);
        List<Denuncia> PorFecha(DateTime fecha, long mpId);

        List<Denuncia> AEITodas(DateTime fecha, bool opc);
        List<Denuncia> PorPalabraYFecha(string palabra, DateTime fecha);
        List<Denuncia> PorKioscoYFecha(long id, DateTime fecha);
        List<Denuncia> PorKiosco(long id);
        List<Denuncia> PorPalabra(string palabra);
    }
}
