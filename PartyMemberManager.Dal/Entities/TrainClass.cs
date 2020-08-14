using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PartyMemberManager.Dal.Entities
{
    /// <summary>
    /// 培训班
    /// </summary>
    public class TrainClass : EntityBase
    {
        /// <summary>
        /// 培训班名称
        /// </summary>
        [DisplayName("培训班名称")]
        [StringLength(50, MinimumLength = 1, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string Name { get; set; }

        /// <summary>
        /// 培训班类型
        /// </summary>
        [DisplayName("培训班类型")]
        public Guid TrainClassTypeId { get; set; }

        /// <summary>
        /// 培训班类型
        /// </summary>
        [DisplayName("培训班类型")]
        public TrainClassType TrainClassType { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        [DisplayName("开始时间")]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 平时成绩比例
        /// </summary>
        [DisplayName("平时成绩占比")]
        [Range(0, 100, ErrorMessageResourceName = "RangeErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public int PSGradeProportion { get; set; }

        /// <summary>
        /// 考试成绩比例
        /// </summary>
        [DisplayName("考试成绩占比")]
        [Range(0, 100, ErrorMessageResourceName = "RangeErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public int CsGradeProportion { get; set; }
    }
}
