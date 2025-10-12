namespace GameServer.Application.Models;

public class Score
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int Value { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ナビゲーションプロパティ
    public User User { get; set; } = null!;
}