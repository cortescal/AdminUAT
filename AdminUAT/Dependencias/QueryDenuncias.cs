using AdminUAT.Data;
using AdminUAT.Models.Denuncias;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Dependencias
{
    public class QueryDenuncias : IQueryDenuncias
    {
        private readonly NewUatDbContext _uatContext;

        public QueryDenuncias(NewUatDbContext uatContext)
        {
            _uatContext = uatContext;
        }

        public List<Denuncia> DSS(long mpId) //denuncias sin solución
        {
            var denuncias = _uatContext.Denuncia
                .Include(x => x.BitaKiosco)
                .Include(x => x.Delito)
                .Where(x => x.Paso == 3)
                .OrderBy(x => x.Id)
                .Where(x => x.MPId == mpId && x.SolucionId == null)
                .ToList();

            return denuncias;
        }

        public List<Denuncia> DCS(long mpId) //denuncias con solución
        {
            var denuncias = _uatContext.Denuncia
                .Include(x => x.BitaKiosco)
                .Include(x => x.Delito)
                .Where(x => x.Paso == 3)
                .OrderBy(x => x.FechaSolucion)
                .Where(x => x.MPId == mpId && x.SolucionId != null)
                .Take(20)
                .ToList();

            return denuncias;
        }

        public List<Denuncia> DenunciaPorId(long id)
        {
            var denuncias = _uatContext.Denuncia
                .Include(x => x.BitaKiosco)
                .Include(x => x.Delito)
                .Where(x => x.Paso == 3)
                .OrderBy(x => x.Id)
                .Where(x => x.Id == id)
                .ToList();

            return denuncias;
        }

        public List<Denuncia> PorFecha(DateTime fecha, long mpId)
        {
            var denuncias = _uatContext.Denuncia
                .Include(x => x.BitaKiosco)
                .Include(x => x.Delito)
                .Include(x => x.MP)
                    .ThenInclude(x => x.UR)
                .Where(x => x.MPId == mpId && x.Paso == 3 && x.AltaSistema.Date == fecha.Date)
                .OrderBy(x => x.Id)
                .ToList();

            return denuncias;
        }

        public List<Denuncia> AEITodas(DateTime fecha, bool opc)
        {
            var lista = new List<Denuncia>();

            if (opc)
            {
                lista = _uatContext.Denuncia
                    .Include(x => x.BitaKiosco)
                    .Include(x => x.Delito)
                    .Include(x => x.MP)
                        .ThenInclude(x => x.UR)
                    .Where(x => x.Paso == 3 && x.AltaSistema.Date == fecha.Date)
                    .OrderBy(x => x.Id)
                    .ToList();
            }
            else
            {
                lista = _uatContext.Denuncia
                    .Include(x => x.BitaKiosco)
                    .Where(x => x.Paso != 3 && x.AltaSistema.Date == fecha.Date)
                    .OrderBy(x => x.Id)
                    .ToList();
            }

            return lista;
        }

        public List<Denuncia> PorPalabraYFecha(string palabra, DateTime fecha)
        {
            var lista = _uatContext.Denuncia
                .Include(x => x.BitaKiosco)
                .Include(x => x.Delito)
                .Include(x => x.MP)
                    .ThenInclude(x => x.UR)
                .Where(x => x.Paso == 3 && x.Relato.Contains(palabra) && x.AltaSistema.Date == fecha.Date)
                .OrderBy(x => x.Id)
                .ToList();

            return lista;
        }

        public List<Denuncia> PorKioscoYFecha(long id, DateTime fecha)
        {
            var lista = _uatContext.Denuncia
                .Include(x => x.BitaKiosco)
                .Include(x => x.Delito)
                .Include(x => x.MP)
                    .ThenInclude(x => x.UR)
                .Where(x => x.Paso == 3 && x.BitaKiosco.Id == id && x.AltaSistema.Date == fecha.Date)
                .OrderBy(x => x.Id)
                .ToList();

            return lista;
        }

        public List<Denuncia> PorKiosco(long id)
        {
            var lista = _uatContext.Denuncia
                .Include(x => x.BitaKiosco)
                .Include(x => x.Delito)
                .Include(x => x.MP)
                    .ThenInclude(x => x.UR)
                .Where(x => x.Paso == 3 && x.BitaKiosco.Id == id)
                .OrderBy(x => x.Id)
                .ToList();

            return lista;
        }

        public List<Denuncia> PorPalabra(string palabra)
        {
            var lista = _uatContext.Denuncia
                .Include(x => x.BitaKiosco)
                .Include(x => x.Delito)
                .Include(x => x.MP)
                    .ThenInclude(x => x.UR)
                .Where(x => x.Paso == 3 && x.Relato.Contains(palabra))
                .OrderBy(x => x.Id)
                .ToList();

            return lista;
        }
    }
}
