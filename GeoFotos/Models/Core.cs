using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GeoFotos.Models
{
    public class Core
    {
        public string id_core { get; set; }
        public string id_image { get; set; }
        //public string project { get; set; }
        //public string idcampaing { get; set; }
        public string ruta_cod { get; set; }
        public string filename { get; set; }
        public Byte[] value { get; set; }
    }
}