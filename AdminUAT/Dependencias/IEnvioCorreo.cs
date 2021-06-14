using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Dependencias
{
    public interface IEnvioCorreo
    {
        bool SendCorreo(string titulo, int paso, long idDenuncia, string path);
        string GeneraCodigo(long idDenuncia);
        Task SendEmailTokenAsync();
    }
}
