using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminUAT.Data;
using AdminUAT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminUAT.Controllers
{
    [Authorize(Roles = "Root")]
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly NewUatDbContext _contextUAT;
        private UserManager<ApplicationUser> _userManager;

        public UsuariosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
            NewUatDbContext contextUAT)
        {
            _context = context;
            _userManager = userManager;
            _contextUAT = contextUAT;
        }

        public IActionResult Index()
        {
            var query = _context.Users
                .OrderBy(x=>x.Nombre)
                .ToList();

            return View(query);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var usuario = _context.Users.Find(id);
            usuario.Estatus = !usuario.Estatus;
            var mp = _contextUAT.MP.Find(usuario.MatchMP);            

            try
            {
                if (mp != null)
                {
                    mp.Activo = usuario.Estatus;
                    _contextUAT.Update(mp);
                    await _contextUAT.SaveChangesAsync();
                }

                _context.Update(usuario);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!usuarioExists(id))
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

        public async Task<IActionResult> ResetPass(string id)
        {
            var us = await _context.Users.FindAsync(id);
            us.PasswordHash = "AQAAAAEAACcQAAAAEEqiXmHSifLHdntCN+O0yJ9+TBzEynBiENVzz4v2zuOko0AIhtwjPn6E4QEMnAqVMg==";
            _context.Update(us);
            await _context.SaveChangesAsync();
            return Redirect("~/Usuarios");
        }

        // GET: Usuarios/Details/27
        public async Task<IActionResult> Details(long? id)
        {
            var query = await _contextUAT.MP
                .Include(x => x.UR)
                    .ThenInclude(x => x.Region)
                .Where(x => x.Id == id)
                .SingleOrDefaultAsync();

            ViewData["Urs"] = await _contextUAT.UR.OrderBy(x => x.Nombre).ToListAsync();

            return View(query);
        }

        public async Task<IActionResult> ActualizarUR(long mpId, long urId)
        {
            var mp = await _contextUAT.MP.FindAsync(mpId);
            if (mp == null)
            {
                return NotFound();
            }

            mp.URId = urId;
            _contextUAT.Update(mp);
            await _contextUAT.SaveChangesAsync();

            return Ok();
        }

        private bool usuarioExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}