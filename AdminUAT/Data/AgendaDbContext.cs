using AdminUAT.Models.AgendaUAT;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Data
{
    public class AgendaDbContext : DbContext
    {
        public AgendaDbContext(DbContextOptions<AgendaDbContext> options)
            : base(options)
        {

        }

        public DbSet<Cita> Cita { get; set; }
        public DbSet<Dia> Dia { get; set; }
        public DbSet<Hora> Hora { get; set; }
        public DbSet<HoraDia> HoraDia { get; set; }
    }
}
