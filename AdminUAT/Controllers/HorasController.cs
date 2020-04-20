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

namespace AdminUAT.Controllers
{
    [Authorize(Roles = "Root")]
    public class HorasController : Controller
    {
        private readonly AgendaDbContext _context;

        public HorasController(AgendaDbContext context)
        {
            _context = context;
        }

        // GET: Horas
        public async Task<IActionResult> Index()
        {
            return View(await _context.Hora.OrderBy(x => x.CampoHora).ToListAsync());
        }

        // GET: Horas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Horas/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CampoHora,Activo")] Hora hora)
        {
            if (ModelState.IsValid)
            {
                _context.Add(hora);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(hora);
        }

        // GET: Horas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hora = await _context.Hora.FindAsync(id);
            if (hora == null)
            {
                return NotFound();
            }
            return View(hora);
        }

        // POST: Horas/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CampoHora,Activo")] Hora hora)
        {
            if (id != hora.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hora);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HoraExists(hora.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(hora);
        }

        private bool HoraExists(int id)
        {
            return _context.Hora.Any(e => e.Id == id);
        }
    }
}
