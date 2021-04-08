using PartyMemberManager.Dal.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PartyMemberManager.Models
{
    public class QueryScoreViewModel
    {

        /// <summary>
        /// 培训班类型
        /// </summary>
        [DisplayName("培训班类型")]
        public Guid TrainClassTypeId { get; set; }

        /// <summary>
        /// 培训班类型
        /// </summary>
        [DisplayName("培训班类型")]
        public TrainClassType TrainClassType { get; set; }

        /// <summary>
        /// 培训班类型
        /// </summary>
        [DisplayName("培训班类型")]
        public string TrainClassTypeDisplay { get => TrainClassType == null ? "" : TrainClassType.Name; }

        /// <summary>
        /// 姓名
        /// </summary>
        [DisplayName("姓名")]
        [Required(ErrorMessage ="请输入姓名")]
        public string Name { get; set; }

        /// <summary>
        /// 学号/工号
        /// </summary>
        [DisplayName("学号/工号")]
        [Required(ErrorMessage = "请输入学号/工号")]
        public string JobNo { get; set; }


        /// <summary>
        /// 身份证号
        /// </summary>
        [DisplayName("身份证号")]
        [Required(ErrorMessage = "请输入身份证号")]
        public string IdNumber { get; set; }
        /// <summary>
        /// 总评成绩
        /// </summary>
        [DisplayName("总成绩")]
        public decimal? TotalGrade { get; set; }

        /// <summary>
        /// 是否可查询
        /// </summary>
        [DisplayName("是否可查询")]
        public bool IsQuery { get; set; }

        public string Message { get; set; }
    }
}
