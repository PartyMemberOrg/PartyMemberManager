using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EntityExtension
{
    public class SelectPagedDataParameter<TResult, TEntity> : PageDataParameter<TEntity>
    {
        public SelectPagedDataParameter(int limit, int page, Expression<Func<TEntity, TResult>> fieldsSelector)
            : base(limit, page)
        {
            this.FieldsSelector = fieldsSelector;
        }

        /// <summary>
        /// 自定义选择列
        /// </summary>
        public Expression<Func<TEntity, TResult>> FieldsSelector { get; private set; }
    }
}
