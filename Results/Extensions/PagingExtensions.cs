namespace Results.Extensions;

using System.Collections;
using Results.Models;

public static class PagingExtensions
{
    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, Paging? pagingArgs)
    {
        var args = pagingArgs ?? Paging.Default;
        return args.UsePaging ? query.Skip(args.SkipAmount).Take(args.PageSize) : query;
    }

    public static IQueryable<T> ApplyPaging<T>(this List<T> query, Paging? pagingArgs)
        => query.AsQueryable().ApplyPaging(pagingArgs);

    public static bool IsObjectNullOrEmpty(object? myObject)
    {
        if (myObject is null)
        {
            return true;
        }

        foreach (var property in myObject.GetType().GetProperties())
        {
            var value = property.GetValue(myObject);
            if (value is null)
            {
                continue;
            }

            // Check for empty strings
            if (value is string str && string.IsNullOrWhiteSpace(str))
            {
                return true;
            }

            // Check for empty collections
            if (value is IEnumerable enumerable && !enumerable.Cast<object>().Any())
            {
                return true;
            }

            // Check for default value types
            if (property.PropertyType.IsValueType && value.Equals(Activator.CreateInstance(property.PropertyType)))
            {
                return true;
            }
        }

        return false;
    }
}
