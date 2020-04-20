using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminUAT.Data;
using AdminUAT.Dependencias;
using AdminUAT.Models;
using AdminUAT.Models.AdminUat;
using AdminUAT.Models.ExtraModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminUAT.Controllers
{
    [Authorize(Roles = "Root,AEI,FiscMet,FiscReg")]
    public class AdminSoportesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISubProceso _subProceso;

        public AdminSoportesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ISubProceso subProceso)
        {
            _context = context;
            _userManager = userManager;
            _subProceso = subProceso;
        }

        public async Task<IActionResult> Index(bool viewPartial = false)
        {
            ViewData["ViewPartial"] = viewPartial;

            var user = await _userManager.GetUserAsync(User);

            var orden = new List<OrdenSoporte>();

            if (User.IsInRole("Root"))
            {
                orden = await _context.OrdenSoporte
                    .Include(x => x.TipoSoporte)
                    .Where(x => x.SolicitudCerrada != 0 && x.SolicitudCerrada != 1 && x.Activo == true)
                    .OrderByDescending(x => x.AltaSistema)
                    .ToListAsync();
            }
            else
            {
                orden = await _context.OrdenSoporte
                    .Include(x => x.TipoSoporte)
                    .Where(x => x.Usuario == user.Id && x.SolicitudCerrada != 0 && x.SolicitudCerrada != 2 && x.Activo == true)
                    .OrderByDescending(x => x.AltaSistema)
                    .ToListAsync();
            }

            ViewData["Ruta"] = $"{Request.Scheme}://{Request.Host.Value}{Request.PathBase.Value}";

            return View(ParserMisSolicitudes(orden));
        }

        public List<MisSolicitudes> ParserMisSolicitudes(List<OrdenSoporte> lista)
        {
            var solicitudes = new List<MisSolicitudes>();

            foreach (var item in lista)
            {
                var obj = new MisSolicitudes
                {
                    Id = item.Id,
                    Solicitud = item.TipoSoporte.Nombre,
                    AltaSistema = item.AltaSistema,
                    Atendido = item.Atendido,
                    NumNotificaciones = User.IsInRole("Root") ? _context.SeguimientoSoporte
                    .Where(x => (x.Visto != 1 && x.Visto != 0) && x.OrdenSoporteId == item.Id)
                    .Count() :
                    _context.SeguimientoSoporte
                    .Where(x => (x.Visto != 2 && x.Visto != 0) && x.OrdenSoporteId == item.Id)
                    .Count()
                };

                var user = _context.Users.Find(item.Usuario);
                obj.Usuario = user.Nombre + " " + user.PrimerApellido;
                solicitudes.Add(obj);
            }

            return solicitudes;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewData["TipoSoporte"] = await _context.TipoSoporte
                .Where(x => x.Activo == true)
                .OrderBy(x => x.Nombre)
                .ToListAsync();

            ViewData["Ruta"] = $"{Request.Scheme}://{Request.Host.Value}{Request.PathBase.Value}";

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(OrdenSoporte orden)
        {
            if (orden.Solicitud == null && orden.TipoSoporteId == 0)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var fecha = DateTime.Now;
            long aux_id = 0;

            orden.Id = aux_id;
            orden.Atendido = false;
            orden.FechaAtencion = fecha;
            orden.AltaSistema = fecha;
            orden.SolicitudCerrada = -1;
            orden.Activo = true;
            orden.Usuario = user.Id;

            _context.Add(orden);
            await _context.SaveChangesAsync();

            return Redirect("~/AdminSoportes/Index?viewPartial=true");
        }

        [HttpGet]
        public async Task<IActionResult> Notificaciones(long soporteId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (!User.IsInRole("Root"))
            {
                if (!(_subProceso.ValidaUsuarioSoporte(user.Id, soporteId)))
                {
                    return NotFound();
                }
            }

            var notificaciones = await _context.SeguimientoSoporte
                .Where(x => x.OrdenSoporteId == soporteId)
                .ToListAsync();

            var aux = new List<SeguimientoSoporte>();

            foreach (var item in notificaciones)
            {
                if (item.Visto != 0)
                {
                    int auxEstatus = GetEstatusSoporte(item.Visto);
                    if (auxEstatus != item.Visto)
                    {
                        item.Visto = auxEstatus;
                        aux.Add(item);
                    }                    
                }
            }

            if (aux.Count() > 0)
            {
                _context.UpdateRange(aux);
                await _context.SaveChangesAsync();
            }

            var soporte = await _context.OrdenSoporte.FindAsync(soporteId);
            var obj = new SeguimientoSoporte
            {
                OrdenSoporteId = soporte.Id,
                Usuario = soporte.Usuario,
                AltaSistema = soporte.AltaSistema,
                Comentario = soporte.Solicitud,
                Visto = 0
            };
            notificaciones.Add(obj);

            ViewData["yo"] = user.Id;
            ViewData["miNombre"] = user.Nombre + " " + user.PrimerApellido;

            if (User.IsInRole("Root"))
            {
                var userDos = await _context.Users.FindAsync(soporte.Usuario);
                ViewData["nombreDos"] = userDos.Nombre + " " + userDos.PrimerApellido;
            }
            else
            {
                ViewData["nombreDos"] = "Administrador";
            }

            return View(notificaciones.OrderByDescending(x => x.AltaSistema).ToList());
        }

        [HttpGet]
        public IActionResult MsjSeguimiento(long soporteId)
        {
            ViewData["UrlBase"] = $"{Request.Scheme}://{Request.Host.Value}{Request.PathBase.Value}";

            return View(soporteId);
        }

        [HttpPost]
        public async Task<IActionResult> AddSeguimiento(long soporteId, string comentario)
        {
            var user = await _userManager.GetUserAsync(User);

            if (!User.IsInRole("Root"))
            {
                if (!(_subProceso.ValidaUsuarioSoporte(user.Id, soporteId)) || comentario == null)
                {
                    return NotFound();
                }
            }

            var obj = new SeguimientoSoporte
            {
                OrdenSoporteId = soporteId,
                Usuario = user.Id,
                AltaSistema = DateTime.Now,
                Comentario = comentario,
                Visto = User.IsInRole("Root") ? 1 : 2
            };

            _context.Add(obj);
            await _context.SaveChangesAsync();

            var soporte = await _context.OrdenSoporte.FindAsync(soporteId);
            if (soporte.Atendido == true && soporte.SolicitudCerrada != -1)
            {
                soporte.SolicitudCerrada = -1;
                _context.Update(soporte);
                await _context.SaveChangesAsync();
            }

            return Redirect("~/AdminSoportes/Notificaciones?soporteId=" + soporteId);
        }

        [Authorize(Roles = "Root")]
        public IActionResult ConfirmaSoporteAtendido(long id)
        {
            var soporte = _context.OrdenSoporte.Find(id);

            if (soporte == null)
            {
                NotFound();
            }

            return View(id);
        }

        [Authorize(Roles = "Root")]
        public async Task<IActionResult> SoporteAtendido(long id)
        {
            var soporte = await _context.OrdenSoporte.FindAsync(id);

            if (soporte == null)
            {
                NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            soporte.Atendido = true;
            soporte.FechaAtencion = DateTime.Now;
            soporte.AtendioUsuario = user.Id;

            _context.Update(soporte);
            await _context.SaveChangesAsync();

            return Redirect("~/AdminSoportes/Index");
        }

        public IActionResult ConfirmaCerrarSoporte(long id)
        {
            var soporte = _context.OrdenSoporte.Find(id);

            if (soporte == null)
            {
                NotFound();
            }

            return View(id);
        }

        public async Task<IActionResult> CerrarSoporte(long id)
        {
            var soporte = await _context.OrdenSoporte.FindAsync(id);

            if (soporte == null)
            {
                NotFound();
            }

            soporte.SolicitudCerrada = GetEstatusSoporte(soporte.SolicitudCerrada);

            _context.Update(soporte);
            await _context.SaveChangesAsync();

            return Redirect("~/AdminSoportes/Index");
        }

        private int GetEstatusSoporte(int estatusActual)
        {
            int nuevoEstatus = estatusActual;
            var soyRoot = User.IsInRole("Root");
            
            if (soyRoot)
            {
                if (estatusActual == -1)
                {
                    nuevoEstatus = 1;
                }
                else if (estatusActual == 2)
                {
                    nuevoEstatus = 0;
                }
            }
            else
            {
                if (estatusActual == -1)
                {
                    nuevoEstatus = 2;
                }
                else if (estatusActual == 1)
                {
                    nuevoEstatus = 0;
                }
            }
            return nuevoEstatus;
        }
    }
}