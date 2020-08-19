using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using PartyMemberManager.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PartyMemberManager.Dal.Entities
{
    public class ActiveApplicationSurvey : EntityBase
    {
        /// <summary>
        /// 年度
        /// </summary>
        [DisplayName("年度")]
        [StringLength(4, MinimumLength = 1, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string Year { get; set; }

        /// <summary>
        /// 学期
        /// </summary>
        [DisplayName("学期")]
        public Term Term { get; set; }

        /// <summary>
        /// 学期
        /// </summary>
        [DisplayName("学期")]
        public string TermDisplay { get => Term.ToString(); }
        /// <summary>
        /// 年度学期
        /// </summary>
        [DisplayName("学年/学期")]
        public string YearTerm { get; set; }

        /// <summary>
        /// 校区
        /// </summary>
        [DisplayName("校区")]
        public SchoolArea SchoolArea { get; set; }
        /// <summary>
        /// 校区
        /// </summary>
        [DisplayName("校区")]
        public string SchoolAreaDisplay { get => SchoolArea.ToString(); }
        /// <summary>
        /// 所属部门
        /// </summary>
        [DisplayName("所属部门")]
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
        /// 学生总人数
        /// </summary>
        [DisplayName("学生总人数")]
        [Range(1, 5000, ErrorMessageResourceName = "RangeErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public int Total { get; set; }

        /// <summary>
        /// 拟培训人数
        /// </summary>
        [DisplayName("拟培训人数")]
        [Range(1, 1000, ErrorMessageResourceName = "RangeErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public int TrainTotal { get; set; }

        /// <summary>
        /// 培训比例
        /// </summary>
        [DisplayName("培训比例")]
        public double Proportion { get; set; }
    }
}
