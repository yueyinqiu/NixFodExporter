using CliFx;
using CliFx.Binding;
using CliFx.Infrastructure;
using CliWrap;
using CliWrap.Buffered;

namespace NixFodExporter;

[Command("from-installables")]
public partial class FromInstallablesCommand : ICommand
{
    [CommandParameter(0)]
    public required DirectoryInfo Output { get; set; }

    [CommandParameter(1)]
    public required IReadOnlyList<string> Installables { get; set; }

    public async ValueTask ExecuteAsync(IConsole console)
    {
        var cancellationToken = console.RegisterCancellationHandler();
        var command = Cli.Wrap("nix").WithArguments(["derivation", "show", "-r", .. Installables]);
        var result = await command.ExecuteBufferedAsync(cancellationToken);
        await new FromDerivationsCommand()
        {
            Output = this.Output,
            Derivations = null!
        }.ExecuteAsync(result.StandardOutput, cancellationToken);
    }
}