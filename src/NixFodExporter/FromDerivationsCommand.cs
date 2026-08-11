using System.Text.Json;
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

    private async ValueTask ExecuteAsync(JsonNode? derivationsJson, CancellationToken cancellationToken)
    {
        var derivations = derivationsJson?["derivations"];
        var fods = derivations?.AsObject()
            .Select(kv => kv.Value?["value"]?["output"]?["out"]?["hash"] is not null);
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
        await this.ExecuteAsync(console.Input.BaseStream, cancellationToken);
    }
}