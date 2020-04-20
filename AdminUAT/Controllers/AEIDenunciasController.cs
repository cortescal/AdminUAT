using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AdminUAT.Data;
using AdminUAT.Models;
using AdminUAT.Models.Denuncias;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminUAT.Controllers
{
    [Authorize(Roles = "AEI, Root")]
    public class AEIDenunciasController : Controller
    {
        private readonly NewUatDbContext _contextUAT;
        private UserManager<ApplicationUser> _userManager;

        public AEIDenunciasController(NewUatDbContext contextUAT, UserManager<ApplicationUser> userManager)
        {
            _contextUAT = contextUAT;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("NotaAEI")] Denuncia denuncia)
        {
            if (denuncia.NotaAEI == null)
            {
                return Redirect("~/Denuncias");
            }

            Denuncia obj = await _contextUAT.Denuncia.FindAsync(id);

            if (obj == null)
            {
                return NotFound();
            }

            var usu = await _userManager.GetUserAsync(User);

            string fecha = DateTime.Now.ToString("MMMM dd, yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("es-MX"));
            obj.NotaAEI = denuncia.NotaAEI + "|cs: " + usu.Nombre + " " + usu.PrimerApellido + " " + usu.SegundoApellido + " " + fecha;
            _contextUAT.Update(obj);
            await _contextUAT.SaveChangesAsync();           

            return Redirect("~/Denuncias");
        }
    }
}