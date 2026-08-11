using Snavi.Core;
using Snavi.Modeling;

namespace Snavi.Executing;

sealed class ArgumentSuggesterExector(string dotnet) : IArgumentSuggesterExecutor<ArgumentSuggesterBase>
{
    public IAsyncEnumerable<ArgumentSuggestion> RunAsync(
        ArgumentSuggesterBase suggester,
        DirectoryInfo? directory,
        IReadOnlyList<string> givenArguments,
        CancellationToken cancellationToken
    )
    {
        return suggester switch
        {
            ArgumentSuggesterCsharp csharp => new ArgumentSuggesterCsharpExecutor(dotnet).RunAsync(
                csharp, directory, givenArguments, cancellationToken
            ),
            ArgumentSuggesterEmpty empty => new ArgumentSuggesterEmptyExecutor().RunAsync(
                empty, directory, givenArguments, cancellationToken
            ),
            _ => throw new Exception($"Unknown argument suggester type '{suggester.GetType().Name}'.")
        };
    }
}
