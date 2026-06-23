namespace GameServer.Application.DTOs;

public record SubmitScoreRequest(int Score);

public record RankingEntry
{
    public string Username { get; init; } = string.Empty;
    public int Score { get; init; }
    public DateTime SubmittedAt { get; init; }
}

public record BestScoreResponse
{
    public int Score { get; init; }
    public DateTime SubmittedAt { get; init; }
}

public record RankingQueryParameters
{
    public int Limit { get; init; } = 10;
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}
