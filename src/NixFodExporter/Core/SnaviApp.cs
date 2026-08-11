using System.Runtime.CompilerServices;
using System.Text.Json;
using Snavi.Executing;
using Snavi.Helpers;
using Snavi.Modeling;

namespace Snavi.Core;

sealed class SnaviApp(
    IReadOnlyList<FileInfo> cheats,
    IUserInterface ui,
    IArgumentSuggesterExecutor<ArgumentSuggesterBase> executor
)
{
    private async IAsyncEnumerable<PickingCommand> EnumerateValidCheatsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        bool notFound = true;
        foreach (var file in cheats)
        {
            CheatFile? cheatFile;
            try
            {
                using var stream = file.OpenRead();
                cheatFile = await JsonSerializer.DeserializeAsync(
                    stream,
                    CheatFileSerializerContext.Default.CheatFile,
                    cancellationToken
                );
            }
            catch (Exception)
            {
                cheatFile = null;
            }

            if (cheatFile is not null)
            {
                notFound = false;
                yield return new (file.Directory, cheatFile);
            }
        }

        if (notFound)
        {
            yield return new (null,
                new CheatFile(
                    "No valid cheat file is found.",
                    [], false
                )
            );
        }
    }

    public async Task<string> RunAsync(CancellationToken cancellationToken)
    {
        var cheat = await ui.PickAsync(
            new RenderingCommand("Choose a Command", 0..0),
            "Command: ",
            this.EnumerateValidCheatsAsync(cancellationToken),
            cancellationToken
        );

        if (cheat is null)
            return "Cancelled.";

        var arguments = new List<string>();
        foreach (var token in cheat.Cheat.Command)
        {
            if (token is CommandTokenVariable variable)
            {
                var header = RenderingCommand.FromCommand(cheat.Cheat.Command, arguments, cheat.Cheat.ExtraArguments);
                var suggestions = executor.RunAsync(variable.Suggester, cheat.Directory, arguments, cancellationToken);
                var value = await ui.InputAsync(header, $"{variable.Name} = ", suggestions, cancellationToken);
                if (value is null)
                    return "Cancelled.";
                arguments.Add(value);
            }
        }

        if (cheat.Cheat.ExtraArguments)
        {
            var value = await ui.InputAsync(
                RenderingCommand.FromCommand(cheat.Cheat.Command, arguments, true),
                "Extra Arguments: ",
                Array.Empty<ArgumentSuggestion>().ToAsyncEnumerable(),
                cancellationToken
            );
            if (value is null)
                return "Cancelled.";
            arguments.Add(value);
        }
        return RenderingCommand.FromCommand(cheat.Cheat.Command, arguments, cheat.Cheat.ExtraArguments).String;
    }
}
