using PartyMemberManager.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PartyMemberManager.Dal.Entities
{
    /// <summary>
    /// 学年学期
    /// </summary>
    public class YearTerm : EntityBase
    {
        /// <summary>
        /// 开始年份
        /// </summary>
        [Range(1900, 2999)]
        [DisplayName("开始年份")]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public int StartYear { get; set; }
        /// <summary>
        /// 结束年份
        /// </summary>
        [DisplayName("结束年份")]
        [NotMapped]
        public int EndYear
        {
            get => StartYear + 1;
        }
        /// <summary>
        /// 学期
        /// </summary>
        [DisplayName("学期")]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public Term Term { get; set; }
        /// <summary>
        /// 学期名称
        /// </summary>
        [DisplayName("学期名称")]
        [NotMapped]
        public string Name { get => $"{StartYear}—{EndYear}学年{Term.ToString()}"; }

        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// 是否启用
        /// </summary>
        [DisplayName("是否启用")]
        public bool Enabled { get; set; }
        /// <summary>
        /// 是否启用
        /// </summary>
        [DisplayName("是否启用")]
        [NotMapped]
        public string EnabledDisplay { get => Enabled ? "是" : "否"; }
    }
}
