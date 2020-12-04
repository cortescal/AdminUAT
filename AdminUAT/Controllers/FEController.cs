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
    [Authorize(Roles = "FiscEsp, Root")]
    [Route("[controller]")]
    [ApiController]
    public class FEController : Controller
    {
        private readonly NewUatDbContext _context;
        private UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _application;

        public FEController(NewUatDbContext context, UserManager<ApplicationUser> userManager
            , ApplicationDbContext application)
        {
            _context = context;
            _userManager = userManager;
            _application = application;
        }

        [Authorize(Roles = "FiscEsp, Root")]
        [HttpGet("Chart")]
        public async Task<IActionResult> Chart(string fecha, string fecha2)
        {
            var userId = _userManager.GetUserId(User);

            var user = await _userManager.FindByIdAsync(userId);

            ViewData["fecha"] = fecha;
            ViewData["fecha2"] = fecha2;

            return View();
        }

        [Authorize(Roles = "FiscEsp, Root")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(string fecha, string fecha2,Guid fiscalia)
        {
            var userId = _userManager.GetUserId(User);

            var user = await _userManager.FindByIdAsync(userId);

            ViewData["fecha"] = fecha;
            ViewData["fecha2"] = fecha2;

            return View();
        }

        [Authorize(Roles = "FiscEsp, Root")]
        [HttpGet("FE/JsonData")]
        public async Task<IEnumerable<MapaData>> JsonData(string fecha, string fecha2)
        {
            List<MapaData> json = new List<MapaData>();

            var userId = _userManager.GetUserId(User);

            if (fecha2 != "" && fecha2 != null)
            {
                json = await JsonData2(fecha, fecha2, userId);
                return json.OrderByDescending(x => x.Recibidas);
            }

            fecha = fecha == null ? DateTime.Now.ToString("yyyy-MM-dd") : fecha;

            var kiosco = await _context.BitaKiosco
                .OrderBy(x => x.Id)
                .ToListAsync();

            if (User.IsInRole("Root"))
            {

            }
            else
            {
                var rolFis = await _application.RolFiscalias.AsNoTracking()
                .Where(x => x.UserId == Guid.Parse(userId)).Select(x => x.FiscaliaId).FirstOrDefaultAsync();

                foreach (var item in kiosco)
                {
                    var recibidas = await _context.Denuncia.AsNoTracking()
                        .Where(x => x.AltaSistema.ToString("yyyy-MM-dd") == fecha &&
                        x.BitaKioscoId == item.Id && x.Paso == 3 && x.FiscaliaId == rolFis).CountAsync();

                    var atendidas = await _context.Denuncia.AsNoTracking()
                        .Where(x => x.AltaSistema.ToString("yyyy-MM-dd") == fecha &&
                        x.BitaKioscoId == item.Id && x.SolucionId != null && x.FiscaliaId == rolFis).CountAsync();

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

            return json.OrderByDescending(x => x.Recibidas);
        }

        [Authorize(Roles = "FiscEsp, Root")]
        [HttpGet("FE/JsonDataRoot")]
        public async Task<IEnumerable<MapaData>> JsonDataRoot(string fecha, string fecha2,Guid fiscalia)
        {
            List<MapaData> json = new List<MapaData>();

            if (fecha2 != "" && fecha2 != null)
            {
                json = await JsonData2Root(fecha, fecha2, fiscalia);
                return json.OrderByDescending(x => x.Recibidas);
            }

            fecha = fecha == null ? DateTime.Now.ToString("yyyy-MM-dd") : fecha;

            var kiosco = await _context.BitaKiosco
                .OrderBy(x => x.Id)
                .ToListAsync();

            foreach (var item in kiosco)
            {
                var recibidas = await _context.Denuncia.AsNoTracking()
                    .Where(x => x.AltaSistema.ToString("yyyy-MM-dd") == fecha &&
                    x.BitaKioscoId == item.Id && x.Paso == 3 && x.FiscaliaId == fiscalia).CountAsync();

                var atendidas = await _context.Denuncia.AsNoTracking()
                    .Where(x => x.AltaSistema.ToString("yyyy-MM-dd") == fecha &&
                    x.BitaKioscoId == item.Id && x.SolucionId != null && x.FiscaliaId == fiscalia).CountAsync();

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

        [Authorize(Roles = "FiscEsp, Root")]
        private async Task<List<MapaData>> JsonData2(string fecha,string fecha2,string user)
        {
            List<MapaData> json = new List<MapaData>();
            DateTime fechaI = Convert.ToDateTime(fecha);
            DateTime fechaF = Convert.ToDateTime(fecha2);

            var kiosco = await _context.BitaKiosco
                    .OrderBy(x => x.Id)
                    .ToListAsync();

            //Esta logueado como root
            if (User.IsInRole("Root"))
            {

            }
            //Esta logueado como fiscal especial
            else
            {
                var rolFis = await _application.RolFiscalias.AsNoTracking()
                .Where(x => x.UserId == Guid.Parse(user)).Select(x => x.FiscaliaId).FirstOrDefaultAsync();

                foreach (var item in kiosco)
                {
                    var recibidas = await _context.Denuncia.AsNoTracking()
                        .Where(x => x.AltaSistema.Date >= fechaI.Date && x.AltaSistema.Date <= fechaF.Date&&
                        x.BitaKioscoId==item.Id&&x.Paso==3&&x.FiscaliaId==rolFis).CountAsync();

                    var atendidas= await _context.Denuncia.AsNoTracking()
                        .Where(x => x.AltaSistema.Date >= fechaI.Date && x.AltaSistema.Date <= fechaF.Date &&
                        x.BitaKioscoId == item.Id && x.SolucionId != null && x.FiscaliaId == rolFis).CountAsync();

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
            return json;
        }

        [Authorize(Roles = "FiscEsp, Root")]
        private async Task<List<MapaData>> JsonData2Root(string fecha, string fecha2, Guid fiscalia)
        {
            List<MapaData> json = new List<MapaData>();
            DateTime fechaI = Convert.ToDateTime(fecha);
            DateTime fechaF = Convert.ToDateTime(fecha2);

            var kiosco = await _context.BitaKiosco
                    .OrderBy(x => x.Id)
                    .ToListAsync();

            foreach (var item in kiosco)
            {
                var recibidas = await _context.Denuncia.AsNoTracking()
                    .Where(x => x.AltaSistema.Date >= fechaI.Date && x.AltaSistema.Date <= fechaF.Date &&
                    x.BitaKioscoId == item.Id && x.Paso == 3 && x.FiscaliaId == fiscalia).CountAsync();

                var atendidas = await _context.Denuncia.AsNoTracking()
                    .Where(x => x.AltaSistema.Date >= fechaI.Date && x.AltaSistema.Date <= fechaF.Date &&
                    x.BitaKioscoId == item.Id && x.SolucionId != null && x.FiscaliaId == fiscalia).CountAsync();

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

        [Authorize(Roles = "FiscEsp, Root")]
        [HttpGet("FE/ChartMP")]
        public async Task<IEnumerable<MP>> ChartMP()
        {
            var userId = _userManager.GetUserId(User);
            var json = new List<MP>();

            //Esta logueado como root
            if (User.IsInRole("Root"))
            {
                

                return json;
            }
            else
            {
                var rolFis = await _application.RolFiscalias.AsNoTracking()
                .Where(x => x.UserId == Guid.Parse(userId)).Select(x => x.FiscaliaId).FirstOrDefaultAsync();

                json = await _context.MP
                    .Where(x => x.Activo == true && x.FiscaliaId == rolFis && x.UR.RegionId != 6)
                    .OrderByDescending(x => (x.Stock - x.Resuelto))
                    .ToListAsync();

                return json;
            }
            
        }

        [Authorize(Roles = "FiscEsp, Root")]
        [HttpGet("FE/ChartMPRoot")]
        public async Task<IEnumerable<MP>> ChartMPRoot(Guid fiscalia)
        {
            var userId = _userManager.GetUserId(User);
            var json = new List<MP>();

            json = await _context.MP
                .Where(x => x.Activo == true && x.UR.RegionId != 6 && x.FiscaliaId==fiscalia)
                .OrderByDescending(x => (x.Stock - x.Resuelto))
                .ToListAsync();

            return json;
            

        }

        //Denuncias por atender por AMP Regional
        [Authorize(Roles = "FiscEsp, Root")]
        [HttpGet("FE/RegionalChartMP")]
        public async Task<IEnumerable<MP>> RegionalChartMP()
        {
            var userId = _userManager.GetUserId(User);
            var json = new List<MP>();

            if (User.IsInRole("Root"))
            {
                return json;
            }
            else
            {
                var rolFis = await _application.RolFiscalias.AsNoTracking()
                    .Where(x => x.UserId == Guid.Parse(userId)).Select(x => x.FiscaliaId).FirstOrDefaultAsync();

                json = await _context.MP
                    .Where(x => x.Activo == true && x.UR.RegionId != 6 && x.FiscaliaId == rolFis)
                    .OrderByDescending(x => (x.Stock - x.Resuelto))
                    .ToListAsync();

                return json;
            }           
        }

        [Authorize(Roles = "FiscEsp, Root")]
        [HttpGet("FE/RegionalChartMPRoot")]
        public async Task<IEnumerable<MP>> RegionalChartMPRoot(Guid fiscalia)
        {
            var json = new List<MP>();

            json = await _context.MP
                .Where(x => x.Activo == true && x.UR.RegionId != 6 && x.FiscaliaId == fiscalia)
                .OrderByDescending(x => (x.Stock - x.Resuelto))
                .ToListAsync();

            return json;
            
        }

        [Authorize(Roles = "FiscEsp, Root")]
        [HttpGet("FE/Regional")]
        public async Task<IActionResult> Regional(string fecha, string fecha2)
        {
            if (User.IsInRole("Root"))
            {
                return Ok(new
                {
                    regional = "",
                    metro = "",
                    regSolucion = "",
                    metroSolucion = "",
                    cdi = "",
                    constancia = "",
                    archivo = ""
                });
            }
            else
            {
                var userId = _userManager.GetUserId(User);
                var rolFis = await _application.RolFiscalias.AsNoTracking()
                        .Where(x => x.UserId == Guid.Parse(userId)).Select(x => x.FiscaliaId).FirstOrDefaultAsync();

                if (fecha2 != "" && fecha2 != null)
                {
                    DateTime fechaI = Convert.ToDateTime(fecha);
                    DateTime fechaF = Convert.ToDateTime(fecha2);

                    var denuncias1 = await _context.Denuncia
                        .Include(x => x.MP)
                            .ThenInclude(x => x.UR)
                        .Where(x => (x.AltaSistema.Date >= fechaI.Date && x.AltaSistema.Date <= fechaF.Date) &&
                            x.Paso == 3 && x.MPId != null && x.FiscaliaId == rolFis)
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
                else
                {
                    fecha = fecha == null ? DateTime.Now.ToString("yyyy-MM-dd") : fecha;

                    var denuncias = await _context.Denuncia
                        .Include(x => x.MP)
                            .ThenInclude(x => x.UR)
                         .Where(x => x.AltaSistema.ToString("yyyy-MM-dd") == fecha && x.Paso == 3 && x.FiscaliaId == rolFis)
                        .ToListAsync();

                    var regional = denuncias.Where(x => x.MP.UR.RegionId != 6).Count();
                    var metro = denuncias.Where(x => x.MP.UR.RegionId == 6).Count();

                    var aux = denuncias.Where(x => x.SolucionId != null).ToList();
                    var regSolucion = aux.Where(x => x.MP.UR.RegionId != 6).Count();
                    var metroSolucion = aux.Where(x => x.MP.UR.RegionId == 6).Count();

                    var cdi = denuncias.Where(x => x.SolucionId == 1).Count();
                    var constancia = denuncias.Where(x => x.SolucionId == 2).Count();
                    var archivo = denuncias.Where(x => x.SolucionId == 3).Count();

                    return Ok(new { regional = regional, metro = metro, regSolucion = regSolucion, 
                        metroSolucion = metroSolucion, cdi = cdi, constancia = constancia, archivo = archivo });
                }
            } 
        }

        [Authorize(Roles = "FiscEsp, Root")]
        [HttpGet("FE/RegionalRoot")]
        public async Task<IActionResult> RegionalRoot(string fecha, string fecha2,Guid fiscalia)
        {
            if (fecha2 != "" && fecha2 != null)
            {
                DateTime fechaI = Convert.ToDateTime(fecha);
                DateTime fechaF = Convert.ToDateTime(fecha2);

                var denuncias1 = await _context.Denuncia
                    .Include(x => x.MP)
                        .ThenInclude(x => x.UR)
                    .Where(x => (x.AltaSistema.Date >= fechaI.Date && x.AltaSistema.Date <= fechaF.Date) &&
                        x.Paso == 3 && x.MPId != null && x.FiscaliaId == fiscalia)
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
            else
            {
                fecha = fecha == null ? DateTime.Now.ToString("yyyy-MM-dd") : fecha;

                var denuncias = await _context.Denuncia
                    .Include(x => x.MP)
                        .ThenInclude(x => x.UR)
                        .Where(x => x.AltaSistema.ToString("yyyy-MM-dd") == fecha && x.Paso == 3 && x.FiscaliaId == fiscalia)
                    .ToListAsync();

                var regional = denuncias.Where(x => x.MP.UR.RegionId != 6).Count();
                var metro = denuncias.Where(x => x.MP.UR.RegionId == 6).Count();

                var aux = denuncias.Where(x => x.SolucionId != null).ToList();
                var regSolucion = aux.Where(x => x.MP.UR.RegionId != 6).Count();
                var metroSolucion = aux.Where(x => x.MP.UR.RegionId == 6).Count();

                var cdi = denuncias.Where(x => x.SolucionId == 1).Count();
                var constancia = denuncias.Where(x => x.SolucionId == 2).Count();
                var archivo = denuncias.Where(x => x.SolucionId == 3).Count();

                return Ok(new
                {
                    regional = regional,
                    metro = metro,
                    regSolucion = regSolucion,
                    metroSolucion = metroSolucion,
                    cdi = cdi,
                    constancia = constancia,
                    archivo = archivo
                });
            }
            
        }

    }
}
