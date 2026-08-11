using Snavi.Executing;
using Snavi.Modeling;

namespace Snavi.Core;

interface IArgumentSuggesterExecutor<T> where T : ArgumentSuggesterBase
{
    IAsyncEnumerable<ArgumentSuggestion> RunAsync(
        T suggester,
        DirectoryInfo? directory,
        IReadOnlyList<string> givenArguments,
        CancellationToken cancellationToken
    );
}
