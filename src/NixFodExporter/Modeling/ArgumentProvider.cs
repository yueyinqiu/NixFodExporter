using System.Text.Json.Serialization;

namespace Snavi.Modeling;

[JsonDerivedType(typeof(ArgumentSuggesterCsharp), nameof(ArgumentSuggesterCsharp))]
[JsonDerivedType(typeof(ArgumentSuggesterEmpty), nameof(ArgumentSuggesterEmpty))]
public abstract record ArgumentSuggesterBase;
