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
    /// 培训班
    /// </summary>
    public class TrainClass : EntityBase
    {
        /// <summary>
        /// 学期
        /// </summary>
        [DisplayName("学期")]
        [NotMapped]
        public string TermDisplay { get => YearTerm == null ? "" : YearTerm.Name; }

        /// <summary>
        /// 年度学期
        /// </summary>
        [DisplayName("学年/学期")]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public Guid YearTermId { get; set; }
        /// <summary>
        /// 批次
        /// </summary>
        [DisplayName("批次")]
        public BatchType Batch { get; set; }

        /// <summary>
        /// 批次
        /// </summary>
        [DisplayName("批次")]
        [NotMapped]
        public string BatchDisplay { get => Batch.ToString(); }
        /// <summary>
        /// 年度学期
        /// </summary>
        [DisplayName("学年/学期")]
        public YearTerm YearTerm { get; set; }
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
        /// 培训班类型
        /// </summary>
        [DisplayName("培训班类型")]
        [NotMapped]
        public string TrainClassTypeDisplay { get => TrainClassType == null ? "" : TrainClassType.Name; }

        /// <summary>
        /// 所属部门
        /// </summary>
        [DisplayName("所属部门")]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public Guid DepartmentId { get; set; }
        /// <summary>
        /// 所属部门
        /// </summary>
        [DisplayName("所属部门")]
        public Department Department { get; set; }
        /// <summary>
        /// 所属部门
        /// </summary>
        [DisplayName("所属部门")]
        public string DepartmentDisplay { get => Department == null ? "" : Department.Name; }
        /// <summary>
        /// 开始时间
        /// </summary>
        [DisplayName("开始时间")]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public Nullable<DateTime> StartTime { get; set; }

        /// <summary>
        /// 平时成绩比例
        /// </summary>
        [DisplayName("平时成绩占比")]
        [Range(0, 100, ErrorMessageResourceName = "RangeErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public int PsGradeProportion { get; set; }

        /// <summary>
        /// 实践成绩比例
        /// </summary>
        [DisplayName("实践成绩比例")]
        [Range(0, 100, ErrorMessageResourceName = "RangeErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public int SjGradeProportion { get; set; }

        /// <summary>
        /// 考试成绩比例
        /// </summary>
        [DisplayName("考试成绩占比")]
        [Range(0, 100, ErrorMessageResourceName = "RangeErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public int CsGradeProportion { get; set; }
    }
}
