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
        /// 姓名
        /// </summary>
        [DisplayName("姓名")]
        [NotMapped]
        public string NameDisplay { get => PotentialMember == null ? "" : PotentialMember.Name; }
        /// <summary>
        /// 工号/学号
        /// </summary>
        [DisplayName("工号/学号")]
        [NotMapped]
        public string JobNoDisplay { get => PotentialMember == null ? "" : PotentialMember.JobNo; }

        /// <summary>
        /// 学年/学期
        /// </summary>
        [DisplayName("学年/学期")]
        [NotMapped]
        public string YearTermDisplay { get => PotentialMember.TrainClass.YearTerm == null ? "" : PotentialMember.TrainClass.YearTerm.Name; }

        /// <summary>
        /// 培训班名称
        /// </summary>
        [DisplayName("培训班名称")]
        [NotMapped]
        public string TrainClassDisplay { get => PotentialMember.TrainClass == null ? "" : PotentialMember.TrainClass.Name; }
    }
}
