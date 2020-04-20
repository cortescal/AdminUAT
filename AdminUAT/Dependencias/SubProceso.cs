using AdminUAT.Data;
using AdminUAT.Models;
using AdminUAT.Models.AdminUat;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace AdminUAT.Dependencias
{
    public class SubProceso : ISubProceso
    {
        private readonly ApplicationDbContext _context;
        private readonly NewUatDbContext _uatContext;
        private readonly AgendaDbContext _agendaContext;

        public SubProceso(ApplicationDbContext context, NewUatDbContext uatContext,
            AgendaDbContext agendaContext)
        {
            _context = context;
            _uatContext = uatContext;
            _agendaContext = agendaContext;
        }

        public int AsignaToken(long idDenuncia)
        {
            var guid = Guid.NewGuid();
            var justNumbers = new String(guid.ToString().Where(Char.IsDigit).ToArray());
            var seed = int.Parse(justNumbers.Substring(0, 4));

            var random = new Random(seed);
            var value = random.Next(1754, 9876);

            ActualizaToken(idDenuncia, value.ToString());

            return value;
        }

        public void ActualizaToken(long idDenuncia, string codigo)
        {
            var objToken = _context.Token.Where(x => x.Denuncia == idDenuncia).FirstOrDefault();

            if (objToken == null)
            {
                var token = new Token()
                {
                    Id = 0,
                    Denuncia = idDenuncia,
                    Codigo = codigo,
                    AltaSistema = DateTime.Now
                };
                _context.Add(token);
                _context.SaveChanges();
            }
            else
            {
                objToken.Codigo = codigo;
                objToken.AltaSistema = DateTime.Now;
                _context.Update(objToken);
                _context.SaveChanges();
            }
        }

        public bool MatchDenunciaMP(long mpId, long denunciaId)
        {
            var denuncia = _uatContext.Denuncia.Find(denunciaId);

            if (denuncia == null)
            {
                return false;
            }

            return denuncia.MPId == mpId? true: false;
        }

        public bool ExpiroCita(long denunciaId)
        {
            var cita = _agendaContext.Cita
                .Include(x => x.HoraDia)
                    .ThenInclude(x => x.Hora)
                .Where(x => x.NumDenuncia == denunciaId && x.Activo == true)
                .FirstOrDefault();

            if (cita == null)
            {
                return false;
            }

            if (Convert.ToDateTime(cita.Dia.Date + cita.HoraDia.Hora.CampoHora) > DateTime.Now)
            {
                return false;
            }

            return true;
        }

        public string GetPorciento(double numero, double total)
        {
            if (numero == 0)
            {
                return "0%";
            }

            double porciento = numero / total * 100.0;

            return porciento - Math.Floor(porciento) == 0 ? string.Format("{0:n0}%", porciento) : string.Format("{0:n2}%", porciento);
        }

        public bool ValidaUsuarioSoporte(string usuario, long soporteId)
        {
            var soporte = _context.OrdenSoporte
                .Where(x => x.Id == soporteId && x.Usuario == usuario  && x.Activo == true && (x.SolicitudCerrada == -1 || x.SolicitudCerrada == 1))
                .FirstOrDefault();

            if (soporte == null)
            {
                return false;
            }

            return true;
        }

        public bool AccesoDenunciaFM(long denunciaId)
        {         
            if (IdRegionDenuncia(denunciaId) == 6)
            {
                return true;
            }

            return false;
        }

        public bool AccesoDenunciaFR(long denunciaId)
        {
            if (IdRegionDenuncia(denunciaId) != 6)
            {
                return true;
            }

            return false;
        }

        public long IdRegionDenuncia(long denunciaId)
        {
            var denuncia = _uatContext.Denuncia
                .Include(x => x.MP)
                    .ThenInclude(x => x.UR)
                .Where(x => x.Id == denunciaId && x.Paso == 3)
                .FirstOrDefault();

            if (denuncia == null)
            {
                return -1;
            }

            return denuncia.MP.UR.RegionId;
        }
    }
}
