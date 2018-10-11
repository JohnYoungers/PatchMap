using EF6AspNetWebApi.Data;
using EF6AspNetWebApi.Exceptions;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EF6AspNetWebApi
{
    public abstract class BaseCommand
    {
        protected ExampleContext DbContext { get; private set; }

        public BaseCommand(ExampleContext dbContext)
        {
            DbContext = dbContext;
        }

        protected T EnsureExists<T>(T item)
            where T : class
            => item ?? throw new ItemNotFoundException($"{typeof(T).Name.Replace("ViewModel", "")} not found");

        protected List<TTarget> FilterToList<TSource, TTarget>(IQueryable<TSource> source, Func<IQueryable, IQueryable> filter, Expression<Func<TSource, TTarget>> map)
            where TSource : class where TTarget : class
            => FilterAndMap(source, filter, map).ToList();

        protected TTarget FilterToFirstOrDefault<TSource, TTarget>(IQueryable<TSource> source, Expression<Func<TSource, TTarget>> map, bool ensureExists = true)
            where TSource : class
            where TTarget : class
        {
            var item = FilterAndMap(source, null, map).FirstOrDefault();
            if (ensureExists)
            {
                EnsureExists(item);
            }

            return item;
        }
        private IQueryable<VM> FilterAndMap<T, VM>(IQueryable<T> source, Func<IQueryable, IQueryable> filter, Expression<Func<T, VM>> map)
        {
            var query = source;
            if (filter != null)
            {
                query = filter(query) as IQueryable<T>;
            }

            return query.AsExpandable().Select(map);
        }
    }
}
