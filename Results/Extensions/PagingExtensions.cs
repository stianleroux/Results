namespace Results.Extensions;

using System.Collections;
using Results.Models;

public static class PagingExtensions
{
    // Instance extension block (for IQueryable<T> and List<T>)
    extension<T>(IQueryable<T> query)
    {
        public IQueryable<T> ApplyPaging(Paging? pagingArgs)
        {
            var args = pagingArgs ?? Paging.Default;

            return !args.UsePaging ? query : query.Skip(args.SkipAmount).Take(args.PageSize);
        }
    }

    extension<T>(List<T> list)
    {
        public IQueryable<T> ApplyPaging(Paging? pagingArgs)
            => list.AsQueryable().ApplyPaging(pagingArgs);
    }

    // Static extension block (no instance, for utility-ish method)
    extension(PagingExtensions)
    {
        public static bool IsObjectNullOrEmpty(object? instance)
        {
            if (instance is null)
            {
                return true;
            }

            foreach (var prop in instance.GetType().GetProperties())
            {
                var value = prop.GetValue(instance);
                if (value is null)
                {
                    return true;
                }

                if (value is string s && string.IsNullOrWhiteSpace(s))
                {
                    return true;
                }

                if (value is IEnumerable enumerable && !enumerable.Cast<object>().Any())
                {
                    return true;
                }

                if (prop.PropertyType.IsValueType
                    && value.Equals(Activator.CreateInstance(prop.PropertyType)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}