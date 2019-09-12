using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace Lexim.Utils.Paging
{
    public static class PageExtensions
    {
        public static IQueryable<T> Page<T>(this IOrderedQueryable<T> source, Query r)
        {
            return source
                .Skip(r.StartIndex)
                .Take(r.PageSize); // TODO: WTF is this
        }

        public static Page<T> ToPage<T>(this IQueryable<T> source, Query r)
        {
            if (r.SortColumn.IsNullOrEmpty())
                throw new ArgumentNullException("r.SortColumn");

            var parameter = Expression.Parameter(typeof(T));
            var memberExpression = Expression.Property(parameter, r.SortColumn);
            var lambdaExpression = Expression.Lambda(memberExpression, parameter);
            LambdaExpression untypedExpression = lambdaExpression;

            IOrderedQueryable<T> sorted =
                r.SortDirection == ListSortDirection.Ascending
                    ? Queryable.OrderBy(source, (dynamic) untypedExpression)
                    : Queryable.OrderByDescending(source, (dynamic) untypedExpression);

            return sorted.ToPage(r);
        }

        public static Page<T> ToPage<T>(this IOrderedQueryable<T> source, Query r)
        {
            return new Page<T>
            {
                PageSize = r.PageSize,
                Count = source.Count(),
                List = source.Page(r).ToList(),
                PageIndex = r.Page
            };
        }

        public static Page<TTarget> MapWith<TSource, TTarget>(this Page<TSource> source, Func<TSource, TTarget> map)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new Page<TTarget>
            {
                Count = source.Count,
                PageIndex = source.PageIndex,
                PageSize = source.PageSize,
                List = source.List.Select(map).ToList()
            };
        }
    }
}
