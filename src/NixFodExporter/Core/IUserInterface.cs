using Snavi.Interacting;

namespace Snavi.Core;

interface IUserInterface
{
    Task<T?> PickAsync<T>(
        IHighlightedString title,
        string prompt,
        IAsyncEnumerable<T> suggestions,
        CancellationToken cancellationToken
    ) where T : IPickable;

    Task<string?> InputAsync(
        IHighlightedString title,
        string prompt,
        IAsyncEnumerable<IPickable> suggestions,
        CancellationToken cancellationToken
    );
}
