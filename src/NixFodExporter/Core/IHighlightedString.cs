namespace Snavi.Core;

interface IHighlightedString
{
    string String { get; }
    Range Highlight { get; }
}