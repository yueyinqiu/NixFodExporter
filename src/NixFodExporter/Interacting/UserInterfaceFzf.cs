using System.Collections;
using System.Text;
using CliWrap;
using CliWrap.Buffered;
using Snavi.Core;

namespace Snavi.Interacting;

sealed class UserInterfaceFzf(string fzf) : IUserInterface
{
    private IEnumerable<int> GetWhiteSpaceCountForTable(
        IEnumerable<string> firstColumn,
        int limit,
        int minimal = 4
    )
    {
        firstColumn = [.. firstColumn];

        var suggestedLength = firstColumn.Max(x => x.Length);

        limit -= minimal;
        if (suggestedLength > limit)
            suggestedLength = limit;

        foreach (var item in firstColumn)
        {
            var difference = suggestedLength - item.Length;
            if (difference < 0)
                difference = 0;
            yield return difference + minimal;
        }
    }

    public async Task<T?> PickAsync<T>(
        IHighlightedString title,
        string prompt,
        IAsyncEnumerable<T> suggestions,
        CancellationToken cancellationToken
    ) where T : IPickable
    {
        var suggestionDictionary = await suggestions.Index().ToDictionaryAsync();
        var whiteSpaces = GetWhiteSpaceCountForTable(
            suggestionDictionary.Select(x => x.Value.Value),
            Console.WindowWidth / 2
        );

        var delimiter = Guid.NewGuid().ToString("N");

        var command = Cli.Wrap(fzf);
        command = command.WithValidation(CommandResultValidation.None);
        command = command.WithArguments([
            "--ansi",
            "--header", ToAnsiString(title),

            "--accept-nth", "1",
            "--with-nth", "{2}{4}{3}",
            "--delimiter", delimiter,
            "--bind", "tab:transform-query(printf '%s' {2})",
            "--preview", $"printf '{prompt}%s\\n' {{2}}",
            "--preview-window", "down:1:border"
        ]);
        command = command.WithStandardInputPipe(PipeSource.FromString(
            string.Join(
                Environment.NewLine,
                from x in suggestionDictionary.Zip(whiteSpaces)
                let key = x.First.Key
                let value = x.First.Value.Value
                let description = x.First.Value.Description
                let whiteSpace = new string(' ', x.Second)
                select $"{key}{delimiter}{value}{delimiter}{description}{delimiter}{whiteSpace}")
            )
        );
        var output = await command.ExecuteBufferedAsync(cancellationToken);

        if (!int.TryParse(output.StandardOutput, out var index))
            index = -1;
        _ = suggestionDictionary.TryGetValue(index, out var selection);
        return selection;
    }

    public async Task<string?> InputAsync(
        IHighlightedString title,
        string prompt,
        IAsyncEnumerable<IPickable> suggestions,
        CancellationToken cancellationToken
    )
    {
        var suggestionsSync = await suggestions.ToArrayAsync();
        var whiteSpaces = GetWhiteSpaceCountForTable(
            suggestionsSync.Select(x => x.Value),
            Console.WindowWidth / 2
        );

        var delimiter = Guid.NewGuid().ToString("N");

        var command = Cli.Wrap(fzf);
        command = command.WithValidation(CommandResultValidation.None);
        command = command.WithArguments([
            "--print-query",
            "--ansi",
            "--header", ToAnsiString(title),

            "--with-nth", "{1}{3}{2}",
            "--delimiter", delimiter,
            "--bind", "tab:transform-query(printf '%s' {1})",
            "--preview", $"printf '{prompt}%s\\n' {{q}}",
            "--preview-window", "down:1:border"
        ]);
        command = command.WithStandardInputPipe(PipeSource.FromString(
            string.Join(
                Environment.NewLine,
                from x in suggestionsSync.Zip(whiteSpaces)
                let value = x.First.Value
                let description = x.First.Description
                let whiteSpace = new string(' ', x.Second)
                select $"{value}{delimiter}{description}{delimiter}{whiteSpace}")
            )
        );
        var output = await command.ExecuteBufferedAsync(cancellationToken);
        if (output.ExitCode != 0 && output.ExitCode != 1)
            return null;
        return output.StandardOutput.Split(Environment.NewLine)[0];
    }

    private static string ToAnsiString(IHighlightedString highlightedString)
    {
        var (offset, length) = highlightedString.Highlight.GetOffsetAndLength(highlightedString.String.Length);
        var builder = new StringBuilder();
        builder.Append(highlightedString.String, 0, offset);
        builder.Append("\x1b[4m");
        builder.Append(highlightedString.String, offset, length);
        builder.Append("\x1b[0m");
        builder.Append(highlightedString.String, offset + length, highlightedString.String.Length - offset - length);
        return builder.ToString();
    }
}
