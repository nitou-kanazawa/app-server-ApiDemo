using GameServer.Application.DTOs;
using GameServer.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameServer.API.Controllers;

[ApiController]
[Route("api/scores")]
public class ScoreController : ControllerBase
{
    private readonly IScoreService _scoreService;

    public ScoreController(IScoreService scoreService)
    {
        _scoreService = scoreService;
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
            return Unauthorized(new { message = "Invalid token" });
        }

        var (success, scoreId, errorMessage) = await _scoreService.SubmitScoreAsync(userId, request);

        if (!success)
        {
            return BadRequest(new { message = errorMessage });
        }

        return Ok(new { message = "Score submitted successfully", scoreId });
    }

    /// <summary>
    /// ランキング取得（トップN件）
    /// 日付フィルタリングに対応
    /// </summary>
    [HttpGet("ranking")]
    public async Task<IActionResult> GetRanking(
        [FromQuery] int limit = 10,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        if (limit <= 0 || limit > 100)
        {
            return BadRequest(new { message = "Limit must be between 1 and 100" });
        }

        var parameters = new RankingQueryParameters
        {
            Limit = limit,
            StartDate = startDate,
            EndDate = endDate
        };

        var rankings = await _scoreService.GetRankingAsync(parameters);
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
            return Unauthorized(new { message = "Invalid token" });
        }

        var bestScore = await _scoreService.GetUserBestScoreAsync(userId);

        if (bestScore == null)
        {
            return Ok(new { message = "No score found", score = 0, submittedAt = (DateTime?)null });
        }

        return Ok(bestScore);
    }
}