using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace ShoeShopDuAn.Models.SP
{
    [Table("tb_Product")]
    public class Product : CommonAbstract
    {
        public Product()
        {
            this.ProductImage = new HashSet<ProductImage>();
            this.OrderDetail = new HashSet<OrderDetail>();
        }

        private const string V = "URL hình ảnh không được vượt quá 255 ký tự.";

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [StringLength(130)]
        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        public string Title { get; set; }

        [StringLength(50)]
        public string ProductCode { get; set; }

        [Required(ErrorMessage = "Danh mục không được để trống")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Số lượng không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public decimal  Quantity { get; set; }

        [Required(ErrorMessage = "Giá không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá không hợp lệ")]
        public decimal Price { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá khuyến mãi không hợp lệ")]
        public decimal? PriceSale { get; set; }

        public string Description { get; set; }

        [System.Web.Mvc.AllowHtml]
        public string Detail { get; set; }

        [StringLength(255, ErrorMessage = V)]
        public string Image { get; set; }

        public string Alias { get; set; }

        public decimal InputPrice { get; set; }

        public bool IsFeature { get; set; }

        public bool IsVisible { get; set; }

        // Thêm thuộc tính [JsonIgnore] để tránh vòng lặp tham chiếu
        [JsonIgnore]
        public virtual ICollection<ProductImage> ProductImage { get; set; }

        [JsonIgnore]
        public virtual ProductCategories ProductCategory { get; set; }

        [JsonIgnore]
        public virtual ICollection<OrderDetail> OrderDetail { get; set; }
    }
}
