using AdminUAT.Data;
using AdminUAT.Models;
using AdminUAT.Models.Denuncias;
using AdminUAT.Models.Responsables;
using AdminUAT.Models.Victimas;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Controllers
{
    //[Authorize(Roles = "MP, Root")]
    public class DescargaPDFController : Controller
    {
        private readonly NewUatDbContext _context;
        private UserManager<ApplicationUser> _userManager;
        private IHostingEnvironment _hostingEnvironment;

        public DescargaPDFController(NewUatDbContext context, UserManager<ApplicationUser> userManager,
            IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GeneraPDF(long id)
        {
            Denuncia denuncia = await Details(id);

            if (denuncia == null)
            {
                return Redirect("~/Identity/Account/AccessDenied");
            }

            string nombreArchivo = denuncia.Expediente.Replace("@", "");
            nombreArchivo = nombreArchivo.Replace("/", "_");
            byte[] bytes;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                //MemoryStream memoryStream = new MemoryStream(); 
                //Document doc = new Document(PageSize.Letter, 19, 0, 7, 1); // izq,der,top,DOWN
                Document doc = new Document(PageSize.Legal, 10, 10, 90, 70);
                PdfWriter wri = PdfWriter.GetInstance(doc, memoryStream);
                doc.Open();// Open Dpcument to write

                // Creamos la imagen 
 
                    var root = _hostingEnvironment.WebRootPath;

                Image imagen = Image.GetInstance(Path.Combine(root, @"images\ENCABEZADO_4.png"));
                imagen.ScaleAbsoluteWidth(500f);
                imagen.ScaleAbsoluteHeight(90f);
                imagen.SetAbsolutePosition(56, 863);
                //Agregamos la imagen al documeno.
                doc.Add(imagen);

                //Marca de agua
                Image objImagePdf;

                // Crea la imagen
                objImagePdf = Image.GetInstance(Path.Combine(root, @"images\LOGO_FONDO_2.png"));
                // Cambia el tamaño de la imagen
                objImagePdf.ScaleToFit(700, 560);
                // Se indica que la imagen debe almacenarse como fondo
                objImagePdf.Alignment = iTextSharp.text.Image.UNDERLYING;
                // Coloca la imagen en una posición absoluta
                objImagePdf.SetAbsolutePosition(44, 0);
                // Imprime la imagen como fondo de página
                doc.Add(objImagePdf);


                /*Image imagen = Image.GetInstance(Path.Combine(root, @"images\logo2.png"));
                imagen.ScaleAbsoluteWidth(70f);
                imagen.ScaleAbsoluteHeight(70f);
                imagen.SetAbsolutePosition(73, 863);
                //Agregamos la imagen al documeno.
                doc.Add(imagen);
                //Write some content 
                Paragraph paragraph = new Paragraph("FISCALÍA GENERAL DEL ESTADO DE PUEBLA");
                paragraph.SetAlignment("CENTER");
                paragraph.Font.SetStyle("bold");
                // Now add the above created text using different class object to our pdf document 
                doc.Add(paragraph);*/
                Paragraph paragraph = new Paragraph("");//eliminar

                Paragraph paragraph2 = new Paragraph("\n\n\n\n");
                doc.Add(paragraph2);

                //Tabla 1
                PdfPTable table = new PdfPTable(2);

                Paragraph paragraph3 = new Paragraph("DENUNCIA ELECTRÓNICA");
                paragraph3.Font.SetStyle("bold");
 
                PdfPCell cell = new PdfPCell(new Phrase(paragraph3));
                cell.Colspan = 2;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.BackgroundColor = BaseColor.LightGray;
                table.AddCell(cell);

                table.AddCell("Folio");
                table.AddCell(denuncia.Expediente);

                table.AddCell("Fecha y hora de registro");
                table.AddCell(denuncia.AltaSistema.ToString());

                table.AddCell("Denuncia con perspectiva de género");
                table.AddCell("No");
                doc.Add(table);

                //Tabla 2
                Victima denunciante = new Victima();
                List<Victima> victimas = new List<Victima>();

                foreach (var item in denuncia.Victima)
                {
                    if (item.Email != "")
                    {
                        denunciante = item;
                    }

                    if (item.EsVictima == true)
                    {
                        victimas.Add(item);
                    }
                }

                paragraph2 = new Paragraph("\n");
                doc.Add(paragraph2);
                table = new PdfPTable(2);

                paragraph3 = new Paragraph("DATOS DEL DENUNCIANTE");
                paragraph3.Font.SetStyle("bold");

                cell = new PdfPCell(new Phrase(paragraph3));
                cell.Colspan = 2;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.BackgroundColor = BaseColor.LightGray;
                table.AddCell(cell);

                table.AddCell("Nombre");
                table.AddCell(denunciante.Nombre.ToUpper() + " " + denunciante.PrimerApellido.ToUpper() + " " + denunciante.SegundoApellido.ToUpper());

                table.AddCell("Género");
                table.AddCell(denunciante.Genero.Sexo);

                table.AddCell("Fecha de Nacimiento");
                table.AddCell(denunciante.FechaNacimiento.ToString("dd/MM/yyyy"));

                int edad = 0;
                //////////////////////
                table.AddCell("Edad");
                if (DateTime.Now.ToString("yyyy") != denunciante.FechaNacimiento.ToString("yyyy")&&DateTime.Now.Year>denunciante.FechaNacimiento.Year)
                {
                    edad = DateTime.Today.AddTicks(-denunciante.FechaNacimiento.Ticks).Year - 1;
                    table.AddCell(edad.ToString());
                }
                else
                {
                    table.AddCell("");
                }  
                //////////////////////
                table.AddCell("Escolaridad");
                table.AddCell(denunciante.Escolaridad.Descripcion);

                DireccionVictima dirVictima = denunciante.DireccionVictima.First(); 
                table.AddCell("Domicilio");
                table.AddCell(dirVictima.Calle + " " + dirVictima.NumExterior + ", " + dirVictima.NumInterior + "\nCol. " + dirVictima.Colonia.Nombre + "\nC.P. " + dirVictima.Colonia.CP + "\n" + dirVictima.Colonia.Municipio.Nombre +"\n" + dirVictima.Colonia.Municipio.Estado.Nombre + ", México");

                table.AddCell("Telefono local");
                table.AddCell(denunciante.TelFijo == ""? "********" : denunciante.TelFijo);

                table.AddCell("Telefono Móvil");
                table.AddCell(denunciante.TelMovil);

                table.AddCell("Correo Electronico");
                table.AddCell(denunciante.Email + "\n ");

                table.AddCell("El denunciante es abogado");
                table.AddCell(denunciante.Abogado == false? "NO" : "SI");

                if (denunciante.Abogado)
                {
                    table.AddCell("Cédula profesional No.");
                    table.AddCell(denunciante.Cedula);

                    table.AddCell("Despacho");
                    table.AddCell(denunciante.Despacho);
                }

                table.AddCell("Victima");
                table.AddCell(denunciante.EsVictima == false ? "NO" : "SI");

                doc.Add(table);

                //Tabla 3
                paragraph2 = new Paragraph("\n");
                doc.Add(paragraph2);
                table = new PdfPTable(1);

                paragraph3 = new Paragraph("DATOS DE VÍCTIMAS U OFENDIDOS(AS)");
                paragraph3.Font.SetStyle("bold");

                cell = new PdfPCell(new Phrase(paragraph3));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.BackgroundColor = BaseColor.LightGray;
                table.AddCell(cell);

                if (victimas.Count > 0)
                {
                    foreach (var item in victimas)
                    {
                        String nombre = item.Nombre.ToUpper() + " " + item.PrimerApellido.ToUpper() + " " + item.SegundoApellido.ToUpper();
                        DireccionVictima dirVs = item.DireccionVictima.FirstOrDefault();
                        table.AddCell("Nombre: " + nombre + ", Genero: " + item.Genero.Sexo + ", Fecha de nacimiento: " + item.FechaNacimiento.ToString("dd/MM/yyyy") + ", Escolaridad: " + item.Escolaridad.Descripcion + ", Estado: " + dirVs.Colonia.Municipio.Estado.Nombre + ", Municipio: " + dirVs.Colonia.Municipio.Nombre + ", Colonia: " + dirVs.Colonia.Nombre + ", Calle: " + dirVs.Calle + ", No. ext: " + dirVs.NumExterior + ", No. Int: " + dirVs.NumInterior + ", Cp: " + dirVs.Colonia.CP);
                    }                   
                }
                else
                {
                    table.AddCell("********");
                }

                doc.Add(table);

                //Tabla 4
                paragraph2 = new Paragraph("\n");
                doc.Add(paragraph2);
                table = new PdfPTable(2);

                paragraph3 = new Paragraph("LUGAR DE LOS HECHOS");
                paragraph3.Font.SetStyle("bold");

                cell = new PdfPCell(new Phrase(paragraph3));
                cell.Colspan = 2;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.BackgroundColor = BaseColor.LightGray;
                table.AddCell(cell);

                table.AddCell(denuncia.FechaEvento.HasValue? "Fecha: " + ((DateTime)denuncia.FechaEvento).ToString("dd/MM/yyyy") : null);
                table.AddCell(denuncia.FechaEvento.HasValue? "Hora: " + ((DateTime)denuncia.FechaEvento).ToString("hh:mm:ss tt") : null);

                DireccionDenuncia lugarHechos = denuncia.DireccionDenuncia.First();
                table.AddCell("Domicilio/Descripción");
                table.AddCell(lugarHechos.Calle + " " + lugarHechos.NumExterior + ", " + lugarHechos.NumInterior + "\nCol. " + lugarHechos.Colonia.Nombre + "\nC.P. " + lugarHechos.Colonia.CP + "\n" + lugarHechos.Colonia.Municipio.Nombre + "\n" + lugarHechos.Colonia.Municipio.Estado.Nombre + ", México");

                doc.Add(table);

                //Tabla 5 DATOS DE IDENTIFICACIÓN DE LOS PROBABLES RESPONSABLES
                paragraph2 = new Paragraph("\n");
                doc.Add(paragraph2);
                table = new PdfPTable(1);

                paragraph3 = new Paragraph("DATOS DE IDENTIFICACIÓN DE LOS PROBABLES RESPONSABLES");
                paragraph3.Font.SetStyle("bold");

                cell = new PdfPCell(new Phrase(paragraph3));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.BackgroundColor = BaseColor.LightGray;
                table.AddCell(cell);

                if (denuncia.Responsable.Count == 0)
                {
                    table.AddCell("********");
                }
                else
                {
                    foreach (var item in denuncia.Responsable)
                    {
                        String nombre = item.Nombre.ToUpper() + " " + item.PrimerApellido.ToUpper() + " " + item.SegundoApellido.ToUpper();
                        DireccionResponsable dirR = item.DireccionResponsable.FirstOrDefault();
                        DescResponsable descR = item.DescResponsable.FirstOrDefault();

                        string dirText = dirR == null? "": ", Estado: " + dirR.Colonia.Municipio.Estado.Nombre + ", Municipio: " + dirR.Colonia.Municipio.Nombre + ", Colonia: " + dirR.Colonia.Nombre + ", Calle: " + dirR.Calle + ", No. ext: " + dirR.NumExterior + ", No. Int: " + dirR.NumInterior + ", Cp: " + dirR.Colonia.CP;
                        string descText = descR == null ? "": ", Color de piel: " + descR.ColorPiel + ", Altura: " + descR.Altura.ToString() + ", Tipo de cabello: " + descR.TipoCabello + ", Color de cabello: " + descR.ColorCabello + ", Color de ojos: " + descR.ColorOjos + ", Complexión: " + descR.Complexion + ", Tatuajes: " + (descR.Tatuajes == true? "SI" : "NO");
                        
                        table.AddCell("Nombre: " + nombre + ", Genero: " + item.Genero.Sexo + ", Alias: " + item.Alias + dirText + descText);
                    }
                }                

                doc.Add(table);

                //Tabla 6 DELITO REPORTADO
                paragraph2 = new Paragraph("\n");
                doc.Add(paragraph2);
                table = new PdfPTable(1);

                paragraph3 = new Paragraph("DELITO REPORTADO");
                paragraph3.Font.SetStyle("bold");

                cell = new PdfPCell(new Phrase(paragraph3));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.BackgroundColor = BaseColor.LightGray;
                table.AddCell(cell);

                if(denuncia.Delito != null)
                {
                    table.AddCell(denuncia.Delito.Tipo);
                }
                else
                {
                    table.AddCell("********");
                }

                doc.Add(table);

                //Tabla 7 INFORMACIÓN ADICIONAL DEL DELITO
                paragraph2 = new Paragraph("\n");
                doc.Add(paragraph2);
                table = new PdfPTable(2);

                paragraph3 = new Paragraph("INFORMACIÓN ADICIONAL DEL DELITO");
                paragraph3.Font.SetStyle("bold");

                cell = new PdfPCell(new Phrase(paragraph3));
                cell.Colspan = 2;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.BackgroundColor = BaseColor.LightGray;
                table.AddCell(cell);

                table.AddCell("Daño");
                table.AddCell(denuncia.DanioId == null? "********": denuncia.Danio.Tipo);

                table.AddCell("Testigos");
                table.AddCell(denuncia.NumTestigo == null? "0":denuncia.NumTestigo.ToString());

                table.AddCell("Denuncia en emergencias");
                table.AddCell((denuncia.NumEmergencia == null || denuncia.NumEmergencia == "")? "********": denuncia.NumEmergencia);

                doc.Add(table);

                //Tabla 7 INFORMACIÓN ADICIONAL DEL DELITO
                paragraph2 = new Paragraph("\n");
                doc.Add(paragraph2);
                table = new PdfPTable(1);

                paragraph3 = new Paragraph("RELATO DE LOS HECHOS");
                paragraph3.Font.SetStyle("bold");

                cell = new PdfPCell(new Phrase(paragraph3));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.BackgroundColor = BaseColor.LightGray;
                table.AddCell(cell);

                table.AddCell(denuncia.Relato);

                doc.Add(table);

                //PIE DE PAGINA
                paragraph = new Paragraph("\nPie de página");
                paragraph.SetAlignment("CENTER");
                doc.Add(paragraph);

                paragraph = new Paragraph("_______________________________________________________________________");
                paragraph.SetAlignment("CENTER");
                paragraph.Font.SetStyle("bold");
                doc.Add(paragraph);

                ///////////////////////////////////////////////////////////////////////////////////
                doc.Close(); //Close document
                bytes = memoryStream.ToArray();
                memoryStream.Close();
            }// end usin memoryStream
            //return bytes;

            return File(bytes, "application/pdf", nombreArchivo + ".pdf");
        }

        public async Task<Denuncia> Details(long? id)
        {
            var usu = await _userManager.GetUserAsync(User);
            var d = await _context.Denuncia.FindAsync(id);

            if (usu.Rol == 2)
            {
                if (id == null || d.MPId != usu.MatchMP)
                {
                    return null;
                }
            }

            Denuncia denuncia = await _context.Denuncia
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

            return denuncia;
        }
    }
}