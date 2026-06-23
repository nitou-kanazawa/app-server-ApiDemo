using GameServer.Application.DTOs;
using GameServer.Application.Interfaces;
using GameServer.Application.Models;
using GameServer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GameServer.Infrastructure.Services;

public class ScoreService : IScoreService
{
    private readonly AppDbContext _dbContext;

    public ScoreService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(bool Success, int? ScoreId, string? ErrorMessage)> SubmitScoreAsync(
        int userId,
        SubmitScoreRequest request)
    {
        // ユーザーの存在確認
        var userExists = await _dbContext.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            return (false, null, "User not found");
        }

        var score = new Score
        {
            UserId = userId,
            Value = request.Score,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Scores.Add(score);
        await _dbContext.SaveChangesAsync();

        return (true, score.Id, null);
    }

    public async Task<IEnumerable<RankingEntry>> GetRankingAsync(RankingQueryParameters parameters)
    {
        var query = _dbContext.Scores
            .Include(s => s.User)
            .AsQueryable();

        // 日付フィルタリング
        if (parameters.StartDate.HasValue)
        {
            query = query.Where(s => s.CreatedAt >= parameters.StartDate.Value);
        }

        if (parameters.EndDate.HasValue)
        {
            query = query.Where(s => s.CreatedAt <= parameters.EndDate.Value);
        }

        var rankings = await query
            .OrderByDescending(s => s.Value)
            .ThenBy(s => s.CreatedAt)
            .Take(parameters.Limit)
            .Select(s => new RankingEntry
            {
                Username = s.User.Username,
                Score = s.Value,
                SubmittedAt = s.CreatedAt
            })
            .ToListAsync();

        return rankings;
    }

    public async Task<BestScoreResponse?> GetUserBestScoreAsync(int userId)
    {
        var bestScore = await _dbContext.Scores
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.Value)
            .FirstOrDefaultAsync();

        if (bestScore == null)
        {
            return null;
        }

        return new BestScoreResponse
        {
            Score = bestScore.Value,
            SubmittedAt = bestScore.CreatedAt
        };
    }
}
