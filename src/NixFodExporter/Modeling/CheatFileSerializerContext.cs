using System.Text.Json.Serialization;

namespace Snavi.Modeling;

[JsonSourceGenerationOptions(
    AllowTrailingCommas = true,
    ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip,
    RespectNullableAnnotations = true,
    RespectRequiredConstructorParameters = true
)]
[JsonSerializable(typeof(CheatFile))]
partial class CheatFileSerializerContext : JsonSerializerContext
{
}