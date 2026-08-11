using System.Runtime.CompilerServices;
using System.Text.Json;
using CliWrap;
using Snavi.ArgumentSuggester.Raw;
using Snavi.Core;
using Snavi.Modeling;

namespace Snavi.Executing;

sealed class ArgumentSuggesterCsharpExecutor(string dotnet) : IArgumentSuggesterExecutor<ArgumentSuggesterCsharp>
{
    public async IAsyncEnumerable<ArgumentSuggestion> RunAsync(
        ArgumentSuggesterCsharp suggester,
        DirectoryInfo? directory,
        IReadOnlyList<string> givenArguments,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        if (directory is null)
        {
            yield return new ArgumentSuggestion("No suggestion provided as the cheat file is not located in any directory.", "");
            yield break;
        }

        var logs = Directory.CreateTempSubdirectory();

        var outputs = logs.CreateSubdirectory("outputs");
        var suggestions = Path.Combine(outputs.FullName, "suggestions.json");

        var command = Cli.Wrap(dotnet);
        command = command.WithArguments([
            "run", Path.GetFullPath(suggester.ScriptPath, directory.FullName)
        ]);
        command = command.WithStandardOutputPipe(PipeTarget.ToFile(Path.Combine(outputs.FullName, "stdout.txt")));
        command = command.WithStandardErrorPipe(PipeTarget.ToFile(Path.Combine(outputs.FullName, "stderr.txt")));
        command = command.WithStandardInputPipe(PipeSource.FromString(
            JsonSerializer.Serialize(new ArgumentSuggesterInput(
                suggestions,
                Environment.CurrentDirectory,
                logs.CreateSubdirectory("temp").FullName,
                givenArguments
            ), ArgumentSuggesterInputSerializerContext.Default.ArgumentSuggesterInput)
        ));
        command = command.WithWorkingDirectory(directory.FullName);
        command = command.WithValidation(CommandResultValidation.None);

        ArgumentSuggesterOutput? outputSuggestions;
        try
        {
            await command.ExecuteAsync(cancellationToken);
            using var stream = File.OpenRead(suggestions);
            outputSuggestions = await JsonSerializer.DeserializeAsync(
                stream,
                ArgumentSuggesterOutputSerializerContext.Default.ArgumentSuggesterOutput,
                cancellationToken
            ) ?? new ArgumentSuggesterOutput([]);
        }
        catch (Exception exception)
        {
            outputSuggestions = null;
            await File.WriteAllTextAsync(
                Path.Combine(outputs.FullName, "snavi-exception.txt"),
                exception.ToString(),
                cancellationToken
            );
        }

        if (outputSuggestions is null)
        {
            yield return new ArgumentSuggestion(
                $"No suggestion provided as an error occurred. For more information, see {logs.FullName}",
                ""
            );
            yield break;
        }

        try
        {
            logs.Delete(true);
        }
        catch
        {

        }

        foreach (var suggestion in outputSuggestions.Suggestions)
        {
            yield return new ArgumentSuggestion(suggestion.Value, suggestion.Description);
        }
    }
}
