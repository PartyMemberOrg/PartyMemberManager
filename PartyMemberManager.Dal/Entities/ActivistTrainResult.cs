using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PartyMemberManager.Dal.Entities
{
    public class ActivistTrainResult : TrainResultBase
    {

        /// <summary>
        /// 积极分子
        /// </summary>
        [DisplayName("积极分子")]
        public Guid PartyActivistId { get; set; }
        /// <summary>
        /// 积极分子
        /// </summary>
        [DisplayName("积极分子")]
        public PartyActivist PartyActivist { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        [DisplayName("姓名")]
        [NotMapped]
        public string NameDisplay { get => PartyActivist == null ? "" : PartyActivist.Name; }
        /// <summary>
        /// 工号/学号
        /// </summary>
        [DisplayName("工号/学号")]
        [NotMapped]
        public string JobNoDisplay { get => PartyActivist == null ? "" : PartyActivist.JobNo; }

        /// <summary>
        /// 学年/学期
        /// </summary>
        [DisplayName("学年/学期")]
        [NotMapped]
        public string YearTermDisplay { get => PartyActivist== null ? "" : PartyActivist.TrainClass==null?"": PartyActivist.TrainClass.YearTerm == null ?"": PartyActivist.TrainClass.YearTerm.Name; }

        /// <summary>
        /// 培训班名称
        /// </summary>
        [DisplayName("培训班名称")]
        [NotMapped]
        public string TrainClassDisplay { get => PartyActivist == null ? "" : PartyActivist.TrainClass==null?"":PartyActivist.TrainClass.Name; }

        /// <summary>
        /// 是否已打印
        /// </summary>
        [DisplayName("是否已打印")]
        [NotMapped]
        public bool IsPrint { get => PartyActivist == null ?false: PartyActivist.IsPrint; }

        /// <summary>
        /// 是否已打印
        /// </summary>
        [DisplayName("是否已打印")]
        [NotMapped]
        public string IsPrintDisplay { get => PartyActivist == null ? "" : PartyActivist.IsPrint ? "是" : "否"; }

        /// <summary>
        /// 打印时间
        /// </summary>
        [DisplayName("打印时间")]
        [NotMapped]
        public DateTime? PrintTimeDisplay { get => PartyActivist == null ?null:PartyActivist.PrintTime; }
    }
}
