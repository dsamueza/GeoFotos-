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
        public ActionResult Index()
        {
            return View();
        }
            public ActionResult load(string key, string uri)
        {

            try
            {
                ImageTotal _imageTotal = new ImageTotal();
                if (key != null && uri != null)
                {
                    if (!key.Equals("") & !uri.Equals(""))
                    {
                        _imageTotal = _consultBackupBussiness.GetListImageByKeyUri(key, uri);


                        return View(_imageTotal);
                    }
                }
                return View();
            }
            catch (Exception e)
            {
                return RedirectToAction("Index", "StatusError", new { uris = uri, keys = key });
            }
           
        }

        public ActionResult image(string core, string uri)
        {
            try
            {
                ImageTotal _imageTotal = new ImageTotal();
                if (core != null && uri != null)
                {
                    if (!core.Equals("") & !uri.Equals(""))
                    {
                        _imageTotal = _consultBackupBussiness.ImageCore(core, uri);


                        return View(_imageTotal);
                    }
                }
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "StatusError", new { statusCode = 1 });
            }
           
        }
    }
}
