using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EntityExtension
{
    /// <summary>
    /// 分页参数设置
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    public class PageDataParameter<TEntity>
    {
        /// <summary>
        /// 分页参数设置
        /// </summary>
        /// <param name="page">当前页号</param>
        /// <param name="limit">每次返回的记录数，默认返回10挑记录</param>
        public PageDataParameter(int page, int limit = 10)
        {
            this.Limit = limit;
            this.Page = page;
        }

        /// <summary>
        /// 每页大小
        /// </summary>
        public int Limit { get; private set; }

        /// <summary>
        /// 当前页
        /// </summary>
        public int Page { get; private set; }
    }
}
