using GeoFotos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GeoFotos.Views
{
    public class StatusErrorController : Controller
    {
        // GET: StatusError
        public ActionResult Index(string keys , string uris)
        {
            linkData _model = new linkData();
            _model.link = uris;
            _model.nameImg =  keys;
            return View(_model);
        }

        
    }
}
