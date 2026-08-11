using System.Text.Json.Serialization;

namespace Snavi.Modeling;

public sealed record CommandTokenLiteral(string Value) : CommandToken;
