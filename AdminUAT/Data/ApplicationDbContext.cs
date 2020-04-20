using AdminUAT.Models;
using AdminUAT.Models.AdminUat;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AdminUAT.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Bitacora>()
                .HasKey(x => new { x.Usuario, x.EventoId, x.FechaEvento });

            builder.Entity<SeguimientoSoporte>()
                .HasKey(x => new { x.OrdenSoporteId, x.Usuario, x.AltaSistema });
        }

        public DbSet<Bitacora> Bitacora { get; set; }
        public DbSet<Evento> Evento { get; set; }
        public DbSet<Noticia> Noticia { get; set; }
        public DbSet<OrdenSoporte> OrdenSoporte { get; set; }
        public DbSet<SeguimientoSoporte> SeguimientoSoporte { get; set; }
        public DbSet<TipoSoporte> TipoSoporte { get; set; }
        public DbSet<Token> Token { get; set; }
    }
}
