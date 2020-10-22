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
    /// 发展对象
    /// </summary>
    public class PotentialMember : PartyMemberBase
    {
        /// <summary>
        /// 确定发展对象时间
        /// </summary>
        [DisplayName("确定发展对象时间")]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public Nullable<DateTime> PotentialMemberTime { get; set; }

        /// <summary>
        /// 确定发展对象列表
        /// </summary>
        [NotMapped]
        public string IdList { get; set; }

        /// <summary>
        /// 入党积极分子
        /// </summary>
        [DisplayName("入党积极分子")]
        public Guid? PartyActivistId { get; set; }
        /// <summary>
        /// 入党积极分子
        /// </summary>
        [DisplayName("入党积极分子")]
        public PartyActivist PartyActivist { get; set; }

    }
}
