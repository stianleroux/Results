#if NETSTANDARD2_0
namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// This type is required for init-only property support on .NET Standard 2.0.
    /// It is provided by the runtime in .NET 5.0+.
    /// </summary>
    internal sealed class IsExternalInit { }
}
#endif

namespace Results.Models
{
    using System;
    using System.Linq.Expressions;
    using Results.Enums;

    /// <summary>
    /// Represents an ordering instruction for a specific entity type.
    /// </summary>
    /// <typeparam name="T">The type of the entity to order.</typeparam>
    public sealed class OrderByExpression<T>(Expression<Func<T, object>> property, SortDirection direction) : IEquatable<OrderByExpression<T>>
    {
        public Expression<Func<T, object>> Property { get; init; } = property ?? throw new ArgumentNullException(nameof(property));
        public SortDirection Direction { get; init; } = direction;

        public bool Equals(OrderByExpression<T>? other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other is null)
            {
                return false;
            }
            // Use Expression.ToString() as a practical equality/hash fallback for expressions.
            return this.Direction == other.Direction
                && string.Equals(this.Property.ToString(), other.Property.ToString(), StringComparison.Ordinal);
        }

        public override bool Equals(object? obj)
            => this.Equals(obj as OrderByExpression<T>);

        public override int GetHashCode()
        {
            unchecked
            {
                var propHash = this.Property?.ToString()?.GetHashCode() ?? 0;
                return (propHash * 397) ^ this.Direction.GetHashCode();
            }
        }
    }

    /// <summary>
    /// Represents a search model with optional ordering and paging parameters.
    /// </summary>
    /// <typeparam name="T">The entity type being searched.</typeparam>
    public class SearchModel<T>
    {
        /// <summary>
        /// Gets or sets the ordering for the search query.
        /// </summary>
        public OrderByExpression<T>? Order { get; set; }

        /// <summary>
        /// Gets or sets the paging parameters for the search query.
        /// </summary>
        public Paging PagingArgs { get; set; } = Paging.Default;

        /// <summary>
        /// Example usage of <see cref="SearchModel{T}"/> with EF Core.
        /// </summary>
        /// <example>
        /// <code>
        /// var search = new SearchModel&lt;User&gt;
        /// {
        ///     Order = new OrderByExpression&lt;User&gt;(u =&gt; u.LastName, SortDirection.Ascending),
        ///     PagingArgs = new Paging { Page = 1, PageSize = 20 }
        /// };
        ///
        /// var query = dbContext.Users.AsQueryable();
        ///
        /// if (search.Order is not null)
        /// {
        ///     query = search.Order.Direction == SortDirection.Ascending
        ///         ? query.OrderBy(search.Order.Property)
        ///         : query.OrderByDescending(search.Order.Property);
        /// }
        ///
        /// query = query.ApplyPaging(search.PagingArgs);
        /// </code>
        /// </example>
    }
}