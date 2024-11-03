using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ShoeShopDuAn.Models.SP
{
    [Table("tb_ProductCategory")]
    public class ProductCategories : CommonAbstract
    {
        internal string SeoTitle;

        public ProductCategories()
        {

            this.Products = new HashSet<Product>();
        }
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required()]
        [StringLength(120)]
        public string Title { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
        public string Alias { get; set; }
        public int Position { get; set; }
        public bool IsDelete
        {
            get; set;
        }
        public ICollection<Product> Products { get; set; }

    }
}