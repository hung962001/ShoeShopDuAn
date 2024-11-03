using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShoeShopDuAn.Models.SP
{
    [Table("tb_Posts")]
    public class Posts : CommonAbstract
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [StringLength(300)]
        public string Title { get; set; }
        public string Description { get; set; }
        [AllowHtml]
        public string Detail { get; set; }
        [StringLength(300)]
        public string Alias { get; set; }
        [StringLength(300)]
        public string Image { get; set; }
        public int CategoryId { get; set; }       
        public string SeoTitle { get; set; }
        [StringLength(300)]
        public string SeoDescription { get; set; }
        public string SeoKeywords { get; set; }
        public bool Isvisible { get; set; }
        public bool IsDelete { get; set; }
        public int ProductId { get; set; }  
        public virtual Product Product { get; set; } 
        public virtual Category Category { get; set; }
    }
}