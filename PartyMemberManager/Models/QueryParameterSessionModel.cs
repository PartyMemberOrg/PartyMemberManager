using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartyMemberManager.Models
{
    /// <summary>
    /// 查询参数Session模型，用于缓存每个控制器的查询参数
    /// </summary>
    public class QueryParameterSessionModel<T>
    {
        /// <summary>
        /// 查询参数
        /// </summary>
        public T QueryParameter { get; set; }
        /// <summary>
        /// 当前页码
        /// </summary>
        public int Page { get; set; }
        /// <summary>
        /// 每页记录数
        /// </summary>
        public int Limit { get; set; }
    }
}
