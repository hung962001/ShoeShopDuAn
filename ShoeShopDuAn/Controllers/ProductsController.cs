using PagedList;
using ShoeShopDuAn.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity; // Import để dùng Include
namespace ShoeShopDuAn.Controllers
{

    public class ProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Product
        // Import để dùng Include
        public ActionResult Index(int? id, string searchQuery, int? minPrice, int? maxPrice, int page = 1, int pageSize = 10)
        {
            ViewBag.CateId = id;
            ViewBag.SearchQuery = searchQuery;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            // Lấy danh sách sản phẩm, không bao gồm sản phẩm nổi bật
            var products = db.Products
                .Include(p => p.ProductCategory)
                .Where(x => !x.IsFeature);

            // Lọc theo danh mục
            if (id.HasValue)
            {
                products = products.Where(x => x.CategoryId == id.Value);
            }

            // Lọc theo từ khóa tìm kiếm
            if (!string.IsNullOrEmpty(searchQuery))
            {
                products = products.Where(x => x.Title.Contains(searchQuery));
            }

            // Lọc theo khoảng giá
            if (minPrice.HasValue)
            {
                products = products.Where(x => x.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                products = products.Where(x => x.Price <= maxPrice.Value);
            }

            // Phân trang và sắp xếp
            var pagedProducts = products
                .OrderBy(x => x.ProductCategory.Title)
                .ThenBy(x => x.Id)
                .ToPagedList(page, pageSize);

            // Lấy danh mục sản phẩm để hiển thị
            ViewBag.Categories = db.ProductCategories.Where(x => !x.IsDelete).ToList();

            return View(pagedProducts);
        }


        public ActionResult ItemsOurProduct()
        {
            var items = db.Products
                 .Where(x => !x.IsFeature)
                 .OrderByDescending(x => x.CreatedDate)
                 .Take(12)
                 .ToList();
            return PartialView(items);

        }
        public ActionResult Detail(int id)
        {
            var item = db.Products.Find(id);
            if (item != null)
            {
                db.Products.Attach(item);              
                db.SaveChanges();
            }
            return View(item);
        }

    }
}