using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GeoFotos.Models
{
    public class ImageTotal
    {
        public string CORE { get; set; }
        public string id { get; set; }
        public string Code { get; set; }
        public string Ruta { get; set; }
        public string Rutaexiste { get; set; }
        public string TipoNegocio { get; set; }
        public string NombreLocal { get; set; }
        public string Dirección { get; set; }
        public DateTime Fecha { get; set; }
        public decimal GeoLa { get; set; }
        public decimal GeoLo { get; set; }

        public IList<ImageData> ImageData { get; set; }
    }
}