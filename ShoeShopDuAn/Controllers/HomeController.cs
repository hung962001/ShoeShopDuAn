using ShoeShopDuAn.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShoeShopDuAn.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
           ;

            return View();
        }

        public ActionResult Contact()
        {      
            return View();
        }
        public ActionResult PartialCategoryProduct()
        {
            var items = db.ProductCategories.ToList();
            return PartialView("_particalCategoryProduct", items);
        }
        public ActionResult PartialFeaturedMenu()
        {
            var items = db.ProductCategories.ToList();
            return PartialView("_ParticalFeaturedMenu", items);
        }
        public ActionResult LastestProduct()
        {
            var items = db.Products.OrderByDescending(c => c.CreatedDate).Take(6).ToList();
            var Top1Top3= items.Take(3).ToList();
            var Top4Top6 = items.Skip(3).ToList();
            ViewBag.Top1Top3 = Top1Top3;
            ViewBag.Top4Top6 = Top4Top6;
            return PartialView("_particalLastestProduct");

        }
    }
}