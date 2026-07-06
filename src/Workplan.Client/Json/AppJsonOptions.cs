using System.Text.Json;
using System.Text.Json.Serialization;

namespace Workplan.Client.Json;

public static class AppJsonOptions
{
    public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}
