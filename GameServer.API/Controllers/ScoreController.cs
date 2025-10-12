using GameServer.Application.Models;
using GameServer.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameServer.API.Controllers;

[ApiController]
[Route("api/scores")]
public class ScoreController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public ScoreController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// スコア送信（JWT認証必須）
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> SubmitScore([FromBody] SubmitScoreRequest request)
    {
        // JWTトークンからユーザーIDを取得
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("Invalid token");
        }

        var score = new Score
        {
            UserId = userId,
            Value = request.Score,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Scores.Add(score);
        await _dbContext.SaveChangesAsync();

        return Ok(new { message = "Score submitted successfully", scoreId = score.Id });
    }

    /// <summary>
    /// ランキング取得（トップN件）
    /// </summary>
    [HttpGet("ranking")]
    public async Task<IActionResult> GetRanking([FromQuery] int limit = 10)
    {
        if (limit <= 0 || limit > 100)
        {
            return BadRequest("Limit must be between 1 and 100");
        }

        var rankings = await _dbContext.Scores
            .Include(s => s.User)
            .OrderByDescending(s => s.Value)
            .ThenBy(s => s.CreatedAt)
            .Take(limit)
            .Select(s => new RankingEntry
            {
                Username = s.User.Username,
                Score = s.Value,
                SubmittedAt = s.CreatedAt
            })
            .ToListAsync();

        return Ok(rankings);
    }

    /// <summary>
    /// 自分のベストスコア取得（JWT認証必須）
    /// </summary>
    [Authorize]
    [HttpGet("me/best")]
    public async Task<IActionResult> GetMyBestScore()
    {
        // JWTトークンからユーザーIDを取得
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("Invalid token");
        }

        var bestScore = await _dbContext.Scores
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.Value)
            .FirstOrDefaultAsync();

        if (bestScore == null)
        {
            return Ok(new { message = "No score found", score = 0 });
        }

        return Ok(new
        {
            score = bestScore.Value,
            submittedAt = bestScore.CreatedAt
        });
    }
}

#region リクエスト・レスポンス用DTO

public record SubmitScoreRequest(int Score);

public record RankingEntry
{
    public string Username { get; init; } = string.Empty;
    public int Score { get; init; }
    public DateTime SubmittedAt { get; init; }
}

#endregion