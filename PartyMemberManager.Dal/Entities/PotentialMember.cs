using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using PartyMemberManager.Core.Enums;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace PartyMemberManager.Dal.Entities
{
    /// <summary>
    /// 入党积极分子
    /// </summary>
    public class PotentialMember : PartyMemberBase
    {
        /// <summary>
        /// 确定发展对象时间
        /// </summary>
        [DisplayName("确定发展对象时间")]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string PotentialMemberTime { get; set; }

    }
}
