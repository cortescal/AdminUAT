﻿// <auto-generated />
using System;
using AdminUAT.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AdminUAT.Migrations.AgendaDb
{
    [DbContext(typeof(AgendaDbContext))]
    [Migration("20200323154241_Second")]
    partial class Second
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.1-servicing-10028")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("AdminUAT.Models.AgendaUAT.Cita", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("Activo");

                    b.Property<DateTime>("AltaSistema");

                    b.Property<int>("Asistencia");

                    b.Property<string>("Comentario");

                    b.Property<DateTime>("Dia");

                    b.Property<long>("HoraDiaId");

                    b.Property<long>("MP");

                    b.Property<bool>("Notificado");

                    b.Property<long>("NumDenuncia");

                    b.Property<int>("SendEmail");

                    b.HasKey("Id");

                    b.HasIndex("HoraDiaId");

                    b.ToTable("Cita");
                });

            modelBuilder.Entity("AdminUAT.Models.AgendaUAT.Dia", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("Activo");

                    b.Property<string>("Nombre");

                    b.Property<int>("Numero");

                    b.HasKey("Id");

                    b.ToTable("Dia");
                });

            modelBuilder.Entity("AdminUAT.Models.AgendaUAT.Hora", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("Activo");

                    b.Property<TimeSpan>("CampoHora");

                    b.HasKey("Id");

                    b.ToTable("Hora");
                });

            modelBuilder.Entity("AdminUAT.Models.AgendaUAT.HoraDia", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("Activo");

                    b.Property<DateTime>("AltaSistema");

                    b.Property<int>("DiaId");

                    b.Property<DateTime>("FechaFinal");

                    b.Property<int>("HoraId");

                    b.Property<long>("MP");

                    b.HasKey("Id");

                    b.HasIndex("DiaId");

                    b.HasIndex("HoraId");

                    b.ToTable("HoraDia");
                });

            modelBuilder.Entity("AdminUAT.Models.AgendaUAT.Cita", b =>
                {
                    b.HasOne("AdminUAT.Models.AgendaUAT.HoraDia", "HoraDia")
                        .WithMany("Cita")
                        .HasForeignKey("HoraDiaId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("AdminUAT.Models.AgendaUAT.HoraDia", b =>
                {
                    b.HasOne("AdminUAT.Models.AgendaUAT.Dia", "Dia")
                        .WithMany("HoraDia")
                        .HasForeignKey("DiaId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("AdminUAT.Models.AgendaUAT.Hora", "Hora")
                        .WithMany("HoraDia")
                        .HasForeignKey("HoraId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}