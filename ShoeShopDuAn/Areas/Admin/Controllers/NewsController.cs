using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using PagedList;
using ShoeShopDuAn.Models;
using StackExchange.Redis;
using System.Configuration;
using Newtonsoft.Json;
using ShoeShopDuAn.Models.SP;

namespace ShoeShopDuAn.Areas.Admin.Controllers
{
   
    public class NewsController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();
        private readonly ConnectionMultiplexer redisConnection;
        private readonly IDatabase redisDB;
       
        public NewsController()
        {
            var redisConnectionString = ConfigurationManager.ConnectionStrings["RedisConnection"].ConnectionString;
            redisConnection = ConnectionMultiplexer.Connect(redisConnectionString);
            redisDB = redisConnection.GetDatabase();
        }

        // GET: Admin/News
        public async Task<ActionResult> Index(string searchNews, int? page)
        {
            string cacheKey = $"News_active_{searchNews ?? "all"}_{page ?? 1}";
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var items = db.News.Where(c => !c.IsDelete).AsQueryable();

            if (!string.IsNullOrEmpty(searchNews))
            {
                items = items.Where(x => x.Title.Contains(searchNews) ||
                                         x.Description.Contains(searchNews) ||
                                         x.Id.ToString().Contains(searchNews));
            }

            var categoryList = items.OrderBy(x => x.Title).ToPagedList(pageNumber, pageSize);
            var serializedNews = Newtonsoft.Json.JsonConvert.SerializeObject(categoryList);

            await redisDB.StringSetAsync(cacheKey, serializedNews, TimeSpan.FromHours(1));

            return View(categoryList);
        }

        // Add Action
        public ActionResult Add()
        {
            var News = db.News.ToList();
            ViewBag.News = new SelectList(News, "Id", "Title");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Add(News model)
        {
            if (ModelState.IsValid)
            {
              
                    model.CreatedDate = DateTime.Now;
                    model.ModifiedDate = DateTime.Now;
                    model.Alias = ShoeShopDuAn.Models.Common.Filter.ChuyenCoDauThanhKhongDau(model.Title);
                    db.News.Add(model);
                    await db.SaveChangesAsync();

                    await redisDB.KeyDeleteAsync("news_active_all_1");

                    return RedirectToAction("Index");
                
            }

        
            return View(model);
        }

        // Edit Action
        public async Task<ActionResult> Edit(int id)
        {
            string cacheKey = $"news_{id}";
            var cachedNews = await redisDB.StringGetAsync(cacheKey);
            News newsItem;

            if (!string.IsNullOrEmpty(cachedNews))
            {
                newsItem = JsonConvert.DeserializeObject<News>(cachedNews);
            }
            else
            {
                newsItem = await db.News.FindAsync(id);
                if (newsItem == null)
                {
                    return HttpNotFound();
                }
                var serializedNews = JsonConvert.SerializeObject(newsItem);
                await redisDB.StringSetAsync(cacheKey, serializedNews, TimeSpan.FromDays(1));
            }

           
            return View(newsItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(News model)
        {
            if (ModelState.IsValid)
            {
                var existingNews = await db.News.FindAsync(model.Id);
                if (existingNews == null)
                {
                    return HttpNotFound();
                }

                existingNews.Title = model.Title;
                existingNews.Description = model.Description;
                existingNews.Detail = model.Detail;
                existingNews.Alias = ShoeShopDuAn.Models.Common.Filter.ChuyenCoDauThanhKhongDau(model.Title);
                existingNews.ModifiedDate = DateTime.Now;
                existingNews.SeoTitle = model.SeoTitle;

                await db.SaveChangesAsync();

                var serializedNews = JsonConvert.SerializeObject(existingNews);
                await redisDB.StringSetAsync($"news_{existingNews.Id}", serializedNews, TimeSpan.FromDays(1));

                await redisDB.KeyDeleteAsync("news_active_all_1");

                return RedirectToAction("Index");
            }

           
            return View(model);
        }

        // Trash Action
        public async Task<ActionResult> Trash(string searchNews, int? page)
        {
            string cacheKey = $"News_trash_{searchNews ?? "all"}_{page ?? 1}";
            int pageSize = 10;
            int pageNumber = page ?? 1;
            var items = db.News.Where(c => c.IsDelete).AsQueryable();
            if (!string.IsNullOrEmpty(searchNews))
            {
                items = items.Where(x => x.Title.Contains(searchNews) ||
                                         x.Description.Contains(searchNews) ||
                                         x.Id.ToString().Contains(searchNews));
            }

            var categoryList = items.OrderBy(x => x.Title).ToPagedList(pageNumber, pageSize);
            var serializedNews = Newtonsoft.Json.JsonConvert.SerializeObject(categoryList);
            await redisDB.StringSetAsync(cacheKey, serializedNews, TimeSpan.FromHours(1));
            return View(categoryList);
        }

        // Delete Action
        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            var newsItem = await db.News.FindAsync(id);
            if (newsItem != null)
            {
                newsItem.IsDelete = true;
                await db.SaveChangesAsync();

                await redisDB.KeyDeleteAsync($"news_{id}");
                await redisDB.KeyDeleteAsync("news_active_all_1");

                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        // Permanently delete from database
        [HttpPost]
        public async Task<ActionResult> DeleteDB(int id)
        {
            var newsItem = await db.News.FindAsync(id);
            if (newsItem != null)
            {
                db.News.Remove(newsItem);
                await db.SaveChangesAsync();

                await redisDB.KeyDeleteAsync($"news_{id}");
                await redisDB.KeyDeleteAsync("news_active_all_1");

                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        // Toggle visibility
        [HttpPost]
        public async Task<ActionResult> Isvisible(int id)
        {
            var newsItem = await db.News.FindAsync(id);
            if (newsItem != null)
            {
                newsItem.Isvisible = !newsItem.Isvisible;
                db.Entry(newsItem).State = System.Data.Entity.EntityState.Modified;
                await db.SaveChangesAsync();

                var serializedNews = JsonConvert.SerializeObject(newsItem);
                await redisDB.StringSetAsync($"news_{id}", serializedNews, TimeSpan.FromDays(1));

                return Json(new { success = true, Isvisible = newsItem.Isvisible });
            }

            return Json(new { success = false });
        }
    }
}
