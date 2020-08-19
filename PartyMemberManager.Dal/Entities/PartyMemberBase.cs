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
        /// 培训班
        /// </summary>
        [DisplayName("学年/学期")]
        [NotMapped]
        public string YearTermDisplay { get => TrainClass == null|| TrainClass.YearTerm == null ? "" : TrainClass.YearTerm.ToString(); }
        /// <summary>
        /// 姓名
        /// </summary>
        [DisplayName("姓名")]
        [StringLength(50, MinimumLength = 1, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string Name { get; set; }

        /// <summary>
        /// 学号/工号
        /// </summary>
        [DisplayName("学号/工号")]
        [StringLength(20, MinimumLength = 1, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
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
        public DateTime BirthDate { get; set; }

        /// <summary>
        /// 民族
        /// </summary>
        [DisplayName("民族")]
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
        public DateTime ApplicationTime { get; set; }

        /// <summary>
        /// 确定入党积极分子时间
        /// </summary>
        [DisplayName("确定入党积极分子时间")]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public DateTime ActiveApplicationTime { get; set; }

    }
}
