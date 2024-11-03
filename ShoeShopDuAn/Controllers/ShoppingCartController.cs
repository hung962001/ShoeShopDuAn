using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShoeShopDuAn.Models;
using ShoeShopDuAn.Models.SP;
using ClosedXML.Excel;
using System.IO;

namespace ShoeShopDuAn.Controllers
{
    public class ShoppingCartController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: ShoppingCart
        public ActionResult Index()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null && cart.Items.Any())
            {
                ViewBag.CheckCart = cart;
            }
            return View();
        }
        public ActionResult CheckOut()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null && cart.Items.Any())
            {
                ViewBag.CheckCart = cart;
            }
            return View();
        }
        public ActionResult CheckOutSuccess()
        {
            return View();
        }
        public ActionResult Partial_Item_ThanhToan()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null && cart.Items.Any())
            {
                return PartialView(cart.Items);
            }
            return PartialView();
        }

        public ActionResult Partial_Item_Cart()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null && cart.Items.Any())
            {
                return PartialView(cart.Items);
            }
            return PartialView();
        }
        public ActionResult ExportInvoiceToExcel(int orderId)
        {
            // Lấy thông tin hóa đơn từ cơ sở dữ liệu
            var order = db.Orders.Include("OrderItems").FirstOrDefault(o => o.Id == orderId);

            if (order == null)
            {
                return HttpNotFound();
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Hóa đơn");

                // Tiêu đề
                worksheet.Cell("A1").Value = "Hóa đơn bán hàng";
                worksheet.Cell("A1").Style.Font.Bold = true;
                worksheet.Cell("A1").Style.Font.FontSize = 16;

                // Thông tin khách hàng
                worksheet.Cell("A3").Value = "Tên khách hàng:";
                worksheet.Cell("B3").Value = order.CustomerName;
                worksheet.Cell("A4").Value = "Địa chỉ:";
                worksheet.Cell("B4").Value = order.Address;
                worksheet.Cell("A5").Value = "Ngày đặt hàng:";
                worksheet.Cell("B5").Value = order.CreatedDate.ToString("dd/MM/yyyy");

                // Tiêu đề bảng
                worksheet.Cell("A7").Value = "Sản phẩm";
                worksheet.Cell("B7").Value = "Số lượng";
                worksheet.Cell("C7").Value = "Giá (VND)";
                worksheet.Cell("D7").Value = "Thành tiền (VND)";

                // Định dạng tiêu đề bảng
                var headerRange = worksheet.Range("A7:D7");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                // Dữ liệu sản phẩm
                int row = 8;
                foreach (var item in order.OrderDetails)
                {
                    var productName = db.Products
                                        .Where(p => p.Id == item.ProductId)
                                        .Select(p => p.Title)
                                        .FirstOrDefault(); // Lấy tên sản phẩm theo ProductId

                    worksheet.Cell(row, 1).Value = item.ProductId;
                    worksheet.Cell(row, 2).Value = productName;  // Thêm tên sản phẩm vào cột
                    worksheet.Cell(row, 3).Value = item.Quantity;
                    worksheet.Cell(row, 4).Value = item.Price;
                    worksheet.Cell(row, 5).Value = item.Quantity * item.Price;
                    row++;
                }
                    // Tính tổng tiền
                    worksheet.Cell(row, 3).Value = "Tổng cộng:";
                worksheet.Cell(row, 4).Value = order.TotalAmount;
                worksheet.Cell(row, 3).Style.Font.Bold = true;
                worksheet.Cell(row, 4).Style.Font.Bold = true;

                // Thiết lập căn chỉnh và đường viền
                worksheet.Columns().AdjustToContents();
                worksheet.Range("A7:D" + row).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                worksheet.Range("A7:D" + row).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // Tạo file và xuất ra dưới dạng stream
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;

                    // Xuất file Excel
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "HoaDon.xlsx");
                }
            }
        }

        public ActionResult ShowCount()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null)
            {
                return Json(new { Count = cart.Items.Count }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Count = 0 }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Partial_CheckOut()
        {
            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckOut(OrderViewModel req)
        {
            var code = new { Success = false, Code = -1 };
            if (ModelState.IsValid)
            {
                ShoppingCart cart = (ShoppingCart)Session["Cart"];
                if (cart != null)
                {
                    Order order = new Order();
                    order.CustomerName = req.CustomerName;
                    order.Phone = req.Phone;
                    order.Address = req.Address;
                    order.Email = req.Email;
                    cart.Items.ForEach(x => order.OrderDetails.Add(new OrderDetail
                    {
                        ProductId = x.ProductId,
                        Quantity = x.Quantity,
                        Price = x.Price
                    }));
                    order.TotalAmount = cart.Items.Sum(x => (x.Price * x.Quantity));
                    order.TypePayment = req.TypePayment;
                    order.CreatedDate = DateTime.Now;
                    order.ModifiedDate = DateTime.Now;
                    order.CreatedBy = req.Phone;
                    Random rd = new Random();
                    order.Code = "DH" + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9);
                    //order.E = req.CustomerName;
                    db.Orders.Add(order);
                    db.SaveChanges();
                    //send mail cho khachs hang
                    var strSanPham = "";
                    var thanhtien = decimal.Zero;
                    var TongTien = decimal.Zero;
                    foreach (var sp in cart.Items)
                    {
                        strSanPham += "<tr>";
                        strSanPham += "<td>" + sp.ProductName + "</td>";
                        strSanPham += "<td>" + sp.Quantity + "</td>";
                        strSanPham += "<td>" + ShoeShopDuAn.Common.Common.FormatNumber(sp.TotalPrice, 0) + "</td>";
                        strSanPham += "</tr>";
                        thanhtien += sp.Price * sp.Quantity;
                    }
                    TongTien = thanhtien;
                    string contentCustomer = System.IO.File.ReadAllText(Server.MapPath("~/Content/templates/send2.html"));
                    contentCustomer = contentCustomer.Replace("{{MaDon}}", order.Code);
                    contentCustomer = contentCustomer.Replace("{{SanPham}}", strSanPham);
                    contentCustomer = contentCustomer.Replace("{{NgayDat}}", DateTime.Now.ToString("dd/MM/yyyy"));
                    contentCustomer = contentCustomer.Replace("{{TenKhachHang}}", order.CustomerName);
                    contentCustomer = contentCustomer.Replace("{{Phone}}", order.Phone);
                    contentCustomer = contentCustomer.Replace("{{Email}}", req.Email);
                    contentCustomer = contentCustomer.Replace("{{DiaChiNhanHang}}", order.Address);
                    contentCustomer = contentCustomer.Replace("{{ThanhTien}}", ShoeShopDuAn.Common.Common.FormatNumber(thanhtien, 0));
                    contentCustomer = contentCustomer.Replace("{{TongTien}}", ShoeShopDuAn.Common.Common.FormatNumber(TongTien, 0));
                    ShoeShopDuAn.Common.Common.SendMail("ShopOnline", "Đơn hàng #" + order.Code, contentCustomer.ToString(), req.Email);

                    string contentAdmin = System.IO.File.ReadAllText(Server.MapPath("~/Content/templates/send1.html"));
                    contentAdmin = contentAdmin.Replace("{{MaDon}}", order.Code);
                    contentAdmin = contentAdmin.Replace("{{SanPham}}", strSanPham);
                    contentAdmin = contentAdmin.Replace("{{NgayDat}}", DateTime.Now.ToString("dd/MM/yyyy"));
                    contentAdmin = contentAdmin.Replace("{{TenKhachHang}}", order.CustomerName);
                    contentAdmin = contentAdmin.Replace("{{Phone}}", order.Phone);
                    contentAdmin = contentAdmin.Replace("{{Email}}", req.Email);
                    contentAdmin = contentAdmin.Replace("{{DiaChiNhanHang}}", order.Address);
                    contentAdmin = contentAdmin.Replace("{{ThanhTien}}", ShoeShopDuAn.Common.Common.FormatNumber(thanhtien, 0));
                    contentAdmin = contentAdmin.Replace("{{TongTien}}", ShoeShopDuAn.Common.Common.FormatNumber(TongTien, 0));
                    ShoeShopDuAn.Common.Common.SendMail("ShopOnline", "Đơn hàng mới #" + order.Code, contentAdmin.ToString(), ConfigurationManager.AppSettings["EmailAdmin"]);
                    cart.ClearCart();
                    return RedirectToAction("CheckOutSuccess");
                }
            }
            return Json(code);
        }

        [HttpPost]
        public ActionResult AddToCart(int id, decimal quantity)
        {
            var code = new { Success = false, msg = "", code = -1, Count = 0 };
            var db = new ApplicationDbContext();
            var checkProduct = db.Products.FirstOrDefault(x => x.Id == id);
            if (checkProduct != null)
            {
                ShoppingCart cart = (ShoppingCart)Session["Cart"] ?? new ShoppingCart();

                ShoppingCartItem item = new ShoppingCartItem
                {
                    ProductId = checkProduct.Id,
                    ProductName = checkProduct.Title,
                    CategoryName = checkProduct.ProductCategory?.Title ?? "Không có danh mục",
                    Alias = checkProduct.Alias,
                    Quantity = quantity, // This should work now
                    Price = checkProduct.PriceSale > 0 ? (decimal)checkProduct.PriceSale : checkProduct.Price,
                    TotalPrice = quantity * (checkProduct.PriceSale > 0 ? (decimal)checkProduct.PriceSale : checkProduct.Price)
                };

                item.ProductImg = checkProduct.ProductImage.FirstOrDefault(x => x.IsDefault)?.Image;

                cart.AddToCart(item, quantity);
                Session["Cart"] = cart;
                code = new { Success = true, msg = "Thêm sản phẩm vào giỏ hàng thành công!", code = 1, Count = cart.Items.Count };
            }
            return Json(code);
        }

        [HttpPost]
        public ActionResult Update(int id, decimal quantity)
        {
            // Kiểm tra nếu quantity hợp lệ
            if (quantity <= 0)
            {
                return Json(new { Success = false, msg = "Số lượng phải lớn hơn 0." });
            }

            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null)
            {
                cart.UpdateQuantity(id, quantity);
                return Json(new { Success = true });
            }
            return Json(new { Success = false });
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var code = new { Success = false, msg = "", code = -1, Count = 0 };

            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null)
            {
                var checkProduct = cart.Items.FirstOrDefault(x => x.ProductId == id);
                if (checkProduct != null)
                {
                    cart.Remove(id);
                    code = new { Success = true, msg = "", code = 1, Count = cart.Items.Count };
                }
            }
            return Json(code);
        }



        [HttpPost]
        public ActionResult DeleteAll()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null)
            {
                cart.ClearCart();
                return Json(new { Success = true });
            }
            return Json(new { Success = false });
        }
    }
}