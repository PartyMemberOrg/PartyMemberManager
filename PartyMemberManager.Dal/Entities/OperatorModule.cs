using PartyMemberManager.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace PartyMemberManager.Dal.Entities
{
    /// <summary>
    /// 模块权限
    /// </summary>
    public class OperatorModule:EntityBase
    {
        /// <summary>
        /// 用户
        /// </summary>
        [DisplayName("用户")]
        public Guid UserId { get; set; }
        /// <summary>
        /// 功能模块
        /// </summary>
        [DisplayName("功能模块")]
        public Guid ModuleId { get; set; }
        /// <summary>
        /// 权限类型
        /// </summary>
        [DisplayName("权限类型")]
        public RightType RightType { get; set; } = RightType.Grant;
        /// <summary>
        /// 用户
        /// </summary>
        [DisplayName("用户")]
        public Operator User { get; set; }
        /// <summary>
        /// 功能模块
        /// </summary>
        [DisplayName("功能模块")]
        public Module Module { get; set; }
    }
}
