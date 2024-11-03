using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ShoeShopDuAn.Models.SP
{
    [Table("tb_Contact")]
    public class Contact : CommonAbstract
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required(ErrorMessage = "Tên không để trống")]
        [StringLength(30, ErrorMessage = "Không vượt quá 30")]
        public string Name { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
    }
}