namespace Results.Models;

using Results.Enums;
using Results.Extensions;

public class BaseSearchModel
{
    public string OrderBy { get; set; }

    public SortDirection SortDirection { get; set; }

    public Paging? PagingArgs { get; set; } = Paging.Default;

    internal string Ordering => $"{this.OrderBy} {this.SortDirection.GetDescription()}";
}
