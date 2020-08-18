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
        /// 积极分子
        /// </summary>
        [DisplayName("积极分子")]
        [NotMapped]
        public string NameDisplay { get => PartyActivist == null ? "" : PartyActivist.Name; }
        /// <summary>
        /// 积极分子
        /// </summary>
        [DisplayName("积极分子")]
        [NotMapped]
        public string JobNoDisplay { get => PartyActivist == null ? "" : PartyActivist.JobNo; }

        /// <summary>
        /// 积极分子
        /// </summary>
        [DisplayName("积极分子")]
        [NotMapped]
        public string YearDisplay { get => PartyActivist.TrainClass == null ? "" : PartyActivist.TrainClass.Year; }
        /// <summary>
        /// 积极分子
        /// </summary>
        [DisplayName("积极分子")]
        [NotMapped]
        public string TermDisplay { get => PartyActivist.TrainClass == null ? "" : PartyActivist.TrainClass.Term.ToString(); }

        /// <summary>
        /// 积极分子
        /// </summary>
        [DisplayName("积极分子")]
        [NotMapped]
        public string TrainClassDisplay { get => PartyActivist.TrainClass == null ? "" : PartyActivist.TrainClass.Name; }
    }
}
