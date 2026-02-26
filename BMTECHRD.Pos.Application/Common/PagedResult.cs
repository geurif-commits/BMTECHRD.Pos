namespace BMTECHRD.Pos.Application.Common;
public sealed class PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public required int Total { get; init; }
}