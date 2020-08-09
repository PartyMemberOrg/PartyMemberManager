using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EntityExtension
{
    /// <summary>
    /// 分页数据模型，如果直接给页面传递此模型，则页面的model必须指定为PageDataViewModel，很不方便
    /// </summary>
    public class PagedDataViewModel
    {
        /// <summary>
        /// 记录总个数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 当前页索引
        /// </summary>
        public int CurrentPage { get; set; }
        /// <summary>
        /// 每页记录数
        /// </summary>
        public int PageSize { get; set; }
    }
    /// <summary>
    /// 视图页面生成的默认model是IEnumerable<TEntity>，为了和此模型匹配，对PagedDataViewModel进行了封装，让其实现IEnumberable<TResult>,
    /// 然后在迭代方法GetEnumerator返回Data的迭代器，这样当用foreach循环遍历PagedDataViewModel<TResult>时，实际上遍历的是Data属性，即被我们封装在里面的数据
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public class PagedDataViewModel<TResult>: PagedDataViewModel,IEnumerable<TResult>
    {
        /// <summary>
        /// 当前页数据
        /// </summary>
        public IEnumerable<TResult> Data { get; set; }

        public IEnumerator<TResult> GetEnumerator()
        {
            return Data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Data.GetEnumerator();
        }
    }
}
