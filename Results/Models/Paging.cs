namespace Results.Models;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public sealed class Paging
{
	private int pageNumber = 1;

	private int pageSize = 20;

	public static Paging NoPaging => new() { UsePaging = false };

	public static Paging Default => new() { UsePaging = true, PageSize = 20, PageNumber = 1 };

	public static Paging FirstItem => new() { UsePaging = true, PageSize = 20, PageNumber = 1 };

	public int PageNumber
	{
		get => this.pageNumber;

		set
        {
            if (value < 1)
            {
                value = 1;
            }

            this.pageNumber = value;
        }
    }

	public int PageSize
	{
		get => this.pageSize;

		set
		{
			if (value < 1)
			{
				value = 20;
			}

			this.pageSize = value;
		}
	}

	public int SkipAmount => (this.PageNumber - 1) * this.PageSize;

	public bool UsePaging { get; set; }
}
