using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminUAT.Data;
using AdminUAT.Models;
using AdminUAT.Models.Denuncias;
using AdminUAT.Models.ExtraModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminUAT.Controllers
{
    [Authorize(Roles = "FiscReg, Root")]
    public class FRController : Controller
    {
        private readonly NewUatDbContext _contextUAT;
        private UserManager<ApplicationUser> _userManager;

        public FRController(NewUatDbContext contextUAT, UserManager<ApplicationUser> userManager)
        {
            _contextUAT = contextUAT;
            _userManager = userManager;
        }

        public async Task<IActionResult> Chart(string fecha, string fecha2)
        {
            var user = await _userManager.GetUserAsync(User);
            ViewData["rol"] = user.Rol;
            ViewData["fecha"] = fecha;
            ViewData["fecha2"] = fecha2;
            return View();
        }

        //Desglose por origen
        [HttpGet("FR/JsonData")]
        public async Task<IEnumerable<MapaData>> JsonData(string fecha, string fecha2)
        {
            List<MapaData> json = new List<MapaData>();
            var fiscalia = await _contextUAT.Fiscalias.AsNoTracking()
                .Where(x => x.Value == "FI").Select(x => x.Id).FirstOrDefaultAsync();

            if (fecha2 != "" && fecha2 != null)
            {
                json = await JsonData2(fecha, fecha2,fiscalia);
                return json.OrderByDescending(x => x.Recibidas);
            }

            fecha = fecha == null ? DateTime.Now.ToString("yyyy-MM-dd") : fecha;

            var kiosco = await _contextUAT.BitaKiosco.AsNoTracking()
                .Include(x => x.UR)
                .Where(x => x.UR.RegionId != 6 && x.UR.RegionId != 1)
                .ToListAsync();

            var denuncia = await _contextUAT.Denuncia.AsNoTracking()
                .Include(x => x.MP)
                    .ThenInclude(x => x.UR)
                    .Where(x => x.AltaSistema.ToString("yyyy-MM-dd") == fecha && x.Paso == 3 && x.FiscaliaId == fiscalia)
                    .Select(x => new
                    {
                        Kiosko = x.BitaKioscoId,
                        Solucion = x.SolucionId,
                        Fiscalia = x.FiscaliaId,
                        Region = x.MP.UR.RegionId
                    })
                    .ToListAsync();

            foreach (var item in kiosco)
            {
                var recibidas = denuncia.Where(x => x.Kiosko == item.Id).Count();
                var atendidas = denuncia.Where(x => x.Kiosko == item.Id && x.Solucion != null && x.Fiscalia == fiscalia).Count();

                if (recibidas > 0 || atendidas > 0)
                {
                    var obj = new MapaData
                    {
                        Kiosco = item.Nombre,
                        Fecha = fecha,
                        Recibidas = recibidas,
                        Atendidas = atendidas
                    };

                    json.Add(obj);
                }
            }

            //Pagina oficial
            var dpoR = denuncia.Where(x => x.Kiosko == 1 && (x.Region != 6 && x.Region != 1) && x.Fiscalia == fiscalia).Count();

            var dpoA = denuncia.Where(x => x.Kiosko == 1  && x.Solucion != null && (x.Region != 6 && x.Region != 1) && x.Fiscalia == fiscalia).Count();

            if(dpoA>0||dpoR>0)
            {
                var obj1 = new MapaData
                {
                    Kiosco = "Pagina Oficial",
                    Fecha = fecha,
                    Recibidas = dpoR,
                    Atendidas = dpoA
                };

                json.Add(obj1);
            }

            return json.OrderByDescending(x => x.Recibidas);
        }

        private async Task<List<MapaData>> JsonData2(string fecha, string fecha2, Guid fiscalia)
        {
            List<MapaData> json = new List<MapaData>();
            DateTime fechaI = Convert.ToDateTime(fecha);
            DateTime fechaF = Convert.ToDateTime(fecha2);

            var kiosco = await _contextUAT.BitaKiosco.AsNoTracking()
                .Include(x => x.UR)
                .Where(x => x.UR.RegionId != 6 && x.UR.RegionId != 1)
                .ToListAsync();

            var denuncia = await _contextUAT.Denuncia.AsNoTracking()
                .Include(x => x.MP)
                    .ThenInclude(x => x.UR)
                    .Where(x => (x.AltaSistema.Date >= fechaI.Date && x.AltaSistema.Date <= fechaF.Date) && x.Paso == 3 && 
                        x.FiscaliaId == fiscalia)
                    .Select(x => new
                    {
                        Kiosko = x.BitaKioscoId,
                        Solucion = x.SolucionId,
                        Fiscalia = x.FiscaliaId,
                        Region = x.MP.UR.RegionId
                    })
                    .ToListAsync();

            foreach (var item in kiosco)
            {
                var a = fechaI.DayOfWeek;

                var recibidas = denuncia.Where(x => x.Kiosko == item.Id).Count();
                var atendidas = denuncia.Where(x => x.Kiosko == item.Id && x.Solucion != null && x.Fiscalia == fiscalia).Count();

                if (recibidas > 0 || atendidas > 0)
                {
                    var obj = new MapaData
                    {
                        Kiosco = item.Nombre,
                        Fecha = fecha,
                        Recibidas = recibidas,
                        Atendidas = atendidas
                    };

                    json.Add(obj);
                }
            }

            //Pagina oficial
            var dpoR = denuncia.Where(x => x.Kiosko == 1 && (x.Region != 6 && x.Region != 1) && x.Fiscalia == fiscalia).Count();

            var dpoA = denuncia.Where(x => x.Kiosko == 1 && x.Solucion != null && (x.Region != 6 && x.Region != 1) && x.Fiscalia == fiscalia).Count();

            if (dpoR > 0 || dpoA > 0)
            {
                var obj1 = new MapaData
                {
                    Kiosco = "Pagina Oficial",
                    Fecha = fecha,
                    Recibidas = dpoR,
                    Atendidas = dpoA
                };

                json.Add(obj1);
            }

            return json;
        }
    }
}