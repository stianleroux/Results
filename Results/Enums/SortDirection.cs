namespace Results.Enums;

using System.ComponentModel;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

[Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
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