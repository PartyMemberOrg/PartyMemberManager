using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace EntityExtension
{
    public static class EntityExtension
    {
        /// <summary>
        /// 获取分页数据
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="dataSource">数据源</param>
        /// <param name="page">当前页号</param>
        /// <param name="limit">每页行数</param>
        /// <returns></returns>
        public static async Task<PagedDataViewModel<TEntity>> GetPagedDataAsync<TEntity>(
            this IOrderedQueryable<TEntity> dataSource, int page, int limit = 10)
        {
            return await dataSource.GetPagedDataAsync( new PageDataParameter<TEntity>(page, limit));
        }
        /// <summary>
        /// 获取分页数据
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="dataSource">数据源（扩展在该数据源上，且数据源必须是经过排序的数据源）</param>
        /// <param name="pagedDataParameter">分页参数</param>
        /// <returns></returns>
        public static async Task<PagedDataViewModel<TEntity>> GetPagedDataAsync<TEntity>(
            this IOrderedQueryable<TEntity> dataSource,
            PageDataParameter<TEntity> pagedDataParameter)
        {
            //对数据源的数据按照分页参数中的筛选条件进行二次筛选并排序（其实数据源已经排序了，所以排序没有必要）
            //返回总的记录个数
            var totalCount = dataSource.Count();
            //当前要显示的页号
            int currentPage = pagedDataParameter.Page;
            //计算总页数
            var pageCount = (int)(Math.Ceiling(totalCount / (double)pagedDataParameter.Limit));
            //如果当前页号超过总页数，则直接返回总页数
            if (currentPage > pageCount)
                currentPage = pageCount;
            //分页操作，跳过当前页前面的所有记录，然后去分页参数中Limit所指定数量的记录，然后异步转换为List，经过转换后数据加载至内存中
            var data = await dataSource.Skip(Math.Max(currentPage - 1, 0) * pagedDataParameter.Limit).Take(pagedDataParameter.Limit).ToListAsync();
            //创建分页视图模型，为页面上的分页Html.Pager提供数据源
            var pageModel = new PagedDataViewModel<TEntity>
            {
                //数据
                Data = data,
                //总记录数
                TotalCount = totalCount,
                //当前页号
                CurrentPage = currentPage,
                //分页的大小
                PageSize = pagedDataParameter.Limit
            };
            return pageModel;
        }

        /// <summary>
        /// 获取分页数据, 可自定义选择列
        /// </summary>
        /// <typeparam name="TResult">查询结果类型</typeparam>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="dataSource">数据源</param>
        /// <param name="pagedDataParameter">分页参数</param>
        /// <returns></returns>
        public static Task<PagedDataViewModel<TResult>> SelectPagedDataAsync<TResult, TEntity>(
                        this IQueryable<TEntity> dataSource,
                        SelectPagedDataParameter<TResult, TEntity> pagedDataParameter)
        {
            return Task<PagedDataViewModel<TResult>>.Factory.StartNew(obj =>
            {
                var temp = dataSource.Select(pagedDataParameter.FieldsSelector);
                var totalCount = temp.Count();
                int currentPage = pagedDataParameter.Page;
                var pageCount = (int)(Math.Ceiling(totalCount / (double)pagedDataParameter.Limit));
                if (currentPage > pageCount)
                    currentPage = pageCount;
                var data = temp.Skip(Math.Max(currentPage - 1, 0) * pagedDataParameter.Limit).Take(pagedDataParameter.Limit);
                return new PagedDataViewModel<TResult> { Data = data, TotalCount = totalCount, CurrentPage = currentPage, PageSize = pagedDataParameter.Limit };
            }, pagedDataParameter);
        }

        /// <summary>
        /// 删除给定实体集合
        /// </summary>
        /// <returns></returns>
        public static void Delete<TEntity>(this DbSet<TEntity> dataSource, Func<TEntity, bool> predicate)
            where TEntity:class
        {
            dataSource.RemoveRange(dataSource.Where(predicate));
        }

    }
}
