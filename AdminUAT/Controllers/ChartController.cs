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
            var fiscalia = await _context.Fiscalias.AsNoTracking()
                .Where(x => x.Value == "FI")
                .Select(x => x.Id).FirstOrDefaultAsync();

            if (fecha != null && fecha2 != null)
            {
                json = await JsonData2(fecha, fecha2);
                return json.OrderByDescending(x => x.Recibidas);
            }

            fecha = fecha == null ? DateTime.Now.ToString("yyyy-MM-dd") : fecha;

            var kiosco = await _context.BitaKiosco.AsNoTracking()
                .OrderBy(x => x.Id)
                .ToListAsync();

            var denuncia= await _context.Denuncia.AsNoTracking()
                    .Where(x => x.AltaSistema.ToString("yyyy-MM-dd") == fecha && x.Paso == 3)
                    .ToListAsync();

            foreach (var item in kiosco)
            {
                var recibidas = denuncia.Where(x => x.BitaKioscoId == item.Id).Count();
                var atendidas = denuncia.Where(x => x.BitaKioscoId == item.Id && x.SolucionId != null).Count();

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

            return json.OrderByDescending(x => x.Recibidas);
        }

        [Authorize(Roles = "AEI, Root")]
        private async Task<List<MapaData>> JsonData2(string fecha, string fecha2)
        {
            List<MapaData> json = new List<MapaData>();
            DateTime fechaI = Convert.ToDateTime(fecha);
            DateTime fechaF = Convert.ToDateTime(fecha2);

            var kiosco = await _context.BitaKiosco.AsNoTracking()
                .OrderBy(x => x.Id)
                .ToListAsync();

            var denuncia = await _context.Denuncia.AsNoTracking()
                    .Where(x => (x.AltaSistema.Date >= fechaI.Date && x.AltaSistema.Date <= fechaF.Date) && x.Paso == 3)
                    .ToListAsync();

            foreach (var item in kiosco)
            {
                var a = fechaI.DayOfWeek;

                var recibidas = denuncia.Where(x => x.BitaKioscoId == item.Id).Count();
                var atendidas = denuncia.Where(x => x.BitaKioscoId == item.Id && x.SolucionId != null).Count();

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

            return json;
        }

        //Denuncias por atender por AMP Metropolitana
        [Authorize(Roles = "AEI, Root, FiscMet")]
        [HttpGet("ChartMP")]
        public async Task<IEnumerable<MP>> ChartMP()
        {
            var fiscalia = await _context.Fiscalias.AsNoTracking()
                .Where(x => x.Value == "FI")
                .Select(x => x.Id).FirstOrDefaultAsync();

            var json = await _context.MP.AsNoTracking()
                .Where(x => x.Activo == true && x.UR.RegionId == 6 && x.FiscaliaId==fiscalia)
                .OrderByDescending(x => (x.Stock - x.Resuelto))
                .ToListAsync();

            return json;
        }

        //Denuncias por atender por AMP Regional
        [Authorize(Roles = "AEI, Root, FiscReg")]
        [HttpGet("RegionalChartMP")]
        public async Task<IEnumerable<MP>> RegionalChartMP()
        {
            var fiscalia = await _context.Fiscalias.AsNoTracking()
                .Where(x => x.Value == "FI").Select(x => x.Id).FirstOrDefaultAsync();

            var json = await _context.MP.AsNoTracking()
                .Where(x => x.Activo == true && x.UR.RegionId != 6 && x.FiscaliaId == fiscalia)
                .OrderByDescending(x => (x.Stock - x.Resuelto))
                .ToListAsync();

            return json;
        }

        [Authorize(Roles ="AEI,Root")]
        [HttpGet("General")]
        public async Task<IActionResult> General(string fecha,string fecha2)
        {
            var fiscalias = await _context.Fiscalias.AsNoTracking()
                .Where(x => x.Status == Status.Active).ToListAsync();
            var fiscSend = new List<DataFiscalia>();

            if (fecha != null && fecha2 != null)
            {
                DateTime fechaI = Convert.ToDateTime(fecha);
                DateTime fechaF = Convert.ToDateTime(fecha2);

                var denuncia = await _context.Denuncia.AsNoTracking()
                        .Include(x => x.MP)
                            .ThenInclude(x => x.UR)
                        .Where(x => (x.AltaSistema.Date >= fechaI.Date && x.AltaSistema.Date <= fechaF.Date) && x.Paso == 3
                        && x.MPId != null)
                        .ToListAsync();

                foreach (var item in fiscalias)
                {
                    var den = denuncia.Where(x => x.FiscaliaId == item.Id).ToList();

                    if (item.Value == "FI")
                    {
                        var fim = den.Where(x => x.MP.UR.RegionId == 6).ToList();
                        var reg = den.Where(x => x.MP.UR.RegionId != 6).ToList();

                        var fimA= fim.Where(x => x.SolucionId != null).Count();
                        var fimR = fim.Count();

                        var regA = reg.Where(x => x.SolucionId != null).Count();
                        var regR = reg.Count();

                        var sendM = new DataFiscalia
                        {
                            Nombre = "FIM",
                            Atendidas=fimA,
                            Recibidas=fimR
                        };
                        var sendR = new DataFiscalia
                        {
                            Nombre = "FIR",
                            Atendidas = regA,
                            Recibidas = regR
                        };
                        fiscSend.Add(sendM);
                        fiscSend.Add(sendR);
                    }
                    else
                    {
                        var nombre = await _context.Fiscalias.AsNoTracking()
                            .Where(x => x.Id == item.Id).Select(x => x.Value).FirstOrDefaultAsync();

                        var atendidas = den.Where(x => x.SolucionId != null).Count();
                        var recibidas = den.Count();

                        var sendF = new DataFiscalia
                        {
                            Nombre = nombre,
                            Atendidas = atendidas,
                            Recibidas = recibidas
                        };

                        fiscSend.Add(sendF);
                    }  
                }

                var cdi = denuncia.Where(x => x.SolucionId == 1).Count();
                var constancia = denuncia.Where(x => x.SolucionId == 2).Count();
                var archivo = denuncia.Where(x => x.SolucionId == 3).Count();

                var send = new MapaFiscalia
                {
                    Data=fiscSend,
                    CDI=cdi,
                    Archivo=archivo,
                    Constancia=constancia
                };
                return Ok(send);
            }
            else
            {
                fecha = fecha == null ? DateTime.Now.ToString("yyyy-MM-dd") : fecha;

                var denuncia = await _context.Denuncia.AsNoTracking()
                        .Where(x => x.AltaSistema.ToString("yyyy-MM-dd") == fecha && x.Paso == 3
                        && x.MPId != null)
                        .ToListAsync();

                foreach (var item in fiscalias)
                {
                    var den = denuncia.Where(x => x.FiscaliaId == item.Id).ToList();

                    var nombre = await _context.Fiscalias.AsNoTracking()
                        .Where(x => x.Id == item.Id).Select(x => x.Value).FirstOrDefaultAsync();

                    var atendidas = den.Where(x => x.SolucionId != null).Count();
                    var recibidas = den.Where(x => x.SolucionId == null).Count();

                    var sendF = new DataFiscalia
                    {
                        Nombre = nombre,
                        Atendidas = atendidas,
                        Recibidas = recibidas
                    };

                    fiscSend.Add(sendF);
                }

                var cdi = denuncia.Where(x => x.SolucionId == 1).Count();
                var constancia = denuncia.Where(x => x.SolucionId == 2).Count();
                var archivo = denuncia.Where(x => x.SolucionId == 3).Count();

                var send = new MapaFiscalia
                {
                    Data = fiscSend,
                    CDI = cdi,
                    Archivo = archivo,
                    Constancia = constancia
                };
                return Ok(send);
            }
        }

        [Authorize(Roles = "AEI, Root, FiscMet, FiscReg")]
        [HttpGet("Regional")]
        public async Task<IActionResult> Regional(string fecha, string fecha2)
        {
            var fiscalia = await _context.Fiscalias.AsNoTracking()
                .Where(x => x.Value == "FI").Select(x => x.Id).FirstOrDefaultAsync();

            if (fecha2 != "" && fecha2 != null)
            {
                DateTime fechaI = Convert.ToDateTime(fecha);
                DateTime fechaF = Convert.ToDateTime(fecha2);

                var denuncias1 = await _context.Denuncia.AsNoTracking()
                    .Include(x => x.MP)
                        .ThenInclude(x => x.UR)
                    .Where(x => (x.AltaSistema.Date >= fechaI.Date && x.AltaSistema.Date <= fechaF.Date) && x.Paso == 3 && x.MPId != null && x.FiscaliaId == fiscalia)
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

            var denuncias = await _context.Denuncia.AsNoTracking()
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

            return Ok(new { regional = regional, metro = metro, regSolucion = regSolucion, metroSolucion = metroSolucion, cdi = cdi, constancia = constancia, archivo = archivo });
        }

    }
}