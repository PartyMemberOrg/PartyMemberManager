using PartyMemberManager.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PartyMemberManager.Dal.Entities
{
    public class PartyMemberBase : EntityBase
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
        [DisplayName("培训班名称")]
        [NotMapped]
        public string TrainClassDisplay { get => TrainClass == null ? "" : TrainClass.Name; }

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
        /// 姓名
        /// </summary>
        [DisplayName("姓名")]
        [StringLength(50, MinimumLength = 2, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string Name { get; set; }

        /// <summary>
        /// 学号/工号
        /// </summary>
        [DisplayName("学号/工号")]
        [StringLength(13, MinimumLength = 10, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string JobNo { get; set; }


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
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public Sex Sex { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        [DisplayName("性别")]
        [NotMapped]
        public string SexDisplay { get => Sex.ToString(); }

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
        /// 出身年月
        /// </summary>
        [DisplayName("出生年月")]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public Nullable<DateTime> BirthDate { get; set; }

        /// <summary>
        /// 民族
        /// </summary>
        [DisplayName("民族")]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public Guid NationId { get; set; }
        /// <summary>
        /// 民族
        /// </summary>
        [DisplayName("民族")]
        public Nation Nation { get; set; }
        /// <summary>
        /// 民族
        /// </summary>
        [DisplayName("民族")]
        [NotMapped]
        public string NationDisplay { get => Nation == null ? "" : Nation.Name; }

        /// <summary>
        /// 电话
        /// </summary>
        [DisplayName("电话")]
        [StringLength(50, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string Phone { get; set; }

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
        /// 所在班级
        /// </summary>
        [DisplayName("所在班级")]
        [StringLength(50, MinimumLength = 1, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string Class { get; set; }

        /// <summary>
        /// 提交入党申请时间
        /// </summary>
        [DisplayName("提交入党申请时间")]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public Nullable<DateTime> ApplicationTime { get; set; }

        /// <summary>
        /// 确定入党积极分子时间
        /// </summary>
        [DisplayName("确定入党积极分子时间")]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public Nullable<DateTime> ActiveApplicationTime { get; set; }


        /// <summary>
        /// 是否打印
        /// </summary>
        [DisplayName("是否已打印证书")]
        public bool IsPrint { get; set; }
        
        /// <summary>
        /// 打印证书时间
        /// </summary>
        [DisplayName("打印证书时间")]
        public DateTime? PrintTime { get; set; }

    }
}
