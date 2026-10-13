using System.Text.Json;

namespace Pos.BuildingBlocks.Infrastructure;

/// <summary>Single source of truth for message serialization settings.</summary>
internal static class MessagingJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}
