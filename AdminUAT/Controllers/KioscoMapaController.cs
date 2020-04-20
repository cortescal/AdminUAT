using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminUAT.Data;
using AdminUAT.Models.Catalogos;
using Microsoft.AspNetCore.Authorization;
using AdminUAT.Models.ExtraModels;
using AdminUAT.Models.Denuncias;
using Microsoft.AspNetCore.Cors;

namespace AdminUAT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KioscoMapaController : ControllerBase
    {
        private readonly NewUatDbContext _context;

        public KioscoMapaController(NewUatDbContext context)
        {
            _context = context;
        }

        // GET: api/KioscoMapa
        [HttpGet]
        [AllowAnonymous]
        [EnableCors("Maps")]
        public async Task<ActionResult<IEnumerable<MapaData>>> GetBitaKiosco(string fecha)
        {
            fecha = fecha == null ? DateTime.Now.ToString("yyyy-MM-dd") : fecha;
            List<MapaData> json = new List<MapaData>();

            var kiosco = await _context.BitaKiosco
                .Where(x => x.Id != 1)
                .OrderBy(x => x.Id)
                .ToListAsync();

            foreach (var item in kiosco)
            {
                var denuncia = await _context.Denuncia
                    .Where(x => x.AltaSistema.ToString("yyyy-MM-dd") == fecha && x.BitaKioscoId == item.Id)
                    .ToListAsync();

                var denuncia2 = denuncia.Where(x => x.SolucionId != null).ToList();

                var obj = new MapaData
                {
                    Kiosco = item.Nombre,
                    Fecha = fecha,
                    Recibidas = denuncia.Count(),
                    Atendidas = denuncia2.Count(),
                    Ubicacion = item.Ubicacion
                };

                json.Add(obj);            
            }

            return json;
        }

        // GET: api/KioscoMapa/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BitaKiosco>> GetBitaKiosco(long id)
        {
            var bitaKiosco = await _context.BitaKiosco.FindAsync(id);

            if (bitaKiosco == null)
            {
                return NotFound();
            }

            return bitaKiosco;
        }

        // PUT: api/KioscoMapa/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBitaKiosco(long id, BitaKiosco bitaKiosco)
        {
            if (id != bitaKiosco.Id)
            {
                return BadRequest();
            }

            _context.Entry(bitaKiosco).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BitaKioscoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/KioscoMapa
        [HttpPost]
        public async Task<ActionResult<BitaKiosco>> PostBitaKiosco(BitaKiosco bitaKiosco)
        {
            _context.BitaKiosco.Add(bitaKiosco);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBitaKiosco", new { id = bitaKiosco.Id }, bitaKiosco);
        }

        // DELETE: api/KioscoMapa/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<BitaKiosco>> DeleteBitaKiosco(long id)
        {
            var bitaKiosco = await _context.BitaKiosco.FindAsync(id);
            if (bitaKiosco == null)
            {
                return NotFound();
            }

            _context.BitaKiosco.Remove(bitaKiosco);
            await _context.SaveChangesAsync();

            return bitaKiosco;
        }

        private bool BitaKioscoExists(long id)
        {
            return _context.BitaKiosco.Any(e => e.Id == id);
        }
    }
}
