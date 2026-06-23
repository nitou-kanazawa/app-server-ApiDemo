using GameServer.Application.DTOs;

namespace GameServer.Application.Interfaces;

public interface IScoreService
{
    Task<(bool Success, int? ScoreId, string? ErrorMessage)> SubmitScoreAsync(int userId, SubmitScoreRequest request);
    Task<IEnumerable<RankingEntry>> GetRankingAsync(RankingQueryParameters parameters);
    Task<BestScoreResponse?> GetUserBestScoreAsync(int userId);
}
