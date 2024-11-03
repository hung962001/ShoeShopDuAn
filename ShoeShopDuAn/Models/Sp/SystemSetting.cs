using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ShoeShopDuAn.Models.SP
{
    public class SytemSetting
    {
        [Key]
        [StringLength(30)]
        public string SettingKey { get; set; }
        [StringLength(300)]
        public string SettingValue { get; set; }
        [StringLength(350)]
        public string SettingDescription { get; set; }
    }
}