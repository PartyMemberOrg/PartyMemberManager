using PartyMemberManager.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PartyMemberManager.Dal.Entities
{
    public class PartyActivist :PartyMemberBase
    {
        /// <summary>
        /// 提交入党申请时间
        /// </summary>
        [DisplayName("提交入党申请时间")]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public DateTime ApplicationTime { get; set; }

        /// <summary>
        /// 确定入党积极分子时间
        /// </summary>
        [DisplayName("确定入党积极分子时间")]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public DateTime ActiveApplicationTime { get; set; }
        /// <summary>
        /// 担任职务
        /// </summary>
        [DisplayName("担任职务")]
        public string Duty { get; set; }
    }
}
