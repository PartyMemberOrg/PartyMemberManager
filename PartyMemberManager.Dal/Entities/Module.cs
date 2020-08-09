using PartyMemberManager.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace PartyMemberManager.Dal.Entities
{
    /// <summary>
    /// 功能模块
    /// </summary>
    public class Module:EntityBase
    {
        /// <summary>
        /// 功能模块
        /// </summary>
        [DisplayName("功能模块")]
        public string Name { get; set; }
        /// <summary>
        /// 分区
        /// </summary>
        [DisplayName("分区")]
        public string Area { get; set; }
        /// <summary>
        /// 控制器
        /// </summary>
        [DisplayName("控制器")]
        public string Controller { get; set; }
        /// <summary>
        /// 动作
        /// </summary>
        [DisplayName("动作")]
        public string Action { get; set; }
        /// <summary>
        /// 角色(可使用角色)
        /// </summary>
        [DisplayName("角色")]
        public Role Roles { get; set; }
        /// <summary>
        /// 上级权限（实现权限级联）
        /// </summary>
        public Guid? ParentModuleId { get; set; }
        /// <summary>
        /// 上级权限（实现权限级联）
        /// </summary>
        public Module ParentModule { get; set; }
        public List<Module> ChildModules { get; set; }
    }
}
