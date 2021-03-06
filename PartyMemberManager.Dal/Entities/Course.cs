﻿using PartyMemberManager.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PartyMemberManager.Dal.Entities
{
    public class Course : EntityBase
    {
        /// <summary>
        /// 年度学期
        /// </summary>
        [DisplayName("学年/学期")]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public Guid? YearTermId { get; set; }
        /// <summary>
        /// 年度学期
        /// </summary>
        [DisplayName("学年/学期")]
        public YearTerm YearTerm { get; set; }

        /// <summary>
        /// 学期
        /// </summary>
        [DisplayName("学期")]
        [NotMapped]
        public string TermDisplay { get => YearTerm == null ? "" : YearTerm.Name; }

        /// <summary>
        /// 培训班名称
        /// </summary>
        [DisplayName("培训班名称")]
        [StringLength(50, MinimumLength = 1, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string TrainClassName { get; set; }
        /// <summary>
        /// 授课类型
        /// </summary>
        [DisplayName("授课类型")]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public CourseType CourseType { get; set; }
        /// <summary>
        /// 授课类型
        /// </summary>
        [DisplayName("授课类型")]
        [NotMapped]
        public string CourseTypeDisplay { get => CourseType.ToString(); }

        /// <summary>
        /// 姓名
        /// </summary>
        [DisplayName("姓名")]
        [StringLength(20, MinimumLength = 1, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string Name { get; set; }
        /// <summary>
        /// 工作单位
        /// </summary>
        [DisplayName("工作单位")]
        [StringLength(100, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string Organization{ get; set; }

        /// <summary>
        /// 职称
        /// </summary>
        [DisplayName("职称")]
        [StringLength(50, MinimumLength = 1, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string Rank { get; set; }

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
        /// 授课时间
        /// </summary>
        [DisplayName("授课时间")]
        [StringLength(100, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string StartTime { get; set; }
        /// <summary>
        /// 授课地点
        /// </summary>
        [DisplayName("授课地点")]
        [StringLength(100, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]

        public string Place { get; set; }

        /// <summary>
        /// 课程名称
        /// </summary>
        [DisplayName("课程名称")]
        [StringLength(200, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]

        public string CourseName { get; set; }

        /// <summary>
        /// 授课学时
        /// </summary>
        [DisplayName("授课学时")]
        [Range(0, 100, ErrorMessageResourceName = "RangeErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]

        public int CourseHour { get; set; }

        /// <summary>
        /// 附件1
        /// </summary>
        [DisplayName("课件")]
        public string Attachment_1 { get; set; }

        /// <summary>
        /// 附件1类型
        /// </summary>
        [DisplayName("附件1类型")]
        public string Attachment_1_Type { get; set; }

        /// <summary>
        /// 附件2
        /// </summary>
        [DisplayName("讲义")]
        public string Attachment_2 { get; set; }

        /// <summary>
        /// 附件2类型
        /// </summary>
        [DisplayName("附件2类型")]
        public string Attachment_2_Type { get; set; }

        /// <summary>
        /// 所属部门
        /// </summary>
        [DisplayName("所属部门")]
        public Guid? DepartmentId { get; set; }
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
        /// 批次
        /// </summary>
        [DisplayName("批次")]
        public BatchType? Batch { get; set; }
        /// <summary>
        /// 批次
        /// </summary>
        [DisplayName("批次")]
        [NotMapped]
        public string BatchDisplay { get => Batch==0?"":Batch.ToString(); }
    }
}
