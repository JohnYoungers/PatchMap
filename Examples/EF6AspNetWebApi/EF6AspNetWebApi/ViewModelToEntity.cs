using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EF6AspNetWebApi
{
    public class ViewModelToEntity<TTarget>
    {
        private readonly IEnumerable<(bool applies, Expression<Func<TTarget, bool>> filter)> filters;

        public ViewModelToEntity(params (bool applies, Expression<Func<TTarget, bool>> filter)[] filters)
        {
            this.filters = filters;
        }

        public (TTarget match, string failureReason) Convert(IQueryable<TTarget> query)
        {
            var filteredQuery = query;
            foreach (var filter in filters.Where(f => f.applies))
            {
                filteredQuery = filteredQuery.Where(filter.filter);
            }

            if (filteredQuery == query)
            {
                return (default(TTarget), "did not contain look up criteria");
            }
            else
            {
                var results = query.ToArray();
                if (!results.Any())
                {
                    return (default(TTarget), "could not be found");
                }
                else if (results.Length > 1)
                {
                    return (default(TTarget), "matched multiple items");
                }
                else
                {
                    return (results[0], null);
                }
            }
        }
    }
}
