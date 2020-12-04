using AdminUAT.Models.Catalogos;
using AdminUAT.Models.MinisterioPublico;
using AdminUAT.Models.Responsables;
using AdminUAT.Models.Victimas;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminUAT.Models.Denuncias
{
    public class Denuncia
    {
        public long Id { get; set; }
        public string Relato { get; set; }
        public DateTime? FechaEvento { get; set; }
        public bool? Confirmacion { get; set; }
        public int? NumTestigo { get; set; }
        public string NumEmergencia { get; set; }
        public DateTime AltaSistema { get; set; }
        public DateTime? FinDenuncia { get; set; }
        public int Paso { get; set; }
        public string Origen { get; set; }
        public DateTime? FechaSolucion { get; set; }
        public string NotaSolucion { get; set; }
        public string NotaAEI { get; set; }
        public string Expediente { get; set; }

        public long? MPId { get; set; }
        public long? SolucionId { get; set; }
        public long? DanioId { get; set; }
        public long? DelitoId { get; set; }
        public long BitaKioscoId { get; set; }

        public Guid? FiscaliaId { get; set; }
        public Guid? FiscaliaCorrespondienteId { get; set; }

        public MP MP { get; set; }
        public Solucion Solucion { get; set; }
        public Danio Danio { get; set; }
        public Delito Delito { get; set; }

        [ForeignKey("FiscaliaId")]
        public Fiscalia Fiscalia { get; set; }
        [ForeignKey("FiscaliaCorrespondienteId")]
        public Fiscalia FiscaliaCorrespondiente { get; set; }

        public ICollection<Victima> Victima { get; set; }
        public ICollection<Responsable> Responsable { get; set; }
        public ICollection<DireccionDenuncia> DireccionDenuncia { get; set; }
        public BitaKiosco BitaKiosco { get; set; }
    }
}
