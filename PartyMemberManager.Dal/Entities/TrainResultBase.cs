using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PartyMemberManager.Dal.Entities
{
    public class TrainResultBase : EntityBase
    {
        /// <summary>
        /// 培训班
        /// </summary>
        [DisplayName("培训班")]
        public Guid? TrainClassId { get; set; }
        /// <summary>
        /// 培训班
        /// </summary>
        [DisplayName("培训班")]
        public TrainClass TrainClass { get; set; }

        /// <summary>
        /// 培训班
        /// </summary>
        [DisplayName("培训班")]
        [NotMapped]
        public string TrainClassDisplay { get => TrainClass == null ? "" : TrainClass.Name; }

        /// <summary>
        /// 平时成绩
        /// </summary>
        [DisplayName("平时成绩")]
        [Range(0,100.00,  ErrorMessageResourceName = "RangeErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public double PsGrade { get; set; }
        /// <summary>
        /// 考试成绩
        /// </summary>
        [DisplayName("考试成绩")]
        [Range(0, 100.00, ErrorMessageResourceName = "RangeErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public double CsGrade { get; set; }
        /// <summary>
        /// 总评成绩
        /// </summary>
        [DisplayName("总成绩")]
        public double TotalGrade { get; set; }
        /// <summary>
        /// 是否合格
        /// </summary>
        [DisplayName("是否合格")]
        public bool IsPass { get; set; }
        /// <summary>
        /// 是否合格
        /// </summary>
        [DisplayName("是否已打印证书")]
        public bool IsPrint { get; set; }
        /// <summary>
        /// 打印证书时间
        /// </summary>
        [DisplayName("打印证书时间")]
        public string PrintTime { get; set; }


    }
}
