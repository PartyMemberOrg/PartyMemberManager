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
        /// 积极分子
        /// </summary>
        [DisplayName("积极分子")]
        public Guid PotentialMemberId { get; set; }
        /// <summary>
        /// 积极分子
        /// </summary>
        [DisplayName("积极分子")]
        public PotentialMember PotentialMember { get; set; }

        /// <summary>
        /// 积极分子
        /// </summary>
        [DisplayName("积极分子")]
        [NotMapped]
        public string PotentialMemberNameDisplay { get => PotentialMember == null ? "" : PotentialMember.Name; }
        /// <summary>
        /// 积极分子
        /// </summary>
        [DisplayName("积极分子")]
        [NotMapped]
        public string PotentialMemberJobNoDisplay { get => PotentialMember == null ? "" : PotentialMember.JobNo; }
    }
}
