using System.Text.Json.Serialization;

namespace Snavi.Modeling;

public sealed record CommandTokenVariable(string Name, ArgumentSuggesterBase Suggester) : CommandToken;
