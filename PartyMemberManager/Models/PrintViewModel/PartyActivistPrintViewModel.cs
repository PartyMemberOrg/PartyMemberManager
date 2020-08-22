using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace PartyMemberManager.Models.PrintViewModel
{
    /// <summary>
    /// 入党积极分子结业证打印视图模型
    /// </summary>
    public class PartyActivistPrintViewModel
    {
        /// <summary>
        /// 证书编号
        /// </summary>
        [DisplayName("证书编号")]
        [JsonProperty("no")]
        public string No { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        [DisplayName("姓名")]
        [JsonProperty("name")]
        public string Name { get; set; }
        /// <summary>
        /// 开始学年
        /// </summary>
        [DisplayName("开始学年")]
        [JsonProperty("startYear")]
        public string StartYear { get; set; }
        /// <summary>
        /// 结束学年
        /// </summary>
        [DisplayName("结束学年")]
        [JsonProperty("endYear")]
        public string EndYear { get; set; }
        /// <summary>
        /// 学期
        /// </summary>
        [DisplayName("学期")]
        [JsonProperty("term")]
        public string Term { get; set; }
        /// <summary>
        /// 打印年
        /// </summary>
        [DisplayName("打印年")]
        [JsonProperty("year")]
        public string Year { get; set; }
        /// <summary>
        /// 打印月
        /// </summary>
        [DisplayName("打印月")]
        [JsonProperty("month")]
        public string Month { get; set; }
        /// <summary>
        /// 打印日
        /// </summary>
        [DisplayName("打印日")]
        [JsonProperty("day")]
        public string Day { get; set; }
    }
}
