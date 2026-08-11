using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using CliFx;
using CliFx.Binding;
using CliFx.Infrastructure;
using CliWrap;
using CliWrap.Buffered;

namespace NixFodExporter;

[Command("from-derivations")]
public partial class FromDerivationsCommand : ICommand
{
    [CommandParameter(0)]
    public required DirectoryInfo Output { get; set; }

    [CommandParameter(1)]
    public required FileInfo Derivations { get; set; }

    private async IAsyncEnumerable<FileInfo> RealiseAsync(
        string drv, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var command = Cli.Wrap("nix-store").WithArguments([
            "--realise",
            Path.GetFullPath(drv, "/nix/store")
        ]);
        Console.WriteLine(command);
        var output = await command.ExecuteBufferedAsync(cancellationToken);
        Console.WriteLine(output.StandardOutput);
        foreach (var line in output.StandardOutput.Split(
            Environment.NewLine,
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            yield return new FileInfo(line);
        }
    }

    private async ValueTask<string> QueryHashMethodAsync(
        FileInfo drvOutput, CancellationToken cancellationToken
    )
    {
        var command = Cli.Wrap("nix-store").WithArguments(["--query", "--hash", drvOutput.FullName]);
        Console.WriteLine(command);
        var hashOutput = await command.ExecuteBufferedAsync(cancellationToken);
        Console.WriteLine(hashOutput.StandardOutput);
        return hashOutput.StandardOutput.Split(":", 2)[0];
    }

    private async ValueTask<bool> QueryHashModeIsFlatAsync(
        FileInfo drvOutput, CancellationToken cancellationToken
    )
    {
        var command = Cli.Wrap("nix-store").WithArguments([
            "--query", "--binding", "outputHashMode",
            drvOutput.FullName
        ]);
        Console.WriteLine(command);
        var hashOutput = await command.ExecuteBufferedAsync(cancellationToken);
        Console.WriteLine(hashOutput.StandardOutput);
        return hashOutput.StandardOutput.Trim() == "flat";
    }

    private async ValueTask DumpAsync(
        FileInfo drvOutput, string outputPath, CancellationToken cancellationToken
    )
    {
        var command = Cli.Wrap("nix-store").WithArguments(["--dump", drvOutput.FullName]);
        Console.WriteLine(command);
        await command.WithStandardOutputPipe(PipeTarget.ToFile(outputPath))
            .ExecuteAsync(cancellationToken);
        Console.WriteLine("Dumped.");
    }

    private async ValueTask ExecuteAsync(JsonNode? derivationsJson,  CancellationToken cancellationToken)
    {
        var outputStoreDirectory = Output.CreateSubdirectory("store");
        await using var script = new StreamWriter(
            Path.Join(Output.FullName, "store.sh")
        );
        await script.WriteLineAsync("set -e".AsMemory(), cancellationToken);
        await script.WriteLineAsync("mkdir -p temp".AsMemory(), cancellationToken);

        var derivations = derivationsJson?["derivations"];
        var fods = derivations?.AsObject()
            .Where(x => x.Value?["outputs"]?["out"]?["hash"] is not null)
            .Select(x => x.Key) ?? [];

        foreach (var fod in fods)
        {
            await foreach (var drvOutput in this.RealiseAsync(fod, cancellationToken))
            {
                await DumpAsync(drvOutput, Path.Combine(
                    outputStoreDirectory.FullName, drvOutput.Name
                ), cancellationToken);

                var hashMethod = await QueryHashMethodAsync(drvOutput, cancellationToken);
                var isFlat = await QueryHashModeIsFlatAsync(drvOutput, cancellationToken);

                var shortName = drvOutput.Name.Split("-", 2)[1];
                await script.WriteLineAsync(
                    $"nix-store --restore temp/{shortName} < store/{drvOutput.Name}".AsMemory(),
                    cancellationToken
                );
                await script.WriteLineAsync(
                    $"nix-store --add-fixed {(isFlat ? "" : "--recursive ")}{hashMethod} temp/{shortName}".AsMemory(),
                    cancellationToken
                );
                await script.WriteLineAsync(
                    $"rm -rf temp/{shortName}".AsMemory(),
                    cancellationToken
                );
            }
        }
    }

    public ValueTask ExecuteAsync(string derivations, CancellationToken cancellationToken)
    {
        return this.ExecuteAsync(JsonNode.Parse(derivations), cancellationToken);
    }

    public async ValueTask ExecuteAsync(Stream derivations, CancellationToken cancellationToken)
    {
        var node = await JsonNode.ParseAsync(derivations, cancellationToken: cancellationToken);
        await this.ExecuteAsync(node, cancellationToken);
    }

    public async ValueTask ExecuteAsync(IConsole console)
    {
        var cancellationToken = console.RegisterCancellationHandler();
        using var stream = Derivations.OpenRead();
        await this.ExecuteAsync(stream, cancellationToken);
    }
}