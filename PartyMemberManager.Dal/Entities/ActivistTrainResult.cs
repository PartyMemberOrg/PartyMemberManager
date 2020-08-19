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
        public string YearTermDisplay { get => PartyActivist.TrainClass == null ? "" : PartyActivist.TrainClass.YearTerm; }

        /// <summary>
        /// 培训班名称
        /// </summary>
        [DisplayName("培训班名称")]
        [NotMapped]
        public string TrainClassDisplay { get => PartyActivist.TrainClass == null ? "" : PartyActivist.TrainClass.Name; }
    }
}
