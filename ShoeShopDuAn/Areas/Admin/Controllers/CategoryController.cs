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
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();
        private readonly ConnectionMultiplexer redisConnection;
        private readonly IDatabase redisDB;

        public CategoryController()
        {
            var redisConnectionString = ConfigurationManager.ConnectionStrings["RedisConnection"].ConnectionString;
            redisConnection = ConnectionMultiplexer.Connect(redisConnectionString);
            redisDB = redisConnection.GetDatabase();
        }
        public async Task<ActionResult> Index(string searchCategories, int? page)
        {
            string cacheKey = $"categories_active_{searchCategories ?? "all"}_{page ?? 1}";
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var items = db.Categories.Where(c => !c.isDeleted).AsQueryable();

            if (!string.IsNullOrEmpty(searchCategories))
            {
                items = items.Where(x => x.Title.Contains(searchCategories) ||
                                         x.Description.Contains(searchCategories) ||
                                         x.Id.ToString().Contains(searchCategories));
            }

            var categoryList = items.OrderBy(x => x.Title).ToPagedList(pageNumber, pageSize);
            var serializedCategories = Newtonsoft.Json.JsonConvert.SerializeObject(categoryList);

            await redisDB.StringSetAsync(cacheKey, serializedCategories, TimeSpan.FromHours(1));

            return View(categoryList);
        }


        public async Task<ActionResult> Edit(int id)
        {
            string cacheKey = $"category_{id}";
            var cacheCategory = await redisDB.StringGetAsync(cacheKey);
            Category item;

            if (!string.IsNullOrEmpty(cacheCategory))
            {
                item = Newtonsoft.Json.JsonConvert.DeserializeObject<Category>(cacheCategory);
            }
            else
            {
                item = await db.Categories.FindAsync(id);
                if (item == null)
                {
                    return HttpNotFound();
                }
                var serializedCategory = Newtonsoft.Json.JsonConvert.SerializeObject(item);
                await redisDB.StringSetAsync(cacheKey, serializedCategory, TimeSpan.FromDays(1));
            }

            return View(item);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Category model)
        {
            if (ModelState.IsValid)
            {
    
                var existingCategory = await db.Categories.FindAsync(model.Id);
                if (existingCategory == null)
                {
                    return HttpNotFound();
                }

                existingCategory.Title = model.Title;
                existingCategory.Description = model.Description;
                existingCategory.Alias = ShoeShopDuAn.Models.Common.Filter.ChuyenCoDauThanhKhongDau(model.Title);
                existingCategory.SeoTitle = model.SeoTitle;
                existingCategory.ModifiedDate = DateTime.Now;
                existingCategory.ModifiedBy = model.ModifiedBy;

                await db.SaveChangesAsync();

             
                var serializedCategory = Newtonsoft.Json.JsonConvert.SerializeObject(existingCategory);
                await redisDB.StringSetAsync($"category_{existingCategory.Id}", serializedCategory, TimeSpan.FromDays(1));

      
                await redisDB.KeyDeleteAsync("categories_active_*"); // Delete all active categories caches

         
                await UpdateActiveCategoriesCache();

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
        public async Task<ActionResult> Add(Category model)
        {
            if (ModelState.IsValid)
            {
                var existingCategories = db.Categories
                    .Where(c => c.Title.Equals(model.Title, StringComparison.OrdinalIgnoreCase) && !c.isDeleted)
                    .ToList();

                if (existingCategories.Any())
                {
                    ModelState.AddModelError("Title", "Tên danh mục đã tồn tại");
                }
                else
                {
                    model.CreatedDate = DateTime.Now;
                    model.ModifiedDate = DateTime.Now;
                    model.Alias = ShoeShopDuAn.Models.Common.Filter.ChuyenCoDauThanhKhongDau(model.Title);
                    db.Categories.Add(model);
                    await db.SaveChangesAsync();

                    // Delete all old cache for active categories
                    await redisDB.KeyDeleteAsync("categories_active_*");

                    // Update the cache with the new list of active categories
                    await UpdateActiveCategoriesCache();

                    return RedirectToAction("Index");
                }
            }
            return View(model);
        }

        private async Task UpdateActiveCategoriesCache()
        {
            // Lấy danh sách các danh mục chưa xóa mềm từ cơ sở dữ liệu
            var activeItems = db.Categories.Where(c => !c.isDeleted).OrderBy(x => x.Title).ToList();

            // Chuyển danh sách sang JSON và lưu vào cache
            var serializedCategories = Newtonsoft.Json.JsonConvert.SerializeObject(activeItems);
            await redisDB.StringSetAsync("categories_active_all_1", serializedCategories, TimeSpan.FromHours(1));
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            var category = await db.Categories.FindAsync(id);
            if (category != null)
            {
                // Mark the category as deleted
                category.isDeleted = true;
                await db.SaveChangesAsync();

                // Delete relevant cache keys (including the active categories cache)
                await redisDB.KeyDeleteAsync("categories_active_all_1");

                // Return success response
                return Json(new { success = true });
            }

            // If category not found, return failure
            return Json(new { success = false });
        }
        // GET: Admin/Category/Trash
        public async Task<ActionResult> Trash(string searchCategories, int? page)
        {
            string cacheKey = $"categories_trash_{searchCategories ?? "all"}_{page ?? 1}";
            int pageSize = 10;
            int pageNumber = page ?? 1;
            var items = db.Categories.Where(c => c.isDeleted).AsQueryable();
            if (!string.IsNullOrEmpty(searchCategories))
            {
                items = items.Where(x => x.Title.Contains(searchCategories) ||
                                         x.Description.Contains(searchCategories) ||
                                         x.Id.ToString().Contains(searchCategories));
            }

            var categoryList = items.OrderBy(x => x.Title).ToPagedList(pageNumber, pageSize);
            var serializedCategories = Newtonsoft.Json.JsonConvert.SerializeObject(categoryList);
            await redisDB.StringSetAsync(cacheKey, serializedCategories, TimeSpan.FromHours(1));
            return View(categoryList);
        }
        [HttpPost]
        public async Task<ActionResult> Restore(int id)
        {
            var item = await db.Categories.FindAsync(id);
            if (item != null)
            {
                item.isDeleted = false;
                await db.SaveChangesAsync();

                // Xóa cache của danh mục
                await redisDB.KeyDeleteAsync($"category_{id}");

                // Xóa và tải lại danh sách danh mục đã xóa từ Redis
                await redisDB.KeyDeleteAsync("categories_trash_all_1");
                await redisDB.KeyDeleteAsync("categories_active_all_1");

                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteDB(int id)
        {
            var item = await db.Categories.FindAsync(id);
            if (item != null)
            {
                        
                // Xóa các bản ghi liên quan trong bảng Post
                var relatedPosts = db.Posts.Where(p => p.CategoryId == id).ToList();
                db.Posts.RemoveRange(relatedPosts);

                // Xóa bản ghi Category
                db.Categories.Remove(item);
                await db.SaveChangesAsync();

                // Xóa cache
                await redisDB.KeyDeleteAsync("categories_trash_all_1");

                return Json(new { success = true });
            }

            return Json(new { success = false });
        }
    }
}
