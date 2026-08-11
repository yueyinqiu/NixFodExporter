using Snavi.Interacting;

namespace Snavi.Executing;

sealed record ArgumentSuggestion(string Value, string Description) : IPickable;