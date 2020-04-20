using AdminUAT.Data;
using AdminUAT.Dependencias;
using AdminUAT.Models.Denuncias;
using AdminUAT.Models.ExtraModels;
using AdminUAT.Models.ExtraModels.Admin;
using AdminUAT.Models.ExtraModels.UAT;
using AdminUAT.Models.MinisterioPublico;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Controllers
{   
    public class RootController : Controller
    {
        private readonly NewUatDbContext _contextUAT;
        private ApplicationDbContext _context;
        private readonly ISubProceso _subProceso;

        public RootController(NewUatDbContext contextUAT, ApplicationDbContext context, ISubProceso subProceso)
        {
            _contextUAT = contextUAT;
            _context = context;
            _subProceso = subProceso;
        }

        //Reporte
        [Authorize(Roles = "Root")]
        public async Task<IActionResult> Index(string fecha, string fecha2)
        {
            var denuncias = fecha2 == null ? await PorFecha(fecha) : await PorRangoFecha(fecha, fecha2);

            ViewData["TotalKiosco"] = CuentaDenunciasPorKiosco(denuncias);
            ViewData["denuncias"] = denuncias;
            ViewData["mp"] = await AtendidasPorMP();

            ViewData["total"] = denuncias.Count();

            return View();
        }

        [Authorize(Roles = "Root")]
        public async Task<IActionResult> ReporteEncuestas(DateTime fecha, DateTime fecha2)
        {
            List<AuxEncuesta> reporte = new List<AuxEncuesta>();
            DateTime aux_f = Convert.ToDateTime("0001-01-01");

            if (fecha.Date != aux_f.Date && fecha2.Date != aux_f.Date)
            {
                List<AuxEncuesta> auxEncuestas = new List<AuxEncuesta>();

                var encuestas = await _contextUAT.Encuesta.ToListAsync();

                foreach (var item in encuestas)
                {
                    var denuncia = _contextUAT.Denuncia
                        .Where(x => x.Id == item.NumeroDenuncia && (x.AltaSistema.Date >= fecha.Date && x.AltaSistema.Date <= fecha2.Date))
                        .FirstOrDefault();

                    if (denuncia != null)
                    {
                        var dir = await _contextUAT.DireccionDenuncia
                        .Include(x => x.Colonia)
                            .ThenInclude(x => x.Municipio)
                        .Where(x => x.DenunciaId == denuncia.Id)
                        .FirstOrDefaultAsync();

                        AuxEncuesta obj = new AuxEncuesta
                        {
                            IdMunicipio = dir.Colonia.Municipio.Id,
                            Municipio = dir.Colonia.Municipio.Nombre,
                            Respuesta = item.Respuesta
                        };
                        auxEncuestas.Add(obj);
                    }                   
                }

                auxEncuestas = auxEncuestas.OrderBy(x => x.IdMunicipio).ToList();               

                Guid auxM = auxEncuestas[0].IdMunicipio;
                string auxNomM = auxEncuestas[0].Municipio;
                long auxSi = 0;
                long auxNo = 0;

                foreach (var item in auxEncuestas)
                {
                    if (item.IdMunicipio != auxM)
                    {
                        var obj = new AuxEncuesta
                        {
                            IdMunicipio = auxM,
                            Municipio = auxNomM,
                            TotalSi = auxSi,
                            TotalNo = auxNo
                        };
                        reporte.Add(obj);
                        auxM = item.IdMunicipio;
                        auxNomM = item.Municipio;
                        auxNo = 0;
                        auxSi = 0;
                    }

                    if (item.Respuesta)
                    {
                        auxSi++;
                    }
                    else
                    {
                        auxNo++;
                    }
                }

                return View(reporte.OrderBy(x => x.Municipio).ToList());
            }
            return View(reporte);
        }

        [Authorize(Roles = "Root")]
        public List<TotalKD> CuentaDenunciasPorKiosco(List<Denuncia> denuncias)
        {
            var respuesta = new List<TotalKD>();

            if (denuncias.Count() > 0)
            {
                int contador = 0;
                long id_aux = 0;
                string k = "";
                foreach (var item in denuncias)
                {
                    if (id_aux == 0)
                    {
                        id_aux = item.BitaKioscoId;
                        k = item.BitaKiosco.Nombre;
                    }
                    else if (id_aux != item.BitaKioscoId)
                    {
                        var obj = new TotalKD
                        {
                            Nombre = k,
                            Total = contador
                        };
                        respuesta.Add(obj);
                        id_aux = item.BitaKioscoId;
                        k = item.BitaKiosco.Nombre;
                        contador = 0;
                    }
                    contador++;
                }

                var obj1 = new TotalKD
                {
                    Nombre = k,
                    Total = contador
                };
                respuesta.Add(obj1);
            }

            return respuesta.OrderByDescending(x => x.Total).ToList();
        }

        [Authorize(Roles = "Root")]
        public async Task<IActionResult> VistaMP(long mpId, string fecha, bool ss = true)
        {
            ViewData["mps"] = await _contextUAT.MP
                .Include(x => x.UR)
                .OrderBy(x => x.UR.Nombre).ToListAsync();

            ViewData["mpId"] = mpId;

            if (mpId != 0)
            {
                var aux = _contextUAT.MP.Find(mpId);
                ViewData["mp"] = aux.Nombre + " " + aux.PrimerApellido + " " + aux.SegundoApellido;
            }
            
            List<Denuncia> denuncias = new List<Denuncia>();
            if (ss)
            {
               denuncias = await _contextUAT.Denuncia
                .Include(x => x.BitaKiosco)
                .Include(x => x.Delito)
                .Where(x => x.Paso == 3)
                .OrderBy(x => x.Id)
                .Where(x => x.MPId == mpId && x.SolucionId == null)
                .ToListAsync();
            }
            else
            {
                denuncias = await _contextUAT.Denuncia
                .Include(x => x.BitaKiosco)
                .Include(x => x.Delito)
                .Where(x => x.Paso == 3)
                .OrderBy(x => x.FechaSolucion)
                .Where(x => x.MPId == mpId && x.SolucionId != null)
                .Take(20)
                .ToListAsync();
            }

            ViewData["ss"] = !ss;

            return View(denuncias);
        }

        [Authorize(Roles = "Root")]
        public async Task<List<Denuncia>> PorFecha(string fecha)
        {
                var denuncias = await _contextUAT.Denuncia
                .Include(x => x.BitaKiosco)
                .Include(x => x.Delito)
                .Where(x => x.Paso == 3)
                .OrderBy(x => x.BitaKioscoId)
                .Where(x => x.AltaSistema.ToString("yyyy-MM-dd") == fecha)
                .ToListAsync();

            return denuncias;
        }

        [Authorize(Roles = "Root")]
        public async Task<List<Denuncia>> PorRangoFecha(string fecha, string fecha2)
        {
            DateTime fechaI = Convert.ToDateTime(fecha);
            DateTime fechaF = Convert.ToDateTime(fecha2);

            var denuncias = await _contextUAT.Denuncia
            .Include(x => x.BitaKiosco)
            .Include(x => x.Delito)
            .Where(x => x.Paso == 3)
            .OrderBy(x => x.BitaKioscoId)
            .Where(x => x.AltaSistema.Date >= fechaI.Date && x.AltaSistema.Date <= fechaF.Date)
            .ToListAsync();

            return denuncias;
        }

        [Authorize(Roles = "Root")]
        public async Task<List<MP>> AtendidasPorMP()
        {

            var mpAux= await _contextUAT.MP
                .Include(x => x.UR)
                    .ThenInclude(x => x.Region)
                .Where(x => x.Activo == true)
                .OrderBy(x => x.UR.Region.Nombre)
                .ToArrayAsync();

            List<MP> mps = new List<MP>();

            foreach (var item in mpAux)
            {
                List<Denuncia> obj = await _contextUAT.Denuncia
                    .Where(x => x.MPId == item.Id && x.SolucionId != null)
                    .ToListAsync();

                MP mp = item;
                mp.Denuncia = obj;
                mps.Add(mp);
            }

            return mps;
        }

        [Authorize(Roles = "Root")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ViewData["mps"] = await _contextUAT.MP.Where(x => x.Activo == true).Include(x => x.UR).OrderBy(x => x.Nombre).ToListAsync();

            var denuncia = await _contextUAT.Denuncia
                .Include(x => x.Victima)
                    .ThenInclude(x => x.Genero)
                .Include(x => x.Victima)
                    .ThenInclude(x => x.Escolaridad)
                .Include(x => x.Victima)
                    .ThenInclude(x => x.DireccionVictima)
                        .ThenInclude(x => x.Colonia)
                            .ThenInclude(x => x.Municipio)
                                .ThenInclude(x => x.Estado)
                .Include(x => x.Danio)
                .Include(x => x.Delito)
                .Include(x => x.DireccionDenuncia)
                    .ThenInclude(x => x.Colonia)
                        .ThenInclude(x => x.Municipio)
                            .ThenInclude(x => x.Estado)
                .Include(x => x.Responsable)
                    .ThenInclude(x => x.Genero)
                .Include(x => x.Responsable)
                    .ThenInclude(x => x.DescResponsable)
                .Include(x => x.Responsable)
                    .ThenInclude(x => x.DireccionResponsable)
                        .ThenInclude(x => x.Colonia)
                            .ThenInclude(x => x.Municipio)
                                .ThenInclude(x => x.Estado)
                .Include(x => x.Solucion)
                .Include(x => x.MP)
                    .ThenInclude(x => x.UR)
                        .ThenInclude(x => x.Region)
                .Include(x => x.BitaKiosco)
                .FirstOrDefaultAsync(m => m.Id == id);

            return View(denuncia);
        }

        [Authorize(Roles = "Root")]
        public async Task<IActionResult> ReasignarMP(long denunciaId, long mpId)
        {
            Denuncia denuncia = await _contextUAT.Denuncia.FindAsync(denunciaId);
            MP mpActual = await _contextUAT.MP.FindAsync(denuncia.MPId);
            MP mpNuevo = await  _contextUAT.MP.FindAsync(mpId);

            using (var transaction = _contextUAT.Database.BeginTransaction())
            {
                try
                {
                    denuncia.MPId = mpId;
                    _contextUAT.Update(denuncia);
                    await _contextUAT.SaveChangesAsync();

                    mpActual.Stock = mpActual.Stock - 1;
                    _contextUAT.Update(mpActual);
                    await _contextUAT.SaveChangesAsync();

                    mpNuevo.Stock = mpNuevo.Stock + 1;
                    _contextUAT.Update(mpNuevo);
                    await _contextUAT.SaveChangesAsync();

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                }
            }

                return Redirect("~/Root/VistaMP?mpId=" + mpId);
        }

        [Authorize(Roles = "Root")]
        public async Task<IActionResult> ReasignarDenuncias(long mpId, long mpId2, List<long> denunciasId)
        {
            int contador = denunciasId.Count();

            if (contador > 0 && mpId2 != 0)
            {
                var denuncias = new List<Denuncia>();
                foreach (var item in denunciasId)
                {
                    var denuncia = await _contextUAT.Denuncia.FindAsync(item);
                    denuncia.MPId = mpId2;
                    denuncias.Add(denuncia);
                }

                MP mpActual = await _contextUAT.MP.FindAsync(mpId);
                MP mpNuevo = await _contextUAT.MP.FindAsync(mpId2);

                mpActual.Stock = mpActual.Stock - contador;
                mpNuevo.Stock = mpNuevo.Stock + contador;

                using (var transaction = _contextUAT.Database.BeginTransaction())
                {
                    try
                    {
                        _contextUAT.UpdateRange(denuncias);
                        await _contextUAT.SaveChangesAsync();

                        _contextUAT.Update(mpActual);
                        await _contextUAT.SaveChangesAsync();

                        _contextUAT.Update(mpNuevo);
                        await _contextUAT.SaveChangesAsync();

                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        //transaction.Rollback();
                    }
                }
            }

            return Redirect("~/Root/VistaMP?mpId=" + mpId);
        }

        [Authorize(Roles = "Root")]
        public IActionResult LiberaToken(long denunciaId)
        {
            _subProceso.ActualizaToken(denunciaId, "0000");

            return Redirect("~/Denuncias");
        }

        [Authorize(Roles = "Root")]
        public async Task<IActionResult> VistaCitasMP()
        {
            return View(await _contextUAT.MP.OrderBy(x => x.Nombre).ToListAsync());
        }

        [Authorize(Roles = "Root,AEI")]
        public async Task<IActionResult> EstadisticaObjetivo(DateTime fecha, DateTime fecha2)
        {
            DateTime aux_fecha = Convert.ToDateTime("0001-01-01");

            int si = 0;
            int no = 0;
            int asignadas = 0;
            int sinAsignar = 0;

            if (fecha != aux_fecha && fecha2 != aux_fecha)
            {
                var denuncias = await _contextUAT.Denuncia
                    .Where(x => x.Paso == 3 && (x.AltaSistema.Date >= fecha.Date && x.AltaSistema.Date <= fecha2.Date))
                    .ToListAsync();

                foreach (var item in denuncias)
                {
                    var aux = await _contextUAT.Encuesta.Where(x => x.NumeroDenuncia == item.Id).FirstOrDefaultAsync();
                    if (aux != null)
                    {
                        if (aux.Respuesta)
                        {
                            si++;
                        }
                        else
                        {
                            no++;
                        }
                    }
                }
             
                asignadas = denuncias.Where(x => x.MPId != null).Count();
                sinAsignar = denuncias.Where(x => x.MPId == null).Count();
            }
            else
            {
                si = await _contextUAT.Encuesta.Where(x => x.Respuesta == true).CountAsync();
                no = await _contextUAT.Encuesta.Where(x => x.Respuesta == false).CountAsync();

                asignadas = await _contextUAT.Denuncia.Where(x => x.Paso == 3 && x.MPId != null).CountAsync();
                sinAsignar = await _contextUAT.Denuncia.Where(x => x.Paso == 3 && x.MPId == null).CountAsync();
            }

            EstadisticaObjetivos obj = new EstadisticaObjetivos
            {
                EncuestaSi = si,
                EncustaNo = no,
                EncuestaTotal = si + no,
                PorcientoSi = _subProceso.GetPorciento(Convert.ToDouble(si), Convert.ToDouble(si+no)),
                PorcientoNo = _subProceso.GetPorciento(Convert.ToDouble(no), Convert.ToDouble(si+no)),
                DenunciasAsignadas = asignadas,
                DenunciasSinAsignar = sinAsignar,
                DenunciasTotal = asignadas + sinAsignar,
                PorcientoAsignadas = _subProceso.GetPorciento(Convert.ToDouble(asignadas), Convert.ToDouble(asignadas+sinAsignar)),
                PorcientoSinAsignar = _subProceso.GetPorciento(Convert.ToDouble(sinAsignar), Convert.ToDouble(asignadas+sinAsignar))
            };

            return View(obj);
        }

        public IActionResult ConfirmaLiberarCodigo(long id)
        {
            return View(id);
        }

    }
}