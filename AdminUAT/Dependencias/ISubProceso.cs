using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Dependencias
{
    public interface ISubProceso
    {
        int AsignaToken(long idDenuncia);
        void ActualizaToken(long idDenuncia, string codigo);
        bool MatchDenunciaMP(long mpId, long denunciaId);
        bool ExpiroCita(long denunciaId);
        string GetPorciento(double numero, double total);
        bool ValidaUsuarioSoporte(string usuario, long soporteId);
        bool AccesoDenunciaFM(long denunciaId);
        bool AccesoDenunciaFR(long denunciaId);
        long IdRegionDenuncia(long denunciaId);
    }
}
