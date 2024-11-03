using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using PagedList;
using ShoeShopDuAn.Models;
using ShoeShopDuAn.Models.SP;
using StackExchange.Redis;
using System.Configuration;

namespace ShoeShopDuAn.Areas.Admin.Controllers
{
    public class ProductCategoryController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();
        private readonly ConnectionMultiplexer redisConnection;
        private readonly IDatabase redisDB;
        public ProductCategoryController()
        {
            var redisConnectionString = ConfigurationManager.ConnectionStrings["RedisConnection"].ConnectionString;
            redisConnection = ConnectionMultiplexer.Connect(redisConnectionString);
            redisDB = redisConnection.GetDatabase();
        }
        public async Task<ActionResult> Index(string searchProductCategories, int? page)
        {
            string cacheKey = $"ProductCategories_active_{searchProductCategories ?? "all"}_{page ?? 1}";
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var items = db.ProductCategories.Where(c => !c.IsDelete).AsQueryable();

            if (!string.IsNullOrEmpty(searchProductCategories))
            {
                items = items.Where(x => x.Title.Contains(searchProductCategories) ||
                                         x.Description.Contains(searchProductCategories) ||
                                         x.Id.ToString().Contains(searchProductCategories));
            }

            var ProductCategoriesList = items.OrderBy(x => x.Title).ToPagedList(pageNumber, pageSize);
            var serializedProductCategories = Newtonsoft.Json.JsonConvert.SerializeObject(ProductCategoriesList);

            await redisDB.StringSetAsync(cacheKey, serializedProductCategories, TimeSpan.FromHours(1));

            return View(ProductCategoriesList);
        }


        public async Task<ActionResult> Edit(int id)
        {
            string cacheKey = $"ProductCategories_{id}";
            var cacheProductCategories = await redisDB.StringGetAsync(cacheKey);
             ProductCategories item;

            if (!string.IsNullOrEmpty(cacheProductCategories))
            {
                item = Newtonsoft.Json.JsonConvert.DeserializeObject<ProductCategories>(cacheProductCategories);
            }
            else
            {
                item = await db.ProductCategories.FindAsync(id);
                if (item == null)
                {
                    return HttpNotFound();
                }
                var serializedProductCategories = Newtonsoft.Json.JsonConvert.SerializeObject(item);
                await redisDB.StringSetAsync(cacheKey, serializedProductCategories, TimeSpan.FromDays(1));
            }

            return View(item);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ProductCategories model)
        {
            if (ModelState.IsValid)
            {

                var existingProductCategories = await db.ProductCategories.FindAsync(model.Id);
                if (existingProductCategories == null)
                {
                    return HttpNotFound();
                }

                existingProductCategories.Title = model.Title;
                existingProductCategories.Description = model.Description;
                existingProductCategories.Alias = ShoeShopDuAn.Models.Common.Filter.ChuyenCoDauThanhKhongDau(model.Title);
                existingProductCategories.ModifiedDate = DateTime.Now;
                existingProductCategories.ModifiedBy = model.ModifiedBy;

                await db.SaveChangesAsync();


                var serializedProductCategories = Newtonsoft.Json.JsonConvert.SerializeObject(existingProductCategories);
                await redisDB.StringSetAsync($"ProductCategories_{existingProductCategories.Id}", serializedProductCategories, TimeSpan.FromDays(1));


                await redisDB.KeyDeleteAsync("ProductCategoriesproduct_active_*"); // Delete all active ProductCategories caches


                await UpdateActiveProductCategoriesCache();

                return RedirectToAction("Index");
            }

            return View(model);
        }


        public ActionResult Add()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Add(ProductCategories model)
        {
            if (ModelState.IsValid)
            {
                var existingProductCategories = db.ProductCategories
                    .Where(c => c.Title.Equals(model.Title, StringComparison.OrdinalIgnoreCase) && !c.IsDelete)
                    .ToList();

                if (existingProductCategories.Any())
                {
                    ModelState.AddModelError("Title", "Tên danh mục đã tồn tại");
                }
                else
                {
                    model.CreatedDate = DateTime.Now;
                    model.ModifiedDate = DateTime.Now;
                    model.Alias = ShoeShopDuAn.Models.Common.Filter.ChuyenCoDauThanhKhongDau(model.Title);
                    db.ProductCategories.Add(model);
                    await db.SaveChangesAsync();

                    // Delete all old cache for active ProductCategories
                    await redisDB.KeyDeleteAsync("ProductCategories_active_*");

                    // Update the cache with the new list of active ProductCategories
                    await UpdateActiveProductCategoriesCache();

                    return RedirectToAction("Index");
                }
            }
            return View(model);
        }

        private async Task UpdateActiveProductCategoriesCache()
        {
            // Lấy danh sách các danh mục chưa xóa mềm từ cơ sở dữ liệu
            var activeItems = db.ProductCategories.Where(c => !c.IsDelete).OrderBy(x => x.Title).ToList();

            // Chuyển danh sách sang JSON và lưu vào cache
            var serializedProductCategories = Newtonsoft.Json.JsonConvert.SerializeObject(activeItems);
            await redisDB.StringSetAsync("ProductCategories_active_all_1", serializedProductCategories, TimeSpan.FromHours(1));
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            var ProductCategories = await db.ProductCategories.FindAsync(id);
            if (ProductCategories != null)
            {
                // Mark the ProductCategories as deleted
                ProductCategories.IsDelete = true;
                await db.SaveChangesAsync();

                // Delete relevant cache keys (including the active ProductCategories cache)
                await redisDB.KeyDeleteAsync("ProductCategories_active_all_1");

                // Return success response
                return Json(new { success = true });
            }

            // If ProductCategories not found, return failure
            return Json(new { success = false });
        }
        // GET: Admin/ProductCategories/Trash
        public async Task<ActionResult> Trash(string searchProductCategories, int? page)
        {
            string cacheKey = $"ProductCategories_trash_{searchProductCategories ?? "all"}_{page ?? 1}";
            int pageSize = 10;
            int pageNumber = page ?? 1;
            var items = db.ProductCategories.Where(c => c.IsDelete).AsQueryable();
            if (!string.IsNullOrEmpty(searchProductCategories))
            {
                items = items.Where(x => x.Title.Contains(searchProductCategories) ||
                                         x.Description.Contains(searchProductCategories) ||
                                         x.Id.ToString().Contains(searchProductCategories));
            }

            var ProductCategoriesList = items.OrderBy(x => x.Title).ToPagedList(pageNumber, pageSize);
            var serializedProductCategories = Newtonsoft.Json.JsonConvert.SerializeObject(ProductCategoriesList);
            await redisDB.StringSetAsync(cacheKey, serializedProductCategories, TimeSpan.FromHours(1));
            return View(ProductCategoriesList);
        }
        [HttpPost]
        public async Task<ActionResult> Restore(int id)
        {
            var item = await db.ProductCategories.FindAsync(id);
            if (item != null)
            {
                item.IsDelete = false;
                await db.SaveChangesAsync();

                // Xóa cache của danh mục
                await redisDB.KeyDeleteAsync($"ProductCategories_{id}");

                // Xóa và tải lại danh sách danh mục đã xóa từ Redis
                await redisDB.KeyDeleteAsync("ProductCategories_trash_all_1");
                await redisDB.KeyDeleteAsync("ProductCategories_active_all_1");

                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteDB(int id)
        {
            var item = await db.ProductCategories.FindAsync(id);
            if (item != null)
            {

                // Xóa các bản ghi liên quan trong bảng Post
               

                // Xóa bản ghi ProductCategories
                db.ProductCategories.Remove(item);
                await db.SaveChangesAsync();

                // Xóa cache
                await redisDB.KeyDeleteAsync("ProductCategories_trash_all_1");

                return Json(new { success = true });
            }

            return Json(new { success = false });
        }
    }
}
