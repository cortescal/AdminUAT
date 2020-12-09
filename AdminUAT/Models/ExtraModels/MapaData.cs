using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Models.ExtraModels
{
    public class MapaData
    {
        public string Kiosco { get; set; }

        public int Recibidas { get; set; }

        public int Atendidas { get; set; }

        public string Fecha { get; set; }

        public string Ubicacion { get; set; }
    }

    public class MapaFiscalia
    {
        public List<DataFiscalia> Data { get; set; }
        public int CDI { get; set; }
        public int Constancia { get; set; }
        public int Archivo { get; set; }
    }

    public class DataFiscalia
    {
        public string Nombre { get; set; }
        public int Atendidas { get; set; }
        public int Recibidas { get; set; }
    }
}
