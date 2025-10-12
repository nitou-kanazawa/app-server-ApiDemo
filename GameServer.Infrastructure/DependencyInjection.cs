using GameServer.Application.Interfaces;
using GameServer.Infrastructure.Data;
using GameServer.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameServer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // データベースプロバイダーの取得
        var dbProvider = configuration["Database:Provider"] ?? "Sqlite";
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // プロバイダーに応じてDbContextを設定
        services.AddDbContext<AppDbContext>(options =>
        {
            switch (dbProvider.ToLower())
            {
                case "sqlite":
                    options.UseSqlite(connectionString);
                    break;

                case "postgresql":
                case "postgres":
                    options.UseNpgsql(connectionString);
                    break;

                default:
                    throw new ArgumentException(
                        $"Unsupported database provider: {dbProvider}. " +
                        $"Supported providers are: Sqlite, PostgreSQL");
            }
        });

        // サービスの登録
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IScoreService, ScoreService>();

        return services;
    }
}
