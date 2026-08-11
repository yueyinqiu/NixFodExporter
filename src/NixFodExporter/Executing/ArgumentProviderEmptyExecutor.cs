using System.Runtime.CompilerServices;
using Snavi.Core;
using Snavi.Modeling;

namespace Snavi.Executing;

sealed class ArgumentSuggesterEmptyExecutor : IArgumentSuggesterExecutor<ArgumentSuggesterEmpty>
{
    public async IAsyncEnumerable<ArgumentSuggestion> RunAsync(
        ArgumentSuggesterEmpty suggester,
        DirectoryInfo? directory,
        IReadOnlyList<string> givenArguments,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        yield break;
    }
}
