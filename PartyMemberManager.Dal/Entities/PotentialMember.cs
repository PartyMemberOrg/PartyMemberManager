using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using PartyMemberManager.Core.Enums;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace PartyMemberManager.Dal.Entities
{
    /// <summary>
    /// 入党积极分子
    /// </summary>
    public class PotentialMember : EntityBase
    {
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
        public string StudentNo { get; set; }


        /// <summary>
        /// 身份证号
        /// </summary>
        [DisplayName("身份证号")]
        [StringLength(20, MinimumLength = 1, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string IDNumber { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        [DisplayName("性别")]
        public Sex Sex { get; set; }
        /// <summary>
        /// 用户角色
        /// </summary>
        [DisplayName("性别")]
        [NotMapped]
        public string SexDisplay { get => Sex.ToString(); }

        /// <summary>
        /// 电话
        /// </summary>
        [DisplayName("电话")]
        [StringLength(50, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string Phone { get; set; }

        /// <summary>
        /// 所在部门
        /// </summary>
        [DisplayName("所在部门")]
        public Department Department { get; set; }

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

        /// <summary>
        /// 确定发展对象时间
        /// </summary>
        [DisplayName("确定发展对象时间")]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public DateTime PotentialMemberTime { get; set; }

    }
}
