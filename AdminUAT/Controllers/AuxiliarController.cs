using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminUAT.Data;
using AdminUAT.Models.Denuncias;
using AdminUAT.Models.MinisterioPublico;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AdminUAT.Controllers
{
    [Authorize(Roles = "Root")]
    //[AllowAnonymous]
    public class AuxiliarController : Controller
    {
        private readonly NewUatDbContext _context;

        public AuxiliarController(NewUatDbContext context)
        {
            _context = context;
        }
       
        public async Task<IActionResult> Index(long id)
        {
            var denuncia = await _context.Denuncia
                .Include(x => x.Solucion)
                .Include(x => x.MP)
                .Where(x => x.Id == id && x.Paso == 3 && x.MP.URId == 18)
                .FirstOrDefaultAsync();

            if (denuncia == null)
            {
                if (id != 0) { ViewData["alerta"] = "Sin acceso a denuncia " + id;  }
                return View();
            }

            ViewData["SolucionId"] = new SelectList(_context.Set<Solucion>(), "Id", "Nombre");

            return View(denuncia);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("SolucionId,NotaSolucion")] Denuncia denuncia)
        {
            if (denuncia.SolucionId == null || denuncia.NotaSolucion == null)
            {
                return Redirect("~/Auxiliar?id=" + id);
            }

            Denuncia obj = await _context.Denuncia.FindAsync(id);

            if (obj == null)
            {
                return NotFound();
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

            return Redirect("~/Auxiliar?id=" + id);
        }

        private bool DenunciaExists(long id)
        {
            return _context.Denuncia.Any(e => e.Id == id);
        }
    }
}