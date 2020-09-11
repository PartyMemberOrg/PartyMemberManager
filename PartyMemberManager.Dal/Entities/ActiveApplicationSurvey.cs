using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using PartyMemberManager.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PartyMemberManager.Dal.Entities
{
    public class ActiveApplicationSurvey : EntityBase
    {
        /// <summary>
        /// 学年学期
        /// </summary>
        [DisplayName("学年/学期")]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public Guid YearTermId { get; set; }
        /// <summary>
        /// 年度学期
        /// </summary>
        [DisplayName("学年/学期")]
        public YearTerm YearTerm { get; set; }
        /// <summary>
        /// 学期
        /// </summary>
        [DisplayName("学年/学期")]
        [NotMapped]
        public string YearTermDisplay { get => YearTerm == null ? "" : YearTerm.Name; }
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
        /// 校区
        /// </summary>
        [DisplayName("校区")]
        public string SchoolAreaDisplay { get =>Department==null?"":Department.SchoolArea.ToString(); }

        /// <summary>
        /// 类型学生或者教师
        /// </summary>
        [DisplayName("类型")]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public PartyMemberType PartyMemberType { get; set; }
        /// <summary>
        /// 类型学生或者教师
        /// </summary>
        [DisplayName("类型")]
        [NotMapped]
        public string PartyMemberTypeDisplay { get => PartyMemberType.ToString(); }

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
