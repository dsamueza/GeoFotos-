using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GeoFotos.Models
{
    public class ImageData
    {
            public String Code { get; set; }
            public string name { get; set; }
            public byte[] valueFoto { get; set; }
            public String base64 { get; set; }
            public String nameImg { get; set; }
            public String uri { get; set; }
            public string link { get; set; }
    
    }
}