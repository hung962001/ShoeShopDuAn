using PagedList;
using ShoeShopDuAn.Models;
using ShoeShopDuAn.Models.SP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using StackExchange.Redis;
using System.Configuration;
using System.Data.Entity;

namespace ShoeShopDuAn.Areas.Admin.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ConnectionMultiplexer redisConnection;
        private readonly IDatabase redisDB;
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // Khởi tạo kết nối Redis trong constructor

        public ProductsController()
        {
            var redisConnectionString = ConfigurationManager.ConnectionStrings["RedisConnection"].ConnectionString;
            redisConnection = ConnectionMultiplexer.Connect(redisConnectionString);
            redisDB = redisConnection.GetDatabase();
        }

        // GET: Admin/Products
        public async Task<ActionResult> Index(string searchProduct, int? page)
        {
            await DeleteProductsWithNoCategory();
            string cacheKey = $"products_{searchProduct ?? "all"}_{page ?? 1}";
            int pageSize = 10;
            int pageNumber = page ?? 1;

            // Khởi tạo truy vấn cho danh sách sản phẩm
            var items = db.Products
                .Where(c => !c.IsFeature && c.CategoryId != 0) // Lọc sản phẩm không phải là sản phẩm nổi bật và CategoryId khác 0
                .AsQueryable();

            // Nếu có từ khóa tìm kiếm, thêm điều kiện vào truy vấn
            if (!string.IsNullOrEmpty(searchProduct))
            {
                items = items.Where(x => x.Title.Contains(searchProduct) ||
                                         x.Description.Contains(searchProduct) ||
                                         x.Id.ToString().Contains(searchProduct));
            }

            // Sắp xếp sản phẩm theo Title và phân trang
            var categories = await db.Categories.ToListAsync();
            var ProductList = items.OrderBy(x => x.Title).ToPagedList(pageNumber, pageSize);         
            // Serialize danh sách sản phẩm để lưu vào cache
            var serializedProductList = Newtonsoft.Json.JsonConvert.SerializeObject(ProductList);
            await redisDB.StringSetAsync(cacheKey, serializedProductList, TimeSpan.FromHours(1));
           
            // Trả về view với danh sách sản phẩm
            return View(ProductList);
        }


        public async Task<ActionResult> Add()
        {
            var cacheCategories = await redisDB.StringGetAsync("categories");
            List<ProductCategories> categories;

            if (!string.IsNullOrEmpty(cacheCategories))
            {
                categories = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ProductCategories>>(cacheCategories);
            }
            else
            {
                // Lấy danh mục với IsDelete = false
                categories = await db.ProductCategories
                    .Where(c => !c.IsDelete) // Thêm điều kiện IsDelete là false
                    .ToListAsync();

                var serializedCategories = Newtonsoft.Json.JsonConvert.SerializeObject(categories);
                await redisDB.StringSetAsync("categories", serializedCategories, TimeSpan.FromDays(1));
            }

            ViewBag.ProductCategory = new SelectList(categories, "Id", "Title");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Add(Product model, List<string> Images, List<int> rDefault)
        {
            if (ModelState.IsValid)
            {
                if (Images != null && Images.Count > 0)
                {
                    model.ProductImage = new HashSet<ProductImage>();

                    for (int i = 0; i < Images.Count; i++)
                    {
                        var isDefault = rDefault != null && rDefault.Contains(i + 1);

                        var productImage = new ProductImage
                        {
                            ProductId = model.Id,
                            Image = Images[i],
                            IsDefault = isDefault
                        };

                        if (productImage.IsDefault)
                        {
                            model.Image = Images[i];
                        }

                        model.ProductImage.Add(productImage);
                    }
                }

                model.CreatedDate = DateTime.Now;
                model.ModifiedDate = DateTime.Now;

                if (string.IsNullOrEmpty(model.Alias))
                    model.Alias = ShoeShopDuAn.Models.Common.Filter.FilterChar(model.Title);

                db.Products.Add(model);
                await db.SaveChangesAsync();

                // Xóa cache sau khi thêm sản phẩm mới
                await redisDB.KeyDeleteAsync("products_all_1");  // Xóa cache cho trang sản phẩm

                // Cập nhật cache danh mục sản phẩm
                var cacheCategories = await redisDB.StringGetAsync("categories");
                if (!string.IsNullOrEmpty(cacheCategories))
                {
                    // Cache tồn tại, xóa cache hiện tại để cập nhật danh mục mới nhất
                    await redisDB.KeyDeleteAsync("categories");
                }

                return RedirectToAction("Index");
            }

            // Lấy danh sách danh mục từ Redis hoặc database nếu cache không tồn tại
            var cacheProductCategories = await redisDB.StringGetAsync("categories");
            List<ProductCategories> productCategories;

            if (!string.IsNullOrEmpty(cacheProductCategories))
            {
                // Lấy từ cache
                productCategories = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ProductCategories>>(cacheProductCategories);
            }
            else
            {
                // Truy vấn từ database nếu cache không có
                productCategories = await db.ProductCategories.Where(c => !c.IsDelete).ToListAsync();

                if (productCategories.Any())
                {
                    // Lưu cache danh mục vào Redis
                    var serializedCategories = Newtonsoft.Json.JsonConvert.SerializeObject(productCategories);
                    await redisDB.StringSetAsync("categories", serializedCategories, TimeSpan.FromDays(1));
                }
            }

            // Gán danh mục vào ViewBag
            ViewBag.ProductCategory = new SelectList(productCategories, "Id", "Title");
            return View(model);

        }
        public async Task<ActionResult> Edit(int id)
        {
            var item = await db.Products
                .Include(p => p.ProductImage) // Include ProductImage để lấy danh sách hình ảnh
                .FirstOrDefaultAsync(p => p.Id == id);

            if (item == null)
            {
                return HttpNotFound();
            }

            // Lấy danh sách danh mục từ database
            var categories = await db.ProductCategories.Where(c => !c.IsDelete).ToListAsync();

            // Tạo SelectList với giá trị đã chọn là item.CategoryId
            ViewBag.ProductCategory = new SelectList(categories, "Id", "Title", item.CategoryId);

            // Chuyển danh sách hình ảnh sang một danh sách URL để sử dụng trong view
            ViewBag.Images = item.ProductImage.Select(img => img.Image).ToList();

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Product model, List<string> Images, List<int> rDefault)
        {
            if (ModelState.IsValid)
            {
                var existingProduct = await db.Products.Include(p => p.ProductImage).FirstOrDefaultAsync(p => p.Id == model.Id);
                if (existingProduct != null)
                {
                    existingProduct.Title = model.Title;
                    existingProduct.Description = model.Description;
                    existingProduct.Alias = ShoeShopDuAn.Models.Common.Filter.ChuyenCoDauThanhKhongDau(model.Title);
                    existingProduct.ModifiedDate = DateTime.Now;
                    existingProduct.ModifiedBy = model.ModifiedBy;
                    existingProduct.CategoryId = model.CategoryId;
                    existingProduct.Price = model.Price;
                    // Xử lý hình ảnh
                    if (Images != null && Images.Count > 0)
                    {
                        // Xóa các hình ảnh hiện tại nếu cần
                        db.ProductImages.RemoveRange(existingProduct.ProductImage);
                        existingProduct.ProductImage = new HashSet<ProductImage>();

                        for (int i = 0; i < Images.Count; i++)
                        {
                            var isDefault = rDefault != null && rDefault.Contains(i + 1);

                            var productImage = new ProductImage
                            {
                                ProductId = existingProduct.Id,
                                Image = Images[i],
                                IsDefault = isDefault
                            };

                            if (productImage.IsDefault)
                            {
                                existingProduct.Image = Images[i];
                            }

                            existingProduct.ProductImage.Add(productImage);
                        }
                    }

                    db.Entry(existingProduct).State = System.Data.Entity.EntityState.Modified;
                    await db.SaveChangesAsync();

                    // Cập nhật cache sản phẩm
                    var serializedProduct = Newtonsoft.Json.JsonConvert.SerializeObject(existingProduct);
                    await redisDB.StringSetAsync($"product_{model.Id}", serializedProduct, TimeSpan.FromDays(1));

                    // Xóa cache danh sách sản phẩm
                    await redisDB.KeyDeleteAsync("products_all_1");

                    return RedirectToAction("Index");
                }
                return HttpNotFound();
            }

            ViewBag.ProductCategory = new SelectList(await db.ProductCategories.ToListAsync(), "Id", "Title");
            return View(model);
        }



        public async Task<ActionResult> Trash(string searchProducts, int? page)
        {

            string cacheKey = $"products_trash_{searchProducts ?? "all"}_{page ?? 1}";
            int pageSize = 10;
            int pageNumber = page ?? 1;
            var items = db.Products.Where(c => c.IsFeature ).AsQueryable();
            if (!string.IsNullOrEmpty(searchProducts))
            {
                items = items.Where(x => x.Title.Contains(searchProducts) ||
                                         x.Description.Contains(searchProducts) ||
                                         x.Id.ToString().Contains(searchProducts));
            }

            var ProductList = items.OrderBy(x => x.Title).ToPagedList(pageNumber, pageSize);
            var serializedProducts = Newtonsoft.Json.JsonConvert.SerializeObject(ProductList);
            await redisDB.StringSetAsync(cacheKey, serializedProducts, TimeSpan.FromHours(1));
            return View(ProductList);
        }
        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            var Products = await db.Products.FindAsync(id);
            if (Products != null)
            {
                // Mark the ProductCategories as deleted
                Products.IsFeature = true;
                await db.SaveChangesAsync();

                // Delete relevant cache keys (including the active ProductCategories cache)
                await redisDB.KeyDeleteAsync("Product_all_1");

                // Return success response
                return Json(new { success = true });
            }

            // If ProductCategories not found, return failure
            return Json(new { success = false });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteDB(int id)
        {
            var item = await db.Products.FindAsync(id);
            if (item != null)
            {
                db.Products.Remove(item);
                await db.SaveChangesAsync();

                // Xóa cache sản phẩm và danh sách
               
                await redisDB.KeyDeleteAsync("products_trash_all_1");

                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
        [HttpPost]
        public async Task<ActionResult> Restore(int id)
        {
            var item = await db.Products.FindAsync(id);
            if (item != null)
            {
                item.IsFeature = false;
                await db.SaveChangesAsync();

                // Xóa cache của danh mục
                await redisDB.KeyDeleteAsync($"product_{id}");

                // Xóa và tải lại danh sách danh mục đã xóa từ Redis
                await redisDB.KeyDeleteAsync("products_trash_all_1");
                await redisDB.KeyDeleteAsync("products_active_all_1");

                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public async Task<ActionResult> IsVisible(int id)
        {
            var item = await db.Products.FindAsync(id);
            if (item != null)
            {
                item.IsVisible = !item.IsVisible;
                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                await db.SaveChangesAsync();

                // Cập nhật cache sản phẩm
                var serializedProduct = Newtonsoft.Json.JsonConvert.SerializeObject(item);
                await redisDB.StringSetAsync($"product_{id}", serializedProduct, TimeSpan.FromDays(1));

                return Json(new { success = true, IsVisible = item.IsVisible });
            }
            return Json(new { success = false });
        }
        private async Task DeleteProductsWithNoCategory()
        {
            // Lấy danh sách các sản phẩm có ProductCategory = 0
            var productsToDelete = await db.Products
                .Include(p => p.ProductImage)
                .Where(p => p.CategoryId == 0)
                .ToListAsync();

            if (productsToDelete.Any())
            {
                // Xóa hình ảnh liên quan và sản phẩm
                foreach (var product in productsToDelete)
                {
                    db.ProductImages.RemoveRange(product.ProductImage);
                    db.Products.Remove(product);

                    // Xóa cache sản phẩm
                    await redisDB.KeyDeleteAsync($"product_{product.Id}");
                }

                await db.SaveChangesAsync();

                // Xóa cache danh sách sản phẩm
                await redisDB.KeyDeleteAsync("products_all_1");
            }
        }

    }
}
