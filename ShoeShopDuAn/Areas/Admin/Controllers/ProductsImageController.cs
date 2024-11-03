using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ShoeShopDuAn.Models;
using ShoeShopDuAn.Models.SP;

namespace ShoeShopDuAn.Areas.Admin.Controllers
{
    public class ProductsImageController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/ProductImage
        public ActionResult Index(int id)
        {
            var items = db.ProductImages.Where(x => x.ProductId == id).ToList();
            ViewBag.ProductId = id;
            return View(items);
        }

        [HttpPost]
        public async Task<ActionResult> AddImage(int productId, string url)
        {
            var newImage = new ProductImage
            {
                ProductId = productId,
                Image = url,
                IsDefault = false
            };

            db.ProductImages.Add(newImage);
            await db.SaveChangesAsync();

            return Json(new { Success = true });
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            var item = await db.ProductImages.FindAsync(id);
            if (item != null)
            {
                db.ProductImages.Remove(item);
                await db.SaveChangesAsync();

                return Json(new { success = true });
            }

            return Json(new { success = false });
        }
    }
}
