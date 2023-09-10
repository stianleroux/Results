namespace Api.Utilities.Extensions;

using Results.Models;

public static class PagingExtensions
{
	public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, Paging pagingArgs)
	{
		var myPagingArgs = pagingArgs;

		if (pagingArgs == null)
		{
			myPagingArgs = Paging.Default;
		}

		return myPagingArgs.UsePaging ? query.Skip(myPagingArgs.SkipAmount).Take(myPagingArgs.PageSize) : query;
	}

	public static IQueryable<T> ApplyPaging<T>(this List<T> query, Paging pagingArgs)
	{
		var myPagingArgs = pagingArgs;

		if (pagingArgs == null)
		{
			myPagingArgs = Paging.Default;
		}

		return query.ApplyPaging(myPagingArgs);
	}

	public static bool IsObjectNullOrEmpty(object myObject)
	{
		if (myObject == null)
		{
			return true;
		}

		foreach (var pi in myObject.GetType().GetProperties())
		{
			if (pi.PropertyType != typeof(string))
			{
				continue;
            }

            var value = (string)pi.GetValue(myObject);
			if (value == null || value.AsSpan().Trim().Length == 0)
			{
				return true;
			}
		}

		return false;
	}
}
