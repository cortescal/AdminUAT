using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminUAT.Models.Denuncias;
using AdminUAT.Models.MinisterioPublico;
using AdminUAT.Models.Catalogos;
using AdminUAT.Models.Victimas;

namespace AdminUAT.Data
{
    public class NewUatDbContext : DbContext
    {
        public NewUatDbContext(DbContextOptions<NewUatDbContext> options)
             : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<MunicipioUR>()
                .HasKey(x => new { x.MunicipioId, x.URId });
        }

        public DbSet<Denuncia> Denuncia { get; set; }
        public DbSet<Encuesta> Encuesta { get; set; }
        public DbSet<Solucion> Solucion { get; set; }
        public DbSet<MP> MP { get; set; }
        public DbSet<BitaKiosco> BitaKiosco { get; set; }
        public DbSet<UR> UR { get; set; }
        public DbSet<Victima> Victima { get; set; }
        public DbSet<DireccionDenuncia> DireccionDenuncia { get; set; }
        public DbSet<Colonia> Colonia { get; set; }
        public DbSet<Municipio> Municipio { get; set; }
    }
}
