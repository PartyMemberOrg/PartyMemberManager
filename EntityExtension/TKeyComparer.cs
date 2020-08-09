using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EntityExtension
{
    /// <summary>
    /// 比较两个对象大小
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    class TKeyComparer<TKey> : IComparer<TKey>
    {
        private Expression<Func<TKey, TKey, int>> comparer;
        public TKeyComparer(Expression<Func<TKey, TKey, int>> comparer)
        {
            this.comparer = comparer;
        }

        public int Compare(TKey x, TKey y)
        {
            return comparer.Compile()(x, y);
        }
    }
}
