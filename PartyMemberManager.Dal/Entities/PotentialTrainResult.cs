using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PartyMemberManager.Dal.Entities
{
    public class PotentialTrainResult : TrainResultBase
    {
        /// <summary>
        /// 发展对象
        /// </summary>
        [DisplayName("发展对象")]
        public Guid PotentialMemberId { get; set; }
        /// <summary>
        /// 发展对象
        /// </summary>
        [DisplayName("发展对象")]
        public PotentialMember PotentialMember { get; set; }

        /// <summary>
        /// 发展对象
        /// </summary>
        [DisplayName("发展对象")]
        [NotMapped]
        public string PotentialMemberNameDisplay { get => PotentialMember == null ? "" : PotentialMember.Name; }
        /// <summary>
        /// 积极分子
        /// </summary>
        [DisplayName("发展对象")]
        [NotMapped]
        public string PotentialMemberJobNoDisplay { get => PotentialMember == null ? "" : PotentialMember.JobNo; }
    }
}
