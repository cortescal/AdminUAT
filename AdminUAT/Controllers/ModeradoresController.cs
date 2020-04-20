using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminUAT.Data;
using AdminUAT.Models.Denuncias;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminUAT.Controllers
{
    public class ModeradoresController : Controller
    {
        private readonly NewUatDbContext _contextUAT;

        public ModeradoresController(NewUatDbContext contextUAT)
        {
            _contextUAT = contextUAT;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<List<Denuncia>> DenunciasSinMP()
        {
            var denuncias = await _contextUAT.Denuncia
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
                        .ThenInclude(x => x.BitaKiosco)
                .Where(x => x.Paso == 3)
                .OrderByDescending(x => x.FinDenuncia)
                .Where(x => x.MP == null)
                .ToListAsync();

            return denuncias;
        }
    }
}