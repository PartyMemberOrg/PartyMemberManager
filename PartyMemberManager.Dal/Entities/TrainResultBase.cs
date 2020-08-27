using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PartyMemberManager.Dal.Entities
{
    public class TrainResultBase : EntityBase
    {
        /// <summary>
        /// 平时成绩
        /// </summary>
        [DisplayName("平时成绩")]
        [Range(0,100.00,  ErrorMessageResourceName = "RangeErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public decimal? PsGrade { get; set; }
        /// <summary>
        /// 考试成绩
        /// </summary>
        [DisplayName("考试成绩")]
        [Range(0, 100.00, ErrorMessageResourceName = "RangeErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public decimal? CsGrade { get; set; }

        /// <summary>
        /// 补考成绩
        /// </summary>
        [DisplayName("补考成绩")]
        [Range(0, 100.00, ErrorMessageResourceName = "RangeErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public decimal? BcGrade { get; set; }
        /// <summary>
        /// 总评成绩
        /// </summary>
        [DisplayName("总成绩")]
        public decimal? TotalGrade { get; set; }
        /// <summary>
        /// 是否合格
        /// </summary>
        [DisplayName("是否合格")]
        public bool IsPass { get; set; }

        /// 证书序号
        /// </summary>
        [DisplayName("证书序号")]
        public int? CertificateOrder { get; set; }

        /// 证书编号
        /// </summary>
        [DisplayName("证书编号")]
        public string CertificateNumber { get; set; }
    }
}
