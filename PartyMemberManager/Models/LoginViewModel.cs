using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PartyMemberManager.Models
{
    /// <summary>
    /// 用户登录视图模型
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// 工号
        /// </summary>
        [DisplayName("工号")]
        [Required]
        public string LoginName { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        [DisplayName("密码")]
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        /// <summary>
        /// 是否记住密码
        /// </summary>
        [DisplayName("是否记住密码")]
        public bool IsRemember { get; set; }
    }
}
