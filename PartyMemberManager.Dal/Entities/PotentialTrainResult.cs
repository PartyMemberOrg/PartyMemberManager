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
        public string YearTermDisplay { get => PotentialMember == null ? "" : PotentialMember.TrainClass == null ? "" : PotentialMember.TrainClass.YearTerm == null ? "" : PotentialMember.TrainClass.YearTerm.Name; }

        /// <summary>
        /// 培训班名称
        /// </summary>
        [DisplayName("培训班名称")]
        [NotMapped]
        public string TrainClassDisplay { get => PotentialMember == null ? "" : PotentialMember.TrainClass == null ? "" : PotentialMember.TrainClass.Name; }


        /// <summary>
        /// 批次
        /// </summary>
        [DisplayName("批次")]
        [NotMapped]
        public string BatchDisplay { get => PotentialMember == null ? "" : PotentialMember.TrainClass == null ? "" : PotentialMember.TrainClass.Batch.ToString(); }

        /// <summary>
        /// 是否已打印
        /// </summary>
        [DisplayName("是否已打印")]
        [NotMapped]
        public bool IsPrint { get => PotentialMember == null ? false : PotentialMember.IsPrint; }

        /// <summary>
        /// 是否已打印
        /// </summary>
        [DisplayName("是否已打印")]
        [NotMapped]
        public string IsPrintDisplay { get => PotentialMember == null?"": PotentialMember.IsPrint ? "是" : "否"; }

        /// <summary>
        /// 打印时间
        /// </summary>
        [DisplayName("打印时间")]
        [NotMapped]
        public DateTime? PrintTimeDisplay { get => PotentialMember == null ? null : PotentialMember.PrintTime; }
    }
}
