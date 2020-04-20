using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminUAT.Data;
using AdminUAT.Dependencias;
using AdminUAT.Models;
using AdminUAT.Models.ExtraModels.Agenda;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminUAT.Controllers
{
    [Authorize(Roles = "MP, Root")]
    public class AgendaController : Controller
    {
        public readonly AgendaDbContext _agendaContext;
        public readonly UserManager<ApplicationUser> _userManager;
        public readonly NewUatDbContext _uatContext;
        public readonly ISubProceso _subProceso;

        public AgendaController(AgendaDbContext agendaContext, UserManager<ApplicationUser> userManager,
            NewUatDbContext uatContext, ISubProceso subProceso)
        {
            _agendaContext = agendaContext;
            _userManager = userManager;
            _uatContext = uatContext;
            _subProceso = subProceso;
        }
       
        [HttpGet]
        public async Task<IActionResult> Index(long? id, bool  recarga = false)
        {
            var user = await _userManager.GetUserAsync(User);

            long? id_aux = User.IsInRole("Root") ? id : user.MatchMP;
            ViewData["_partial"] = User.IsInRole("Root") ? true : false;
            ViewData["mpId"] = id_aux;

            if (recarga)
            {
                ViewData["_partial"] = recarga;
            }

            var citas = await _agendaContext.Cita
                .Include(x => x.HoraDia)
                    .ThenInclude(x => x.Dia)
                .Include(x => x.HoraDia)
                    .ThenInclude(x => x.Hora)
                .Where(x => x.MP == id_aux && x.Activo == true && x.Asistencia == (-1))
                .OrderBy(x => x.Dia)
                .ToListAsync();

            var lista = new List<AuxCita>();

            if (citas.Count() > 0)
            {
                var objCita = citas[0];
                var auxHorario = new List<AuxHorario>();

                foreach (var item in citas)
                {
                    if (item.Dia != objCita.Dia)
                    {
                        var obj = new AuxCita
                        {
                            Fecha = objCita.Dia,
                            Dia = objCita.HoraDia.Dia.Nombre,
                            AuxHorario = auxHorario.OrderBy(x => x.Hora).ToList()
                        };

                        lista.Add(obj);
                        objCita = item;
                        auxHorario = new List<AuxHorario>();
                    }

                    var denuncia = await _uatContext.Denuncia.FindAsync(item.NumDenuncia);

                    var victima = await _uatContext.Victima
                        .Where(x => x.Email != "" && x.DenunciaId == item.NumDenuncia)
                        .FirstOrDefaultAsync();

                    var objHorario = new AuxHorario
                    {
                        CitaId = item.Id,
                        IdDenuncia = denuncia.Id,
                        Folio = denuncia.Expediente,
                        Denunciante = victima.Nombre + " " + victima.PrimerApellido + " " + victima.SegundoApellido,
                        Hora = item.HoraDia.Hora.CampoHora
                    };
                    auxHorario.Add(objHorario);
                }

                var obj1 = new AuxCita
                {
                    Fecha = objCita.Dia,
                    Dia = objCita.HoraDia.Dia.Nombre,
                    AuxHorario = auxHorario
                };

                lista.Add(obj1);
            }

            ViewData["ruta"] = $"{Request.Scheme}://{Request.Host.Value}{Request.PathBase.Value}/";

            return View(lista);
        }

        [HttpGet]
        public async Task<IActionResult> MiHorario(long mpId)
        {
            var user = await _userManager.GetUserAsync(User);

            long? id_aux = User.IsInRole("Root") ? mpId : user.MatchMP;

            ViewData["sin_horario"] = false;

            var dias = await _agendaContext.Dia
                    .Where(x => x.Activo == true)
                    .OrderBy(x => x.Numero)
                    .ToListAsync();

            int bandera = 0;
            foreach (var item in dias)
            {
                var horario = await _agendaContext.HoraDia
                    .Include(x => x.Hora)
                    .Where(x => x.MP == mpId && x.DiaId == item.Id && x.Activo == true)
                    .OrderBy(x => x.Hora.CampoHora)
                    .ToListAsync();

                item.HoraDia = horario;

                if (horario.Count() > 0)
                {
                    bandera++;
                }
            }

            if (bandera == 0)
            {
                ViewData["sin_horario"] = true;
            }

            return View(dias);
        }

        [HttpGet]
        public async Task<IActionResult> GetCodigo(long denunciaId)
        {
            if (User.IsInRole("Root"))
            {
                return Ok(new { token = _subProceso.AsignaToken(denunciaId) });
            }

            var user = await _userManager.GetUserAsync(User);
            if (User.IsInRole("MP") && _subProceso.MatchDenunciaMP(user.MatchMP, denunciaId) && _subProceso.ExpiroCita(denunciaId))
            {                
                return Ok(new { token = _subProceso.AsignaToken(denunciaId) });
            }

            return NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> AddAsistencia(long citaId, int asistencia)
        {
            var cita = await _agendaContext.Cita.FindAsync(citaId);

            if (User.IsInRole("MP"))
            {
                var user = await _userManager.GetUserAsync(User);

                if (!(_subProceso.MatchDenunciaMP(user.MatchMP, cita.NumDenuncia)))
                {
                    return NotFound();
                }
            }
            
            cita.Asistencia = asistencia;
            _agendaContext.Update(cita);
            await _agendaContext.SaveChangesAsync();

            return Redirect("~/Agenda/Index?id="+ cita.MP +"&recarga=true");
        }
    }
}