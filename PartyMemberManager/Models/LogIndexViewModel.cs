using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace PartyMemberManager.Models
{
    public class LogIndexViewModel
    {
        /// <summary>
        /// 开始日期
        /// </summary>
        [DisplayName("开始日期")]
        public DateTime? DateStart { get; set; }
        /// <summary>
        /// 结束日期
        /// </summary>
        [DisplayName("结束日期")]
        public DateTime? DateEnd { get; set; }
        /// <summary>
        /// 日志日期
        /// </summary>
        [DisplayName("日期")]
        public string DateRange
        {
            get => string.Format("{0:yyyy-MM-dd} - {1:yyyy-MM-dd}", DateStart, DateEnd);
            set
            {
                string[] dates = value.Split(" - ");
                DateStart = DateTime.Parse(dates[0]);
                DateEnd = DateTime.Parse(dates[1]);
            }
        }
        /// <summary>
        /// 日志级别
        /// </summary>
        [DisplayName("日志级别")]
        public string LogLevel { get; set; }
        /// <summary>
        /// 分类
        /// </summary>
        [DisplayName("分类")]
        public string CategoryName { get; set; }
        /// <summary>
        /// 信息
        /// </summary>
        [DisplayName("信息")]
        public string Message { get; set; }
        /// <summary>
        /// 是否是提交上来的查询数据
        /// </summary>
        public bool IsPost { get; set; } = false;
    }
}
