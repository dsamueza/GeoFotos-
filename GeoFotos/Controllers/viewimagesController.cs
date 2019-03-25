using GeoFotos.Bussines;
using GeoFotos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GeoFotos.Controllers
{
    public class viewimagesController : Controller
    {
        private ConsultBackupBussiness _consultBackupBussiness = new ConsultBackupBussiness();

        public ActionResult load(string key, string uri)
        {
            ImageTotal _imageTotal = new ImageTotal();
            if (key != null && uri != null)
            {
                if (!key.Equals("") & !uri.Equals(""))
                {
                    _imageTotal.ImageData = _consultBackupBussiness.GetListImageByKeyUri(key, uri);
                    
                    
                    return View(_imageTotal);
                }
            }
            return View();
        }
    }
}
