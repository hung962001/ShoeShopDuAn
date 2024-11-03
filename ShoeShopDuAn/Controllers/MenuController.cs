using ShoeShopDuAn.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShoeShopDuAn.Controllers
{
    public class MenuController : Controller
    {
        // GET: Menu
        private ApplicationDbContext db = new ApplicationDbContext(); 
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult OurMenuproducts()
        {
          
            return PartialView("OurMenuProducts");
        }
      
    }
}