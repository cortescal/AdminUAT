using AdminUAT.Data;
using AdminUAT.Dependencias;
using AdminUAT.Models;
using AdminUAT.Models.AdminUat;
using AdminUAT.Models.Denuncias;
using AdminUAT.Models.ExtraModels;
using AdminUAT.Models.MinisterioPublico;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace AdminUAT.Controllers
{
    [Authorize(Roles = "MP, Root, AEI, FiscMet, FiscReg")]
    public class DenunciasController : Controller
    {
        private readonly NewUatDbContext _context;
        private UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbcontext;
        private readonly ISubProceso _subProceso;
        private readonly IQueryDenuncias _queryDenuncias;
        private readonly IEnvioCorreo _correo;

        public DenunciasController(NewUatDbContext context, UserManager<ApplicationUser> userManager,
            ApplicationDbContext dbcontext, ISubProceso subProceso, IQueryDenuncias queryDenuncias , IEnvioCorreo correo)
        {
            _context = context;
            _userManager = userManager;
            _dbcontext = dbcontext;
            _subProceso = subProceso;
            _queryDenuncias = queryDenuncias;
            _correo = correo;
        }

        // GET: Denuncias
        public async Task<IActionResult> Index(long id, DateTime fecha, string palabra, long kiosco, bool opc = true)
        {
            var lista = new List<Denuncia>();
            ViewData["opc"] = opc == true ? "true" : "false";
            ViewBag.alerta = true;
            ViewBag.ruta = $"{Request.Scheme}://{Request.Host.Value}{Request.PathBase.Value}";

            if (User.IsInRole("MP"))
            {
                var user = await _userManager.GetUserAsync(User);
                lista = await SolicitudMP(opc, fecha, id, user.MatchMP);
            }
            else
            {
                lista = SolicitudGeneral(fecha, opc, id, palabra, kiosco);
            }

            return View(lista);
        }

        [Authorize(Roles = "Root, MP")]
        private async Task<List<Denuncia>> SolicitudMP(bool opc, DateTime fecha, long id, long mpId)
        {
            var lista = new List<Denuncia>();

            if (id != 0)
            {
                if (_subProceso.MatchDenunciaMP(mpId, id))
                {
                    lista = _queryDenuncias.DenunciaPorId(id);
                    ViewBag.total = lista.Count();
                }
                else
                {
                    ViewBag.alerta = false;
                }
                ViewBag.titulo = "Mi denuncia con folio " + id;
            }
            else if (fecha.ToString("dd-MM-yyyy") != "01-01-0001")
            {
                lista = _queryDenuncias.PorFecha(fecha, mpId);
                lista = opc == true ? lista.Where(x => x.SolucionId == null).ToList() : lista.Where(x => x.SolucionId != null).ToList();
                ViewBag.titulo = "Mis denuncias por fecha " + fecha.ToString("dd/MM/yyyy");
                ViewBag.total = lista.Count();
            }
            else
            {
                if (opc)
                {
                    lista = _queryDenuncias.DSS(mpId);
                    ViewBag.titulo = "Mis denuncias en espera de atención";
                    ViewBag.total = lista.Count();
                }
                else
                {
                    lista = _queryDenuncias.DCS(mpId);
                    ViewBag.titulo = "Mis denuncias atendidas";
                    var mp = await _context.MP.FindAsync(mpId);
                    ViewBag.total = "20/" + mp.Resuelto;
                }
            }

            return lista;
        }

        [Authorize(Roles = "Root, AEI, FiscMet, FiscReg")]
        private List<Denuncia> SolicitudGeneral(DateTime fecha, bool opc, long id, string palabra, long kiosco)
        {
            ViewBag.fecha = fecha.ToString("yyyy-MM-dd");
            ViewBag.opc = !opc;

            var lista = new List<Denuncia>();

            if (palabra != null && fecha.ToString("dd-MM-yyyy") != "01-01-0001")
            {
                lista = _queryDenuncias.PorPalabraYFecha(palabra, fecha);
                ViewBag.titulo = "Denuncias con la palabra \"" + palabra + "\" del dia " + fecha.ToString("dd-MM-yyyy");
            }
            else if (kiosco != 0 && fecha.ToString("dd-MM-yyyy") != "01-01-0001")
            {
                lista = _queryDenuncias.PorKioscoYFecha(kiosco, fecha);
                var aux = _context.BitaKiosco.Find(kiosco);
                ViewBag.titulo = "Denuncias del kiosco \"" + aux.Nombre + "\" del dia " + fecha.ToString("dd-MM-yyyy");
            }
            else if (id != 0)
            {
                if (User.IsInRole("FiscMet"))
                {
                    if (_subProceso.AccesoDenunciaFM(id))
                    {
                        lista = _queryDenuncias.DenunciaPorId(id);
                    }
                    else
                    {
                        ViewBag.alerta = false;
                    }
                }
                else if (User.IsInRole("FiscReg"))
                {
                    if (_subProceso.AccesoDenunciaFR(id))
                    {
                        lista = _queryDenuncias.DenunciaPorId(id);
                    }
                    else
                    {
                        ViewBag.alerta = false;
                    }
                }
                else
                {
                    lista = _queryDenuncias.DenunciaPorId(id);
                }
                ViewBag.titulo = "Denuncia con folio " + id;
            }
            else if (palabra != null)
            {
                lista = _queryDenuncias.PorPalabra(palabra);
                ViewBag.titulo = "Denuncias con la palabra \"" + palabra + "\"";
            }
            else if (kiosco != 0)
            {
                lista = _queryDenuncias.PorKiosco(kiosco);
                var aux = _context.BitaKiosco.Find(kiosco);
                ViewBag.titulo = "Denuncias de kiosco " + aux.Nombre;
            }
            else
            {
                fecha = fecha.ToString("dd-MM-yyyy") != "01-01-0001" ? fecha : DateTime.Now;
                lista = _queryDenuncias.AEITodas(fecha, opc);
                ViewBag.titulo = opc == true ? "Denuncias recibidas del " + fecha.ToString("dd-MM-yyyy") : "Denuncias sin concluir del " + fecha.ToString("dd-MM-yyyy");
            }

            if (User.IsInRole("FiscMet"))
            {
                lista = lista.Where(x => x.MP.UR.RegionId == 6).ToList();
                ViewBag.kioscos = _context.BitaKiosco.Where(x => x.UR.RegionId == 6).OrderBy(x => x.Nombre).ToList();
            }
            else if (User.IsInRole("FiscReg"))
            {
                lista = lista.Where(x => x.MP.UR.RegionId != 6).ToList();
                ViewBag.kioscos = _context.BitaKiosco.Where(x => x.UR.RegionId != 6).OrderBy(x => x.Nombre).ToList();
            }
            else
            {
                ViewBag.kioscos = _context.BitaKiosco.Where(x => x.Activo == true).OrderBy(x => x.Nombre).ToList();
            }

            ViewBag.total = lista.Count();

            return lista;
        }

        // GET: Denuncias/Details/5
        public async Task<IActionResult> Details(long id)
        {
            if (!await ConfirmaPermisos(id))
            {
                return Redirect("~/Identity/Account/AccessDenied");
            }

            ViewData["SolucionId"] = new SelectList(_context.Set<Solucion>(), "Id", "Nombre");

            var denuncia = await _context.Denuncia
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

        private async Task<bool> ConfirmaPermisos(long id)
        {
            if (User.IsInRole("MP"))
            {
                var usu = await _userManager.GetUserAsync(User);
                return _subProceso.MatchDenunciaMP(usu.MatchMP, id);
            }
            else if (User.IsInRole("FiscReg"))
            {
                return _subProceso.AccesoDenunciaFR(id);
            }
            else if (User.IsInRole("FiscMet"))
            {
                return _subProceso.AccesoDenunciaFM(id);
            }
            else if (User.IsInRole("Root") || User.IsInRole("AEI"))
            {
                return true;
            }

            return false;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Root, MP")]
        public async Task<IActionResult> Edit(long id, [Bind("SolucionId,NotaSolucion,Codigo")] Comentario denuncia)
        {
            if (denuncia.SolucionId == null || denuncia.NotaSolucion == null || denuncia.Codigo == null)
            {
                return Ok(new { msj = "El campo código, solución y/o nota esta vacios.", error = true });
            }

            Denuncia obj = await _context.Denuncia.FindAsync(id);

            if (obj == null)
            {
                return Ok(new { msj = "Denuncia no encontrada en los registros.", error = true });
            }

            if (User.IsInRole("MP"))
            {
                var objToken = _dbcontext.Token.Where(x => x.Denuncia == id).FirstOrDefault();

                if (objToken == null)
                {
                    return Ok(new { msj = "Aun no envias un código al denunciante.", error = true });
                }

                if (!(objToken.Codigo == denuncia.Codigo))
                {
                    return Ok(new { msj = "Código incorrecto", error = true });
                }
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var mp = await _context.MP.FindAsync(obj.MPId);

                    obj.FechaSolucion = DateTime.Now;
                    obj.NotaSolucion = denuncia.NotaSolucion;
                    obj.SolucionId = denuncia.SolucionId;

                    _context.Update(obj);
                    await _context.SaveChangesAsync();

                    mp.Resuelto = mp.Resuelto + 1;
                    _context.Update(mp);
                    await _context.SaveChangesAsync();

                    transaction.Commit();
                }
                catch (DbUpdateConcurrencyException)
                {
                    transaction.Rollback();

                    if (!DenunciaExists(obj.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }


            var solucion = await _context.Solucion.FindAsync(obj.SolucionId);

            return Ok(new { nota = obj.NotaSolucion, fecha = ((DateTime)obj.FechaSolucion).ToString("dd/MM/yyyy hh:mm:ss tt"), solucion = solucion.Nombre, msj = "Exito al guardar los cambios", error = false });
        }

        [Authorize(Roles = "Root, MP")]
        [HttpGet]
        public async Task<IActionResult> SendToken(long id)
        {
            var denunciante = await _context.Victima
                .Where(x => x.DenunciaId == id && x.Email != "")
                .FirstOrDefaultAsync();

            if (denunciante == null)
            {
                return Ok(new { resp = false, email = "" });
            }

            try
            {
                var value = _subProceso.AsignaToken(id);

                MailMessage email = new MailMessage();
                email.To.Add(new MailAddress(denunciante.Email));
                email.Subject = "Codigo de ratificación";
                string name = denunciante.Nombre + " " + denunciante.PrimerApellido + " " + denunciante.SegundoApellido;
                email.Body = string.Format(
                    @"<center><b>Fiscalía General del Estado de Puebla</b></center>
                    <p>Estimado <b>{0}:</b></p>
                    <p>Este es el código de ratificación de tu denuncia, el cual sera solicitado por el Ministerio Público:</p>
                    <p><b>{1}</b></p>
                    <p>Estamos para servirle en la línea telefónica <b>222-211-7900</b></p>
                    <p>
                    Te recordamos nuestro domicilio:<br/>
                    Boulevard “Héroes del 5 de Mayo” esquina con Avenida 31 Oriente<br/>
                    Col. Ladrillera de Benítez, C.P. 72530<br/>
                    Puebla, Pue. MX.<br/>
                    </p>
                    ", name, value);
                email.IsBodyHtml = true;
                email.Priority = MailPriority.Normal;

                SmtpClient smtp = new SmtpClient();
                /*
                email.From = new MailAddress("uat.fiscalia.puebla.3@outlook.com");
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("uat.fiscalia.puebla.3@outlook.com", "Fge.2020**", "outlook.com");
                smtp.Host = "smtp-mail.outlook.com";
                smtp.TargetName = "STARTTLS/smtp-mail.outlook.com";
                smtp.Port = 25;
                smtp.EnableSsl = true;
                */
                email.From = new MailAddress("uat.fiscalia.puebla.7@gmail.com");
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("uat.fiscalia.puebla.7@gmail.com", "Fge.2020**");

                smtp.Send(email);
                email.Dispose();
            }
            catch (Exception ex)
            {
                //return Ok(new { resp = false, email = "" });
                var value = _subProceso.AsignaToken(id);

                MailMessage email = new MailMessage();
                email.To.Add(new MailAddress(denunciante.Email));
                email.Subject = "Codigo de ratificación";
                string name = denunciante.Nombre + " " + denunciante.PrimerApellido + " " + denunciante.SegundoApellido;
                email.Body = string.Format(
                    @"<center><b>Fiscalía General del Estado de Puebla</b></center>
                    <p>Estimado <b>{0}:</b></p>
                    <p>Este es el código de ratificación de tu denuncia, el cual sera solicitado por el Ministerio Público:</p>
                    <p><b>{1}</b></p>
                    <p>Estamos para servirle en la línea telefónica <b>222-211-7900</b></p>
                    <p>
                    Te recordamos nuestro domicilio:<br/>
                    Boulevard “Héroes del 5 de Mayo” esquina con Avenida 31 Oriente<br/>
                    Col. Ladrillera de Benítez, C.P. 72530<br/>
                    Puebla, Pue. MX.<br/>
                    </p>
                    ", name, value);
                email.IsBodyHtml = true;
                email.Priority = MailPriority.Normal;

                SmtpClient smtp = new SmtpClient();
                /*
                email.From = new MailAddress("uat.fiscalia.puebla.1@outlook.com");
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("uat.fiscalia.puebla.1@outlook.com", "Fge.2020**", "outlook.com");
                smtp.Host = "smtp-mail.outlook.com";
                smtp.TargetName = "STARTTLS/smtp-mail.outlook.com";
                smtp.Port = 25;
                smtp.EnableSsl = true;
                */
                email.From = new MailAddress("uat.fiscalia.puebla.8@gmail.com");
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("uat.fiscalia.puebla.8@gmail.com", "Fge.2020**");
                smtp.Send(email);
                email.Dispose();
            }

            return Ok(new { resp = true, email = denunciante.Email });
        }

        [Authorize(Roles = "Root")]
        [HttpPost]
        public async Task<IActionResult> ReenviarEmail(int id, int paso)
        {
            var titulo = "";
            string msj = "";
            if (paso==1)
            {
                titulo = "UAT@ - CONFIRMACIÓN DE CORREO ELECTRÓNICO";
                msj= $"{Request.Scheme}://fiscalia.puebla.gob.mx:8099/";
            }
            else if(paso==3)
            {
                var idMP = await _context.Denuncia.Where(x => x.Id == id).Select(x => x.MPId).FirstOrDefaultAsync();
                titulo = "UAT@ - AVISO DE RECEPCIÓN DE DENUNCIA";

                if (idMP > 0)
                {
                    var mp = await _context.MP
                        .Include(x => x.UR)
                        .ThenInclude(x => x.Region)
                    .Where(x => x.Id == idMP)
                    .FirstOrDefaultAsync();

                    msj = $"<b>Lic. {mp.Nombre} {mp.PrimerApellido} {mp.SegundoApellido}, Agente del Ministerio Público</b>, adscrito a la Fiscalía de Investigación {mp.UR.Region.Nombre}." +
                        $"<p><b>Datos de contacto</b></p>" +
                        $"<p>{mp.UR.Nota}</p>";
                }   
            }
            var respuesta = _correo.SendCorreo(titulo, paso, id, msj);
            return Ok(respuesta);
        }

        private bool DenunciaExists(long id)
        {
            return _context.Denuncia.Any(e => e.Id == id);
        }
    }
}
