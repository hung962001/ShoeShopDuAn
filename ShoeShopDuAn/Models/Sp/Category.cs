
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

namespace ShoeShopDuAn.Models.SP
{
    public class Category : CommonAbstract
    {

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required(ErrorMessage = "Tên danh mục không được trống")]
        [StringLength(150)]
        public string Title { get; set; }
        public string Alias { get; set; }
        public string SeoTitle { get; set; }
        public string Description { get; set; }
        public bool isDeleted { get; set; }
        public ICollection<Posts> Posts { get; set; }
    }
}
