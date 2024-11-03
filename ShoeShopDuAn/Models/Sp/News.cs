using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShoeShopDuAn.Models.SP
{
    [Table("tb_News")]
    public class News : CommonAbstract
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage ="Không để trống")]
        [StringLength(130)]
        public string Title { get; set; }
        public string Alias { get; set; }
        public string Description { get; set; }
        [AllowHtml]
        public string Detail { get; set; }
        public string Image { get; set; }
        public string SeoTitle { get; set; }
        public string SeoDescription { get; set; }
        public string SeoKeywords { get; set; }
        public bool Isvisible { get; set; }
        public bool IsDelete { get; set; }

    }
}