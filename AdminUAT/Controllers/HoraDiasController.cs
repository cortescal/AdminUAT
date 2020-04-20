using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AdminUAT.Data;
using AdminUAT.Models.AgendaUAT;
using Microsoft.AspNetCore.Authorization;
using AdminUAT.Models.AgendaUAT.ViewModelAgenda;
using Microsoft.AspNetCore.Http;

namespace AdminUAT.Controllers
{
    [Authorize(Roles = "Root")]
    public class HoraDiasController : Controller
    {
        private readonly AgendaDbContext _context;
        private readonly NewUatDbContext _uatContext;

        public HoraDiasController(AgendaDbContext context, NewUatDbContext uatContext)
        {
            _context = context;
            _uatContext = uatContext;
        }

        private DateTime BuscarLunesPasado(DateTime currentDate)
        {
            var dayOfWeek = currentDate.DayOfWeek;
            int differenceDay = dayOfWeek - DayOfWeek.Monday;
            return currentDate.AddDays(-differenceDay);
        }

        // GET: HoraDias
        public async Task<IActionResult> Index(long? mpId)
        {
            var dias = (from d in _context.Dia
                        where d.Activo == true
                        select new DiaHorario
                        {
                            IdDias = d.Id,
                            nombreDias = d.Nombre
                        }).ToList();
            var horas = (from h in _context.Hora
                         orderby h.CampoHora ascending
                         where h.Activo == true
                         select new HoraHorario
                         {
                             IdHoras = h.Id,
                             campoHora = h.CampoHora
                         }).ToList();
            var horario = new ModelHorario { dias = dias, horas = horas };

            var combinaciones = (from d in _context.Dia
                                 from h in _context.Hora
                                 orderby h.CampoHora ascending
                                 where d.Activo == true && h.Activo == true
                                 select new Horario
                                 {
                                     idDia = d.Id,
                                     idHora = h.Id,
                                     activo = false
                                 }).Distinct().ToList();

            var validos = (from x in _context.HoraDia
                           where x.Activo == true && x.MP == mpId
                           select new Horario
                           {
                               idDia = x.DiaId,
                               idHora = x.HoraId,
                               activo = x.Activo
                           }).ToList();

            for (int i = 0; i <= combinaciones.Count - 1; i++)
            {
                for (int j = 0; j <= validos.Count - 1; j++)
                {
                    if (combinaciones[i].idDia == validos[j].idDia && combinaciones[i].idHora == validos[j].idHora)
                    {
                        combinaciones[i].activo = true;
                    }
                }
            }

            var model = new Horarios
            {
                horario = combinaciones,
                ModelHorario = horario,
                idMP = mpId
            };

            ViewData["mps"] = await _uatContext.MP.OrderBy(x => x.Nombre).ToListAsync();
            ViewData["letrero"] = 0;

            var mp = await _uatContext.MP.FindAsync(mpId);
            var days = new List<Dia>();
            var mps = await _uatContext.MP.FindAsync(mpId);

            if (mps != null)
            {
                ViewData["mp"] = mps;

                days = await _context.Dia
                    .Where(x => x.Activo == true)
                    .OrderBy(x => x.Numero)
                    .ToListAsync();

                int bandera = 0;
                foreach (var item in days)
                {
                    var horary = await _context.HoraDia
                        .Include(x => x.Hora)
                        .Where(x => x.MP == mpId && x.DiaId == item.Id)
                        .OrderBy(x => x.Hora.CampoHora)
                        .ToListAsync();

                    item.HoraDia = horary;

                    if (horary.Count() > 0)
                    {
                        bandera++;
                    }
                }

                if (bandera == 0)
                {
                    ViewData["letrero"] = 1;
                }
            }
            else if (mpId == null)
            {
                ViewData["letrero"] = -1;
            }
            else
            {
                ViewData["letrero"] = 2; //no existe id MP
            }
            return View(model);
        }

        [HttpPost]
        [Route("/modificaHorario", Name = "modificaHorario")]
        public async Task<IActionResult> modificaHorario(long idMP,int idDia, int idHora,bool activo)
        {
            var mpActivo = await _uatContext.MP.FindAsync(idMP);
            var mp = await _context.HoraDia.AnyAsync(x => x.MP == idMP);
            if(mpActivo!=null) 
            { //Existe mp
                if (mp == true)
                {//Mp tiene algun horario registrado
                    var verifica = (from i in _context.HoraDia
                                    where i.DiaId == idDia && i.HoraId == idHora && i.MP == idMP
                                    select i).ToList();
                    
                    if (verifica.Count!=0)
                    {//La hora y el dia existen
                        var id = (from i in _context.HoraDia
                                  where i.DiaId == idDia && i.HoraId == idHora && i.MP == idMP && i.Activo == activo
                                  select i).First();
                        if (id.Activo == true)
                            id.Activo = false;
                        else
                            id.Activo = true;
                        _context.HoraDia.Update(id);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {//No hay hora y dia registrado
                        var obj = new HoraDia
                        {
                            MP = idMP,
                            Activo = true,
                            HoraId = idHora,
                            DiaId = idDia,
                            AltaSistema = DateTime.Now
                        };
                        await _context.AddAsync(obj);
                        await _context.SaveChangesAsync();
                    }
                }
                else //No hay MP en base de datos pero si existe MP
                {
                    var obj = new HoraDia
                    {
                        MP = idMP,
                        Activo = true,
                        HoraId = idHora,
                        DiaId = idDia,
                        AltaSistema = DateTime.Now
                    };
                    await _context.AddAsync(obj);
                    await _context.SaveChangesAsync();
                }
            }

            
            var envia = (from i in _context.HoraDia
                      where i.DiaId == idDia && i.HoraId == idHora && i.MP == idMP
                      select i).First();
            return Ok(envia);
        }

        [HttpPost]
        [Route("/modificaFechas", Name = "modificaFechas")]
        public ActionResult modificaFechas(IFormCollection form)
        {
            DateTime hoy = DateTime.Now;
            DateTime ayer = DateTime.Now.AddDays(-1);

            string _fechaInicio = form["fechaInicio"];
            string _fechaFinal = form["fechaFinal"];
            string _idMP = form["idMP"];
            if (_idMP == "")
                _idMP = "0";
            if (_fechaInicio == "")
                _fechaInicio = ayer.ToString();
            if (_fechaFinal == "")
                _fechaFinal = ayer.ToString();
            

            var idMP = (long)Convert.ToDouble(_idMP);
            var fechaInicio = Convert.ToDateTime(_fechaInicio);
            var fechaFinal = Convert.ToDateTime(_fechaFinal);

            int r = DateTime.Compare(hoy, fechaInicio);
            int result = DateTime.Compare(fechaInicio, fechaFinal);

            var mp = _context.HoraDia.Any(x => x.MP == idMP);
            if(mp==true&&(result!=1&&r!=1))
            {
                var obj = new HoraDia
                {
                    FechaInicio = fechaInicio,
                    FechaFinal = fechaFinal
                };

                var some = _context.HoraDia.Where(i => i.MP == idMP).ToList();
                some.ForEach(a => {
                    a.FechaInicio = fechaInicio;
                    a.FechaFinal = fechaFinal;
                });

                _context.SaveChanges();
            }

            return Redirect("~/HoraDias?mpId=" + idMP);
        }
        // GET: HoraDias/Create
        public IActionResult Create(long mpId)
        {
            ViewData["MpId"] = mpId;
            ViewData["DiaId"] = _context.Dia.Where(x => x.Activo == true).OrderBy(x => x.Numero).ToList();
            ViewData["HoraId"] = _context.Hora.Where(x => x.Activo == true).OrderBy(x => x.CampoHora).ToList();
            return View();
        }

        // POST: HoraDias/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(long mpId, int dia, List<int> horas)
        {
            var horaDia = new List<HoraDia>();
            long id_aux = 0;

            foreach (var item in horas)
            {
                var aux = await _context.HoraDia
                    .Where(x => x.MP == mpId && x.HoraId == item && x.DiaId == dia)
                    .FirstOrDefaultAsync();
                if (aux == null)
                {
                    var obj = new HoraDia
                    {
                        Id = id_aux,
                        MP = mpId,
                        Activo = true,
                        HoraId = item,
                        DiaId = dia,
                        AltaSistema = DateTime.Now
                    };

                    horaDia.Add(obj);
                }
            }

            _context.AddRange(horaDia);
            await _context.SaveChangesAsync();

            return Ok(new { ruta = $"{Request.Scheme}://{Request.Host.Value}{Request.PathBase.Value}/" });
        }

        // GET: HoraDias/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var horaDia = await _context.HoraDia.FindAsync(id);
            if (horaDia == null)
            {
                return NotFound();
            }

            horaDia.Activo = !(horaDia.Activo);
            _context.Update(horaDia);
            await _context.SaveChangesAsync();

            return Redirect("~/HoraDias?mpId=" + horaDia.MP);
        }

        private bool HoraDiaExists(long id)
        {
            return _context.HoraDia.Any(e => e.Id == id);
        }
    }
}
