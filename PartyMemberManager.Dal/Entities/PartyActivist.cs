using PartyMemberManager.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PartyMemberManager.Dal.Entities
{
    /// <summary>
    /// 入党积极分子
    /// </summary>
    public class PartyActivist :PartyMemberBase
    {
        /// <summary>
        /// 担任职务
        /// </summary>
        [DisplayName("担任职务")]
        public string Duty { get; set; }
    }
}
