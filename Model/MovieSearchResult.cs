namespace moodies_backend.Model;

public sealed record MovieSearchResult
{
    public long Id { get; init; }
    public string Title { get; init; }
    public long Score { get; init; }
}