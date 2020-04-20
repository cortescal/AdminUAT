using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AdminUAT.Models;
using Microsoft.AspNetCore.Identity;
using AdminUAT.Data;
using Microsoft.AspNetCore.Authorization;

namespace AdminUAT.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public HomeController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        [Authorize]
        public IActionResult Index()
        {
            var id = _userManager.GetUserId(User);
            var usuario = _context.Users.Find(id);
            ViewData["rol"] = usuario.Rol;
            ViewData["nombre"] = usuario.Nombre + " " + usuario.PrimerApellido + " " + usuario.SegundoApellido;

            return View(_context.Noticia.Where(x => x.Activo == true).OrderBy(x => x.AltaSistema).ToList());
        }

        public IActionResult Manual()
        {
            return View();
        }

        /*[AllowAnonymous]
        public async Task<IActionResult> Privacy()
        {
            if (User.Identity.IsAuthenticated)
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
                var user = await _userManager.GetUserAsync(HttpContext.User);
                await _userManager.AddToRoleAsync(user, "Admin");
            }
            return View();
        }*/

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
