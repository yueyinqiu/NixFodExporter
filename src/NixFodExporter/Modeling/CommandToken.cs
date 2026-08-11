using System.Text.Json.Serialization;

namespace Snavi.Modeling;

[JsonDerivedType(typeof(CommandTokenLiteral), nameof(CommandTokenLiteral))]
[JsonDerivedType(typeof(CommandTokenVariable), nameof(CommandTokenVariable))]
public abstract record CommandToken;
