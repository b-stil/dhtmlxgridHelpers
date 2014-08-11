using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DHXHelperDemo.Code.DHX;
using DHXHelperDemo.Models;

namespace DHXHelperDemo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View("Index");
        }

        public ActionResult StandardGrid()
        {
            return View("Standard");
        }

        public ActionResult StandardGridWithObjInit()
        {
            return View("StandardObjectInit");
        }

        public DHXResult<DemoDHXVM> GetDemoJson()
        {
            var m = new DemoData();
            var vm = m.GetDemoData().AsQueryable();

            return new DHXResult<DemoDHXVM>(vm, Request, true);
        }

        public DHXResult<GridVM<DemoDHXVM>> GetObjectInitDemoJson()
        {
            var m = new DemoData();
            var vm = m.GetObjectDemoData().AsQueryable();

            return new DHXResult<GridVM<DemoDHXVM>>(vm, Request, true);
        }

    }
}