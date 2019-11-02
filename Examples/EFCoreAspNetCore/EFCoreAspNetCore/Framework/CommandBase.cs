using EFCoreAspNetCore.Data;
using EFCoreAspNetCore.Framework.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EFCoreAspNetCore.Framework
{
    public abstract class CommandBase
    {
        protected ExampleContext DbContext { get; private set; }

        protected CommandBase(ExampleContext dbContext)
        {
            DbContext = dbContext;
        }

        protected static T EnsureExists<T>(T item)
            where T : class
            => item ?? throw new ItemNotFoundException($"{typeof(T).Name.Replace("ViewModel", "")} not found");

        protected static TTarget Map<TSource, TTarget>(TSource source, Expression<Func<TSource, TTarget>> mapper)
            => mapper.Compile().Invoke(source);

        protected static TTarget FilterToFirstOrDefault<TSource, TTarget>(IQueryable<TSource> source, Expression<Func<TSource, TTarget>> map, bool ensureExists = true)
            where TSource : class
            where TTarget : class
        {
            var item = FilterAndMap(source, map, null).FirstOrDefault();
            if (ensureExists)
            {
                EnsureExists(item);
            }

            return item;
        }
        protected static List<TTarget> FilterToList<TSource, TTarget>(IQueryable<TSource> source, Expression<Func<TSource, TTarget>> map, Func<IQueryable, IQueryable> filter = null)
            => FilterAndMap(source, map, filter).ToList();

        private static IQueryable<TTarget> FilterAndMap<TSource, TTarget>(IQueryable<TSource> source, Expression<Func<TSource, TTarget>> map, Func<IQueryable, IQueryable> filter)
        {
            var query = source;
            if (filter != null)
            {
                query = filter(query) as IQueryable<TSource>;
            }

            return query.Select(map);
        }
    }
}
