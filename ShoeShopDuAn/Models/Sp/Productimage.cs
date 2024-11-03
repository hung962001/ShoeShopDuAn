using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ShoeShopDuAn.Models.SP
{
    public class ProductImage
    {
      
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ProductId { get; set; }
        [StringLength(255, ErrorMessage = "URL không được vượt quá 255 ký tự.")]
        public string Image { get; set; }
        public bool IsDefault { get; set; }
        
        public virtual Product Product { get; set; } 
    }
}