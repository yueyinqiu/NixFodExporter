using Snavi.Modeling;
using Snavi.Interacting;

namespace Snavi.Helpers;

public sealed record PickingCommand(DirectoryInfo? Directory, CheatFile Cheat) : IPickable
{
    public string Value => RenderingCommand.FromCommand(Cheat.Command, null, Cheat.ExtraArguments).String;

    public string Description => Cheat.Description;
}