using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PartyMemberManager.Core.Enums;

namespace PartyMemberManager.Dal.Entities
{
    /// <summary>
    /// 操作员信息
    /// </summary>
    public class Operator : EntityBase
    {
        /// <summary>
        /// 工号
        /// </summary>
        [DisplayName("工号")]
        [StringLength(10, MinimumLength = 1, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "AccountNoErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string LoginName { get; set; }
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
        public string Phone { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [DisplayName("密码")]
        [StringLength(100, MinimumLength = 6, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// 用户角色
        /// </summary>
        [DisplayName("用户角色")]
        public Role Roles { get; set; }
        /// <summary>
        /// 用户角色
        /// </summary>
        [DisplayName("用户角色")]
        [NotMapped]
        public string RolesDisplay { get => Roles.ToString(); }
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
        /// 是否启用
        /// </summary>
        [DisplayName("是否启用")]
        public bool Enabled { get; set; }
        /// <summary>
        /// 是否启用
        /// </summary>
        [DisplayName("是否启用")]
        [NotMapped]
        public string EnabledDisplay { get => Enabled ? "是" : "否"; }
    }
}
