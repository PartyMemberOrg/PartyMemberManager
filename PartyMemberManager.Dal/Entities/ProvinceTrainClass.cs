using PartyMemberManager.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PartyMemberManager.Dal.Entities
{
    public class ProvinceTrainClass:EntityBase
    {
        /// <summary>
        /// 年份
        /// </summary>
        [DisplayName("年份")]
        [StringLength(4, MinimumLength = 1, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string Year { get; set; }

        /// <summary>
        /// 培训班名称
        /// </summary>
        [DisplayName("培训班名称")]
        [StringLength(100, MinimumLength = 1, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string Name { get; set; }

        /// <summary>
        /// 总人数
        /// </summary>
        [DisplayName("总人数")]
        public int? Total { get; set; }

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
