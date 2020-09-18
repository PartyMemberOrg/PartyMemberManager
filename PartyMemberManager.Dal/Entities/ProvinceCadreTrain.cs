using PartyMemberManager.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PartyMemberManager.Dal.Entities
{
    public class ProvinceCadreTrain : EntityBase
    {
        /// <summary>
        /// 培训班
        /// </summary>
        [DisplayName("培训班")]

        public Guid ProvinceTrainClassId { get; set; }

        /// <summary>
        /// 培训班
        /// </summary>
        [DisplayName("培训班")]

        public ProvinceTrainClass ProvinceTrainClass { get; set; }
        /// <summary>
        /// 培训班
        /// </summary>
        [DisplayName("培训班")]
        [NotMapped]
        public string TrainClassDisplay { get => ProvinceTrainClass == null ? "" : ProvinceTrainClass.Name; }

        /// <summary>
        /// 培训班年份
        /// </summary>
        [DisplayName("培训班年份")]
        [NotMapped]
        public string YearDisplay { get => ProvinceTrainClass == null ? "" : ProvinceTrainClass.Year; }

        /// <summary>
        /// 姓名
        /// </summary>
        [DisplayName("姓名")]
        [StringLength(50, MinimumLength = 2, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string Name { get; set; }

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
        /// 民族
        /// </summary>
        [DisplayName("民族")]
        public Guid NationId { get; set; }
        /// 民族
        /// </summary>
        [DisplayName("民族")]
        public Nation Nation { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        [DisplayName("民族")]
        [NotMapped]
        public string NationDisplay { get => Nation==null?"":Nation.Name; }

        /// <summary>
        /// 所在单位
        /// </summary>
        [DisplayName("所在单位")]
        [StringLength(20, MinimumLength = 1, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string  Department { get; set; }


        /// <summary>
        /// 职务
        /// </summary>
        [DisplayName("职务")]
        [StringLength(20, MinimumLength = 1, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string Post { get; set; }

        /// <summary>
        /// 手机
        /// </summary>
        [DisplayName("手机")]
        [StringLength(50, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string Phone { get; set; }
        /// <summary>
        /// 学员需求1
        /// </summary>
        [DisplayName("学员需求1")]
        [StringLength(255, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string Demand1 { get; set; }
        /// <summary>
        /// 学员需求2
        /// </summary>
        [DisplayName("学员需求2")]
        [StringLength(255, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string Demand2 { get; set; }
        /// <summary>
        /// 其他
        /// </summary>
        [DisplayName("其他")]
        [StringLength(255, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string OtherDemand { get; set; }
    }
}
