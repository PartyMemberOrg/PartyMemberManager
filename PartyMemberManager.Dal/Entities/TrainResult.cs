using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PartyMemberManager.Dal.Entities
{
    public class TrainResult : EntityBase
    {
        /// <summary>
        /// 年度
        /// </summary>
        [DisplayName("年度")]
        [StringLength(4, MinimumLength = 1, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string  Year{ get; set; }
        /// <summary>
        /// 培训班类型
        /// </summary>
        public TrainClassType TrainClassType { get; set; }
        /// <summary>
        /// 培训人员
        /// </summary>
        public Guid PartyActivistId { get; set; }
        /// <summary>
        /// 培训人员
        /// </summary>
        public PartyActivist PartyActivist { get; set; }

        /// <summary>
        /// 平时成绩
        /// </summary>
        [DisplayName("平时成绩")]
        [Range(0,100.00,  ErrorMessageResourceName = "RangeErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public double PSGrade { get; set; }
        /// <summary>
        /// 考试成绩
        /// </summary>
        [DisplayName("考试成绩")]
        [Range(0, 100.00, ErrorMessageResourceName = "RangeErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public double CSGrade { get; set; }
        /// <summary>
        /// 总评成绩
        /// </summary>
        public double TotalGrade { get; set; }
        /// <summary>
        /// 是否合格
        /// </summary>
        public bool IsPass { get; set; }
    }
}
