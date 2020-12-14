using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AdminUAT.Data;
using AdminUAT.Models.Denuncias;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace AdminUAT.Areas.Bot.Controllers
{
    [Area("Bot")]
    public class BotsController : Controller
    {
        private readonly NewUatDbContext _contextUat;
        private readonly IHostingEnvironment _hostingEnvironment;

        public BotsController(NewUatDbContext contextUat, IHostingEnvironment hostingEnvironment)
        {
            _contextUat = contextUat;
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult BotSolucionMP()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> BotSolucionMP(IFormFile file)
        {
            string rootPath = _hostingEnvironment.WebRootPath;
            string namePath = Path.Combine(rootPath, "DenunciasLiberadas");

            if (!Directory.Exists(namePath))
            {
                Directory.CreateDirectory(namePath);
            }

            int sumaA = 0;
            int sumaB = 0;

            string pathFinal = Path.Combine(namePath, file.FileName);
            using (var stream = new FileStream(pathFinal, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            using (ExcelPackage package = new ExcelPackage(new FileInfo(pathFinal)))
            {
                ExcelWorksheet workSheet = package.Workbook.Worksheets[1];
                int totalRows = workSheet.Dimension.Rows;
                ViewBag.resumen = "Se cargo el archivo \"" +  file.FileName + "\" con " + totalRows + " denuncias.";

                for (int i = 2; i <= totalRows + 1; i++)
                {
                    try
                    {
                        var obj = await _contextUat.Denuncia.FindAsync(Convert.ToInt64(workSheet.Cells[i, 2].Value.ToString().Trim()));

                        if (obj != null)
                        {
                            if (obj.SolucionId == null)
                            {
                                obj.SolucionId = Convert.ToInt64(workSheet.Cells[i, 3].Value.ToString().Trim());
                                obj.NotaSolucion = workSheet.Cells[i, 4].Value.ToString().Trim();
                                obj.FechaSolucion = DateTime.Now;

                                var mp = await _contextUat.MP.FindAsync(obj.MPId);
                                mp.Resuelto++;

                                _contextUat.Update(obj);
                                _contextUat.Update(mp);
                                await _contextUat.SaveChangesAsync();

                                sumaA++;
                            }
                            else
                            {
                                sumaB++;
                            }
                        }
                        else
                        {
                            sumaB++;
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }

            ViewBag.sumaA = sumaA + " con cambio a atendido";
            ViewBag.SumaB = sumaB + " sin cambio de estatus";

            return View();
        }
    }
}
