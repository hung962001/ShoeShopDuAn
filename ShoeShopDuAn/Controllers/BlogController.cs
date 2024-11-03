using PagedList;
using ShoeShopDuAn.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShoeShopDuAn.Controllers
{
    public class BlogController : Controller
    {
        // GET: Blog
        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index(string searchNews, string sortOrder, int? page)
        {
            var blogs = from b in db.News select b;

            // Tìm kiếm theo từ khóa
            if (!String.IsNullOrEmpty(searchNews))
            {
                blogs = blogs.Where(b => b.Title.Contains(searchNews));
            }

            // Sắp xếp theo tiêu chí
            switch (sortOrder)
            {
                case "az":
                    blogs = blogs.OrderBy(b => b.Title);
                    break;
                case "za":
                    blogs = blogs.OrderByDescending(b => b.Title);
                    break;
                case "date_desc":
                    blogs = blogs.OrderByDescending(b => b.CreatedDate);
                    break;
                case "date_asc":
                    blogs = blogs.OrderBy(b => b.CreatedDate);
                    break;
                default:
                    blogs = blogs.OrderBy(b => b.CreatedDate); // Mặc định sắp xếp theo ngày mới đến cũ
                    break;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(blogs.Where(c=>!c.IsDelete).ToPagedList(pageNumber, pageSize));
        }
        public ActionResult LastestBlog( int take=12)
        {
            var items = db.News.Where(c => !c.IsDelete).OrderByDescending(c => c.CreatedDate).Take(take).ToList();
            return View(items);
        }
        public ActionResult DetailNews(int id)
        {
            var items = db.News.Find(id);
            return View(items);
        }
      
    }
}