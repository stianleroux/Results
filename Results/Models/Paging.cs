namespace Results.Models;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public sealed class Paging
{
    private const int DefaultPageSize = 20;
    private int pageNumber = 1;
    private int pageSize = DefaultPageSize;

    public static Paging NoPaging => new() { UsePaging = false };

    public static Paging Default => new() { UsePaging = true, PageSize = DefaultPageSize, PageNumber = 1 };

    public int PageNumber
    {
        get => this.pageNumber;
        set => this.pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => this.pageSize;
        set => this.pageSize = value < 1 ? DefaultPageSize : value;
    }

    public int SkipAmount => (this.PageNumber - 1) * this.PageSize;

    public bool UsePaging { get; set; }
}
