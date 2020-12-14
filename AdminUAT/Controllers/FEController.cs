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
    [Authorize(Roles = "FiscEsp, Root,FiscReg,FiscMet")]
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

        [HttpGet("Chart")]
        public async Task<IActionResult> Chart(string fecha, string fecha2)
        {
            var userId = _userManager.GetUserId(User);

            var user = await _userManager.FindByIdAsync(userId);

            ViewData["fecha"] = fecha;
            ViewData["fecha2"] = fecha2;

            return View();
        }

        [HttpGet("Index")]
        public async Task<IActionResult> Index(string fecha, string fecha2,Guid fiscalia)
        {
            var userId = _userManager.GetUserId(User);

            var user = await _userManager.FindByIdAsync(userId);

            ViewData["fecha"] = fecha;
            ViewData["fecha2"] = fecha2;

            return View();
        }

        [HttpGet("FE/JsonData")]
        public async Task<IEnumerable<MapaData>> JsonData(string fecha, string fecha2)
        {
            List<MapaData> json = new List<MapaData>();

            var userId = _userManager.GetUserId(User);

            if (fecha != null && fecha2 != null)
            {
                json = await JsonData2(fecha, fecha2, userId);
                return json.OrderByDescending(x => x.Recibidas);
            }

            fecha = fecha == null ? DateTime.Now.ToString("yyyy-MM-dd") : fecha;

            var kiosco = await _context.BitaKiosco.AsNoTracking()
                .OrderBy(x => x.Id)
                .ToListAsync();

            if(User.IsInRole("FiscReg"))
            {
                var rolFis = await _context.Fiscalias.Where(x => x.Value == "FI").Select(x => x.Id).FirstOrDefaultAsync();

                var denuncia = await _context.Denuncia.AsNoTracking()
                    .Include(x=>x.MP)
                        .ThenInclude(x=>x.UR)
                    .Where(x => x.AltaSistema.ToString("yyyy-MM-dd") == fecha && x.Paso == 3 && x.FiscaliaId == rolFis && 
                        x.MP.UR.RegionId !=6)
                    .Select(x => new {
                        Kiosco = x.BitaKioscoId,
                        Solucion = x.SolucionId,
                    })
                    .ToListAsync();

                foreach (var item in kiosco)
                {
                    var recibidas = denuncia.Where(x => x.Kiosco == item.Id).Count();

                    var atendidas = denuncia.Where(x => x.Kiosco == item.Id && x.Solucion != null).Count();

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
            }
            else if(User.IsInRole("FiscMet"))
            {
                var rolFis = await _context.Fiscalias.Where(x => x.Value == "FI").Select(x => x.Id).FirstOrDefaultAsync();

                var denuncia = await _context.Denuncia.AsNoTracking()
                    .Include(x => x.MP)
                        .ThenInclude(x => x.UR)
                    .Where(x => x.AltaSistema.ToString("yyyy-MM-dd") == fecha && x.Paso == 3 && x.FiscaliaId == rolFis &&
                        x.MP.UR.RegionId == 6)
                    .Select(x => new {
                        Kiosco = x.BitaKioscoId,
                        Solucion = x.SolucionId,
                    })
                    .ToListAsync();

                foreach (var item in kiosco)
                {
                    var recibidas = denuncia.Where(x => x.Kiosco == item.Id).Count();

                    var atendidas = denuncia.Where(x => x.Kiosco == item.Id && x.Solucion != null).Count();

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
            }
            else
            {
                var rolFis = await _application.RolFiscalias.AsNoTracking()
                    .Where(x => x.UserId == Guid.Parse(userId)).Select(x => x.FiscaliaId).FirstOrDefaultAsync();

                var denuncia = await _context.Denuncia.AsNoTracking()
                    .Where(x => x.AltaSistema.ToString("yyyy-MM-dd") == fecha && x.Paso == 3 && x.FiscaliaId == rolFis)
                    .Select(x=>new {
                        Kiosco=x.BitaKioscoId,
                        Solucion=x.SolucionId
                    })
                    .ToListAsync();

                foreach (var item in kiosco)
                {
                    var recibidas = denuncia.Where(x => x.Kiosco == item.Id).Count();

                    var atendidas = denuncia.Where(x =>x.Kiosco == item.Id && x.Solucion != null).Count();

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
            }       

            return json.OrderByDescending(x => x.Recibidas);
        }

        [HttpGet("FE/JsonDataRoot")]
        public async Task<IEnumerable<MapaData>> JsonDataRoot(string fecha, string fecha2,Guid fiscalia)
        {
            List<MapaData> json = new List<MapaData>();

            if (fecha != null && fecha2 != null)
            {
                json = await JsonData2Root(fecha, fecha2, fiscalia);
                return json.OrderByDescending(x => x.Recibidas);
            }

            fecha = fecha == null ? DateTime.Now.ToString("yyyy-MM-dd") : fecha;

            var kiosco = await _context.BitaKiosco.AsNoTracking()
                .OrderBy(x => x.Id)
                .ToListAsync();

            var denuncia = await _context.Denuncia.AsNoTracking()
                .Where(x => x.AltaSistema.ToString("yyyy-MM-dd") == fecha && x.Paso == 3 && x.FiscaliaId == fiscalia)
                .Select(x => new
                {
                    Kiosco = x.BitaKioscoId,
                    Solucion = x.SolucionId
                }).ToListAsync();

            foreach (var item in kiosco)
            {
                var recibidas = denuncia.Where(x =>x.Kiosco == item.Id).Count();

                var atendidas = denuncia.Where(x => x.Kiosco == item.Id && x.Solucion != null).Count();

                if(recibidas>0||atendidas>0)
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

        private async Task<List<MapaData>> JsonData2(string fecha,string fecha2,string user)
        {
            List<MapaData> json = new List<MapaData>();
            DateTime fechaI = Convert.ToDateTime(fecha);
            DateTime fechaF = Convert.ToDateTime(fecha2);

            var kiosco = await _context.BitaKiosco.AsNoTracking()
                    .OrderBy(x => x.Id)
                    .ToListAsync();

            if (User.IsInRole("FiscReg"))
            {
                var rolFis = await _context.Fiscalias.Where(x => x.Value == "FI").Select(x => x.Id).FirstOrDefaultAsync();

                var denuncia = await _context.Denuncia.AsNoTracking()
                    .Include(x => x.MP)
                        .ThenInclude(x => x.UR)
                    .Where(x => x.AltaSistema.Date >= fechaI.Date && x.AltaSistema.Date <= fechaF.Date && x.Paso == 3 && x.FiscaliaId == rolFis &&
                        x.MP.UR.RegionId != 6)
                    .Select(x => new {
                        Kiosco = x.BitaKioscoId,
                        Solucion = x.SolucionId,
                    })
                    .ToListAsync();

                foreach (var item in kiosco)
                {
                    var recibidas = denuncia.Where(x => x.Kiosco == item.Id).Count();

                    var atendidas = denuncia.Where(x => x.Kiosco == item.Id && x.Solucion != null).Count();

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
            }
            else if (User.IsInRole("FiscMet"))
            {
                var rolFis = await _context.Fiscalias.Where(x => x.Value == "FI").Select(x => x.Id).FirstOrDefaultAsync();

                var denuncia = await _context.Denuncia.AsNoTracking()
                    .Include(x => x.MP)
                        .ThenInclude(x => x.UR)
                    .Where(x => x.AltaSistema.Date >= fechaI.Date && x.AltaSistema.Date <= fechaF.Date && x.Paso == 3 && x.FiscaliaId == rolFis &&
                        x.MP.UR.RegionId == 6)
                    .Select(x => new {
                        Kiosco = x.BitaKioscoId,
                        Solucion = x.SolucionId,
                    })
                    .ToListAsync();

                foreach (var item in kiosco)
                {
                    var recibidas = denuncia.Where(x => x.Kiosco == item.Id).Count();

                    var atendidas = denuncia.Where(x => x.Kiosco == item.Id && x.Solucion != null).Count();

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
            }
            //Esta logueado como fiscal especial
            else
            {
                var rolFis = await _application.RolFiscalias.AsNoTracking()
                .Where(x => x.UserId == Guid.Parse(user)).Select(x => x.FiscaliaId).FirstOrDefaultAsync();

                var denuncia = await _context.Denuncia.AsNoTracking()
                        .Where(x => x.AltaSistema.Date >= fechaI.Date && x.AltaSistema.Date <= fechaF.Date && 
                        x.Paso == 3 && x.FiscaliaId == rolFis)
                        .Select(x => new
                        {
                            Kiosco = x.BitaKioscoId,
                            Solucion = x.SolucionId
                        }).ToListAsync();

                foreach (var item in kiosco)
                {
                    var recibidas = denuncia.Where(x =>x.Kiosco==item.Id).Count();

                    var atendidas= denuncia.Where(x =>x.Kiosco == item.Id && x.Solucion != null).Count();

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
            }
            return json;
        }

        private async Task<List<MapaData>> JsonData2Root(string fecha, string fecha2, Guid fiscalia)
        {
            List<MapaData> json = new List<MapaData>();
            DateTime fechaI = Convert.ToDateTime(fecha);
            DateTime fechaF = Convert.ToDateTime(fecha2);

            var kiosco = await _context.BitaKiosco.AsNoTracking()
                    .OrderBy(x => x.Id)
                    .ToListAsync();

            var denuncia = await _context.Denuncia.AsNoTracking()
                    .Where(x => x.AltaSistema.Date >= fechaI.Date && x.AltaSistema.Date <= fechaF.Date
                        && x.Paso == 3 && x.FiscaliaId == fiscalia)
                    .Select(x => new
                        {
                            Kiosco = x.BitaKioscoId,
                            Solucion = x.SolucionId
                        }).ToListAsync();

            foreach (var item in kiosco)
            {
                var recibidas = denuncia
                    .Where(x =>x.Kiosco == item.Id).Count();

                var atendidas = denuncia
                    .Where(x =>x.Kiosco == item.Id && x.Solucion != null).Count();

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

        [HttpGet("FE/ChartMP")]
        public async Task<IEnumerable<MP>> ChartMP()
        {
            var userId = _userManager.GetUserId(User);
            var json = new List<MP>();

            //Esta logueado como root
            if (User.IsInRole("FiscMet"))
            {
                var rolFis = await _context.Fiscalias.Where(x => x.Value == "FI").Select(x => x.Id).FirstOrDefaultAsync();

                json = await _context.MP.AsNoTracking()
                    .Where(x => x.Activo == true && x.FiscaliaId == rolFis&&x.URId==6)
                    .OrderByDescending(x => (x.Stock - x.Resuelto))
                    .ToListAsync();

                return json;
            }
            else if(User.IsInRole("FiscReg"))
            {
                var rolFis = await _context.Fiscalias.Where(x => x.Value == "FI").Select(x => x.Id).FirstOrDefaultAsync();

                json = await _context.MP.AsNoTracking()
                    .Where(x => x.Activo == true && x.FiscaliaId == rolFis && x.UR.RegionId != 6&&x.URId!=6)
                    .OrderByDescending(x => (x.Stock - x.Resuelto))
                    .ToListAsync();

                return json;
            }
            else
            {
                var rolFis = await _application.RolFiscalias.AsNoTracking()
                .Where(x => x.UserId == Guid.Parse(userId)).Select(x => x.FiscaliaId).FirstOrDefaultAsync();

                json = await _context.MP.AsNoTracking()
                    .Where(x => x.Activo == true && x.FiscaliaId == rolFis)
                    .OrderByDescending(x => (x.Stock - x.Resuelto))
                    .ToListAsync();

                return json;
            }
            
        }

        [HttpGet("FE/ChartMPRoot")]
        public async Task<IEnumerable<MP>> ChartMPRoot(Guid fiscalia)
        {
            var userId = _userManager.GetUserId(User);
            var json = new List<MP>();

            json = await _context.MP.AsNoTracking()
                .Where(x => x.Activo == true && x.FiscaliaId==fiscalia)
                .OrderByDescending(x => (x.Stock - x.Resuelto))
                .ToListAsync();

            return json;
            

        }

        [HttpGet("FE/Regional")]
        public async Task<IActionResult> Regional(string fecha, string fecha2)
        {
            if (User.IsInRole("FiscMet"))
            {
                var rolFis = await _context.Fiscalias.AsNoTracking()
                        .Where(x => x.Value=="FI").Select(x => x.Id).FirstOrDefaultAsync();

                if (fecha != null && fecha2 != null)
                {
                    DateTime fechaI = Convert.ToDateTime(fecha);
                    DateTime fechaF = Convert.ToDateTime(fecha2);

                    var denuncias1 = await _context.Denuncia
                        .Include(x => x.MP)
                            .ThenInclude(x => x.UR)
                        .Where(x => (x.AltaSistema.Date >= fechaI.Date && x.AltaSistema.Date <= fechaF.Date) &&
                            x.Paso == 3 && x.MPId != null && x.FiscaliaId == rolFis&&x.MP.UR.RegionId==6)
                        .Select(x => new {
                            Solucion = x.SolucionId
                        })
                        .ToListAsync();

                    var regional1 = denuncias1.Count();

                    var regSolucion1 = denuncias1.Where(x => x.Solucion != null).Count();
                    var cdi1 = denuncias1.Where(x => x.Solucion == 1).Count();
                    var constancia1 = denuncias1.Where(x => x.Solucion == 2).Count();
                    var archivo1 = denuncias1.Where(x => x.Solucion == 3).Count();

                    return Ok(new { regional = regional1, regSolucion = regSolucion1, cdi = cdi1, constancia = constancia1, archivo = archivo1 });
                }
                else
                {
                    fecha = fecha == null ? DateTime.Now.ToString("yyyy-MM-dd") : fecha;

                    var denuncias = await _context.Denuncia
                        .Include(x => x.MP)
                            .ThenInclude(x => x.UR)
                         .Where(x => x.AltaSistema.ToString("yyyy-MM-dd") == fecha && x.Paso == 3 && x.FiscaliaId == rolFis &&
                            x.MP.UR.RegionId==6)
                         .Select(x => new {
                             Solucion = x.SolucionId
                         })
                        .ToListAsync();

                    var regional = denuncias.Count();

                    var regSolucion = denuncias.Where(x => x.Solucion != null).Count();

                    var cdi = denuncias.Where(x => x.Solucion == 1).Count();
                    var constancia = denuncias.Where(x => x.Solucion == 2).Count();
                    var archivo = denuncias.Where(x => x.Solucion == 3).Count();

                    return Ok(new
                    {
                        regional = regional,
                        regSolucion = regSolucion,
                        cdi = cdi,
                        constancia = constancia,
                        archivo = archivo
                    });
                }
            }
            else if(User.IsInRole("FiscReg"))
            {
                var rolFis = await _context.Fiscalias.AsNoTracking()
                        .Where(x => x.Value == "FI").Select(x => x.Id).FirstOrDefaultAsync();

                if (fecha != null && fecha2 != null)
                {
                    DateTime fechaI = Convert.ToDateTime(fecha);
                    DateTime fechaF = Convert.ToDateTime(fecha2);

                    var denuncias1 = await _context.Denuncia
                        .Include(x => x.MP)
                        .ThenInclude(x=>x.UR)
                        .Where(x => (x.AltaSistema.Date >= fechaI.Date && x.AltaSistema.Date <= fechaF.Date) &&
                            x.Paso == 3 && x.MPId != null && x.FiscaliaId == rolFis && x.MP.UR.RegionId!=6)
                        .Select(x => new {
                            Solucion = x.SolucionId
                        })
                        .ToListAsync();

                    var regional1 = denuncias1.Count();

                    var regSolucion1 = denuncias1.Where(x => x.Solucion != null).Count();
                    var cdi1 = denuncias1.Where(x => x.Solucion == 1).Count();
                    var constancia1 = denuncias1.Where(x => x.Solucion == 2).Count();
                    var archivo1 = denuncias1.Where(x => x.Solucion == 3).Count();

                    return Ok(new { regional = regional1, regSolucion = regSolucion1, cdi = cdi1, constancia = constancia1, archivo = archivo1 });
                }
                else
                {
                    fecha = fecha == null ? DateTime.Now.ToString("yyyy-MM-dd") : fecha;

                    var denuncias = await _context.Denuncia
                        .Include(x => x.MP)
                        .ThenInclude(x => x.UR)
                         .Where(x => x.AltaSistema.ToString("yyyy-MM-dd") == fecha && x.Paso == 3 && x.FiscaliaId == rolFis
                            && x.MP.UR.RegionId != 6)
                         .Select(x => new {
                             Solucion = x.SolucionId
                         })
                        .ToListAsync();

                    var regional = denuncias.Count();

                    var regSolucion = denuncias.Where(x => x.Solucion != null).Count();

                    var cdi = denuncias.Where(x => x.Solucion == 1).Count();
                    var constancia = denuncias.Where(x => x.Solucion == 2).Count();
                    var archivo = denuncias.Where(x => x.Solucion == 3).Count();

                    return Ok(new
                    {
                        regional = regional,
                        regSolucion = regSolucion,
                        cdi = cdi,
                        constancia = constancia,
                        archivo = archivo
                    });
                }
            }
            else
            {
                var userId = _userManager.GetUserId(User);
                var rolFis = await _application.RolFiscalias.AsNoTracking()
                        .Where(x => x.UserId == Guid.Parse(userId)).Select(x => x.FiscaliaId).FirstOrDefaultAsync();

                if (fecha != null && fecha2 != null)
                {
                    DateTime fechaI = Convert.ToDateTime(fecha);
                    DateTime fechaF = Convert.ToDateTime(fecha2);

                    var denuncias1 = await _context.Denuncia
                        .Include(x => x.MP)
                        .Where(x => (x.AltaSistema.Date >= fechaI.Date && x.AltaSistema.Date <= fechaF.Date) &&
                            x.Paso == 3 && x.MPId != null && x.FiscaliaId == rolFis)
                        .Select(x=>new {
                            Solucion=x.SolucionId
                        })
                        .ToListAsync();

                    var regional1 = denuncias1.Count();

                    var regSolucion1 = denuncias1.Where(x => x.Solucion != null).Count();
                    var cdi1 = denuncias1.Where(x => x.Solucion == 1).Count();
                    var constancia1 = denuncias1.Where(x => x.Solucion == 2).Count();
                    var archivo1 = denuncias1.Where(x => x.Solucion == 3).Count();

                    return Ok(new { regional = regional1, regSolucion = regSolucion1, cdi = cdi1, constancia = constancia1, archivo = archivo1 });
                }
                else
                {
                    fecha = fecha == null ? DateTime.Now.ToString("yyyy-MM-dd") : fecha;

                    var denuncias = await _context.Denuncia
                        .Include(x => x.MP)
                         .Where(x => x.AltaSistema.ToString("yyyy-MM-dd") == fecha && x.Paso == 3 && x.FiscaliaId == rolFis)
                         .Select(x => new {
                             Solucion = x.SolucionId
                         })
                        .ToListAsync();

                    var regional = denuncias.Count();

                    var regSolucion = denuncias.Where(x => x.Solucion != null).Count();

                    var cdi = denuncias.Where(x => x.Solucion == 1).Count();
                    var constancia = denuncias.Where(x => x.Solucion == 2).Count();
                    var archivo = denuncias.Where(x => x.Solucion == 3).Count();

                    return Ok(new { regional = regional, regSolucion = regSolucion, 
                         cdi = cdi, constancia = constancia, archivo = archivo });
                }
            } 
        }

        [HttpGet("FE/RegionalRoot")]
        public async Task<IActionResult> RegionalRoot(string fecha, string fecha2,Guid fiscalia)
        {
            if (fecha != null && fecha2 != null)
            {
                DateTime fechaI = Convert.ToDateTime(fecha);
                DateTime fechaF = Convert.ToDateTime(fecha2);

                var denuncias1 = await _context.Denuncia
                    .Include(x => x.MP)
                    .Where(x => (x.AltaSistema.Date >= fechaI.Date && x.AltaSistema.Date <= fechaF.Date) &&
                        x.Paso == 3 && x.MPId != null && x.FiscaliaId == fiscalia)
                    .Select(x => new {
                        Solucion = x.SolucionId
                    })
                    .ToListAsync();

                var regional1 = denuncias1.Count();

                var regSolucion1 = denuncias1.Where(x => x.Solucion != null).Count();
                var cdi1 = denuncias1.Where(x => x.Solucion == 1).Count();
                var constancia1 = denuncias1.Where(x => x.Solucion == 2).Count();
                var archivo1 = denuncias1.Where(x => x.Solucion == 3).Count();

                return Ok(new 
                { 
                    regional = regional1, 
                    regSolucion = regSolucion1, 
                    cdi = cdi1, 
                    constancia = constancia1, 
                    archivo = archivo1 
                });
            }
            else
            {
                fecha = fecha == null ? DateTime.Now.ToString("yyyy-MM-dd") : fecha;

                var denuncias = await _context.Denuncia
                    .Include(x => x.MP)
                        .ThenInclude(x => x.UR)
                        .Where(x => x.AltaSistema.ToString("yyyy-MM-dd") == fecha && x.Paso == 3 && x.FiscaliaId == fiscalia)
                        .Select(x => new {
                            Solucion = x.SolucionId
                        })
                    .ToListAsync();

                var regional = denuncias.Count();

                var regSolucion = denuncias.Where(x => x.Solucion != null).Count();
                var cdi = denuncias.Where(x => x.Solucion == 1).Count();
                var constancia = denuncias.Where(x => x.Solucion == 2).Count();
                var archivo = denuncias.Where(x => x.Solucion == 3).Count();

                return Ok(new
                {
                    regional = regional,
                    regSolucion = regSolucion,
                    cdi = cdi,
                    constancia = constancia,
                    archivo = archivo
                });
            }
            
        }

    }
}
