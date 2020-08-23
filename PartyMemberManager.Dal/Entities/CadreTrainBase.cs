using PartyMemberManager.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PartyMemberManager.Dal.Entities
{
    public class CadreTrainBase:EntityBase
    {
        /// <summary>
        /// 学年/学期
        /// </summary>
        [DisplayName("学年/学期")]

        public Guid? YearTermId { get; set; }

        /// <summary>
        /// 学年/学期
        /// </summary>
        [DisplayName("学年/学期")]

        public YearTerm YearTerm { get; set; }
        /// <summary>
        /// 学年学期
        /// </summary>
        [DisplayName("学年/学期")]
        [NotMapped]
        public string YearTermDisplay { get => YearTerm == null ? "" : YearTerm.Name; }

        /// <summary>
        /// 组织单位
        /// </summary>
        [DisplayName("组织单位")]
        [StringLength(50, MinimumLength = 1, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string Organizer { get; set; }

        /// <summary>
        /// 培训单位
        /// </summary>
        [DisplayName("培训单位")]
        [StringLength(50, MinimumLength = 1, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string TrainOrganizational { get; set; }

        /// <summary>
        /// 培训时间
        /// </summary>
        [DisplayName("培训时间")]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public DateTime? TrainTime{ get; set; }

        /// <summary>
        /// 培训时长
        /// </summary>
        [DisplayName("培训时长")]
        [StringLength(10, MinimumLength = 1, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]

        public string TrainDuration { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        [DisplayName("姓名")]
        [StringLength(20, MinimumLength = 1, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string Name { get; set; }
        /// <summary>
        /// 手机
        /// </summary>
        [DisplayName("手机")]
        [StringLength(50, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string Phone { get; set; }


        /// <summary>
        /// 身份证号
        /// </summary>
        [DisplayName("身份证号")]
        [StringLength(20, MinimumLength = 1, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string IdNumber { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        [DisplayName("性别")]
        public Sex Sex { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        [DisplayName("性别")]
        [NotMapped]
        public string SexDisplay { get => Sex.ToString(); }

        /// <summary>
        /// 所在单位
        /// </summary>
        [DisplayName("所在单位")]
        public Guid DepartmentId { get; set; }
        /// <summary>
        /// 所在单位
        /// </summary>
        [DisplayName("所在单位")]
        public Department Department { get; set; }

        /// <summary>
        /// 所属部门
        /// </summary>
        [DisplayName("所属部门")]
        public string DepartmentDisplay { get => Department == null ? "" : Department.Name; }

        /// <summary>
        /// 职务
        /// </summary>
        [DisplayName("职务")]
        [StringLength(20, MinimumLength = 1, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string Post { get; set; }
    }
}
