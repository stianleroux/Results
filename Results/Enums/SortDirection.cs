namespace Results.Enums;

using System.ComponentModel;
using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SortDirection
{
    /// <summary>
    /// The ascending
    /// </summary>
    [Description("Asc")]
    Ascending,

    /// <summary>
    /// The descending
    /// </summary>
    [Description("Desc")]
    Descending
}