using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PartyMemberManager.Dal.Entities
{
    public class TrainResult : PartyMemberBase
    {
        /// <summary>
        /// 培训班
        /// </summary>
        public Guid TrainClassId { get; set; }
        /// <summary>
        /// 培训班
        /// </summary>
        public TrainClass TrainClass { get; set; }
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
