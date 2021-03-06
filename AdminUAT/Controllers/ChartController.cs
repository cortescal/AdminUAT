using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminUAT.Data;
using AdminUAT.Models;
using AdminUAT.Models.ExtraModels;
using AdminUAT.Models.MinisterioPublico;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminUAT.Controllers
{
    [Authorize(Roles = "AEI, Root, FiscMet, FiscReg")]
    [Route("[controller]")]
    [ApiController]
    public class ChartController : Controller
    {
        private readonly NewUatDbContext _context;
        private UserManager<ApplicationUser> _userManager;

        public ChartController(NewUatDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "AEI, Root")]
        public async Task<IActionResult> Index(string fecha, string fecha2)
        {
            var user = await _userManager.GetUserAsync(User);
            ViewData["fecha"] = fecha;
            ViewData["fecha2"] = fecha2;

            return View();
        }

        //Desglose por origen
        [Authorize(Roles = "AEI, Root")]
        [HttpGet("JsonData")]
        public async Task<IEnumerable<MapaData>> JsonData(string fecha, string fecha2)
        {
            List<MapaData> json = new List<MapaData>();
            if (fecha2 != "" && fecha2 != null)
            {
                json = await JsonData2(fecha, fecha2);
                return json.OrderByDescending(x => x.Recibidas);
            }

            fecha = fecha == null ? DateTime.Now.ToString("yyyy-MM-dd") : fecha;

            var kiosco = await _context.BitaKiosco
                .OrderBy(x => x.Id)
                .ToListAsync();

            foreach (var item in kiosco)    
            {
                var recibidas = await _context.Denuncia
                    .Where(x => x.AltaSistema.ToString("yyyy-MM-dd") == fecha && x.BitaKioscoId == item.Id && x.Paso == 3)
                    .CountAsync();

                var atendidas = await _context.Denuncia
                    .Where(x => x.AltaSistema.ToString("yyyy-MM-dd") == fecha && x.BitaKioscoId == item.Id && x.SolucionId != null)
                    .CountAsync();

                var obj = new MapaData
                {
                    Kiosco = item.Nombre,

                    Fecha = fecha,
                    Recibidas = recibidas,
                    Atendidas = atendidas
                };

                json.Add(obj);
            }

            return json.OrderByDescending(x => x.Recibidas);
        }

        [Authorize(Roles = "AEI, Root")]
        private async Task<List<MapaData>> JsonData2(string fecha, string fecha2)
        {
            List<MapaData> json = new List<MapaData>();
            DateTime fechaI = Convert.ToDateTime(fecha);
            DateTime fechaF = Convert.ToDateTime(fecha2);

            var kiosco = await _context.BitaKiosco
                .OrderBy(x => x.Id)
                .ToListAsync();

            foreach (var item in kiosco)
            {
                var a = fechaI.DayOfWeek;
               
                var recibidas = await _context.Denuncia
                    .Where(x => (x.AltaSistema.Date >= fechaI.Date && x.AltaSistema.Date <= fechaF.Date) && x.BitaKioscoId == item.Id && x.Paso == 3)
                    .CountAsync();

                var atendidas = await _context.Denuncia
                    .Where(x => (x.AltaSistema.Date >= fechaI.Date && x.AltaSistema.Date <= fechaF.Date) && x.BitaKioscoId == item.Id && x.SolucionId != null)
                    .CountAsync();

                var obj = new MapaData
                {
                    Kiosco = item.Nombre,

                    Fecha = fecha,
                    Recibidas = recibidas,
                    Atendidas = atendidas
                };

                json.Add(obj);
            }

            return json;
        }

        //Denuncias por atender por AMP Metropolitana
        [Authorize(Roles = "AEI, Root, FiscMet")]
        [HttpGet("ChartMP")]
        public async Task<IEnumerable<MP>> ChartMP()
        {
            var json = await _context.MP
                .Where(x => x.Activo == true && x.UR.RegionId == 6)
                .OrderByDescending(x => (x.Stock - x.Resuelto))
                .ToListAsync();

            return json;
        }

        //Denuncias por atender por AMP Regional
        [Authorize(Roles = "AEI, Root, FiscReg")]
        [HttpGet("RegionalChartMP")]
        public async Task<IEnumerable<MP>> RegionalChartMP()
        {
            var json = await _context.MP
                .Where(x => x.Activo == true && x.UR.RegionId != 6)
                .OrderByDescending(x => (x.Stock - x.Resuelto))
                .ToListAsync();

            return json;
        }

        [Authorize(Roles = "AEI, Root, FiscMet, FiscReg")]
        [HttpGet("Regional")]
        public async Task<IActionResult> Regional(string fecha, string fecha2)
        {
            if (fecha2 != "" && fecha2 != null)
            {
                DateTime fechaI = Convert.ToDateTime(fecha);
                DateTime fechaF = Convert.ToDateTime(fecha2);

                var denuncias1 = await _context.Denuncia
                    .Include(x => x.MP)
                        .ThenInclude(x => x.UR)
                    .Where(x => (x.AltaSistema.Date >= fechaI.Date && x.AltaSistema.Date <= fechaF.Date) && x.Paso == 3 && x.MPId != null)
                    .ToListAsync();

                var regional1 = denuncias1.Where(x => x.MP.UR.RegionId != 6).Count();
                var metro1 = denuncias1.Where(x => x.MP.UR.RegionId == 6).Count();
                var aux1 = denuncias1.Where(x => x.SolucionId != null).ToList();
                var regSolucion1 = aux1.Where(x => x.MP.UR.RegionId != 6).Count();
                var metroSolucion1 = aux1.Where(x => x.MP.UR.RegionId == 6).Count();

                var cdi1 = denuncias1.Where(x => x.SolucionId == 1).Count();
                var constancia1 = denuncias1.Where(x => x.SolucionId == 2).Count();
                var archivo1 = denuncias1.Where(x => x.SolucionId == 3).Count();

                return Ok(new { regional = regional1, metro = metro1, regSolucion = regSolucion1, metroSolucion = metroSolucion1, cdi = cdi1, constancia = constancia1, archivo = archivo1 });
            }

            fecha = fecha == null ? DateTime.Now.ToString("yyyy-MM-dd") : fecha;

            var denuncias = await _context.Denuncia
                .Include(x => x.MP)
                    .ThenInclude(x => x.UR)
                 .Where(x => x.AltaSistema.ToString("yyyy-MM-dd") == fecha && x.Paso == 3)
                .ToListAsync();

            var regional = denuncias.Where(x => x.MP.UR.RegionId != 6).Count();
            var metro = denuncias.Where(x => x.MP.UR.RegionId == 6).Count();

            var aux = denuncias.Where(x => x.SolucionId != null).ToList();
            var regSolucion = aux.Where(x => x.MP.UR.RegionId != 6).Count();
            var metroSolucion = aux.Where(x => x.MP.UR.RegionId == 6).Count();

            var cdi = denuncias.Where(x => x.SolucionId == 1).Count();
            var constancia = denuncias.Where(x => x.SolucionId == 2).Count();
            var archivo = denuncias.Where(x => x.SolucionId == 3).Count();

            return Ok(new { regional = regional, metro = metro, regSolucion = regSolucion, metroSolucion = metroSolucion, cdi = cdi, constancia = constancia, archivo = archivo });
        }

    }
}