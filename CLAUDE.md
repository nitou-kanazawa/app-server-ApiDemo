# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Unity向けゲームAPIサーバー（学習用）。ASP.NET Core Web APIで構築され、JWT認証とスコアランキング機能を提供する。

## Technology Stack

- .NET 9.0 / ASP.NET Core Web API
- Entity Framework Core + SQLite (データベース: `game.db`)
- JWT認証 (BCrypt.Net-Next でパスワードハッシュ化)
- Swagger UI (開発環境のみ)

## Essential Commands

### Build and Run
```bash
# プロジェクトのビルド
dotnet build GameServer/GameServer.csproj

# 開発サーバーの起動
dotnet run --project GameServer/GameServer.csproj

# リリースビルド
dotnet build GameServer/GameServer.csproj -c Release
```

### Database Management
```bash
# マイグレーションの作成
dotnet ef migrations add <MigrationName> --project GameServer

# データベースの更新（マイグレーション適用）
dotnet ef database update --project GameServer

# マイグレーション履歴の確認
dotnet ef migrations list --project GameServer

# データベースの削除
dotnet ef database drop --project GameServer
```

### Testing and Development
```bash
# Swagger UI でAPIテスト
# アプリ起動後、ブラウザで https://localhost:<port>/swagger にアクセス

# Entity Framework Core ツールのインストール（初回のみ）
dotnet tool install --global dotnet-ef
```

## Architecture

### Core Components

**Program.cs** - アプリケーションエントリーポイント
- DbContext設定（SQLite接続: `game.db`）
- JWT認証設定（デフォルトキー: `SuperSecretKey12345`、または `appsettings.json` の `Jwt:Key` から取得）
- Swagger/OpenAPI設定（開発環境のみ）
- Controllerルーティング設定

**Models/AppDbContext.cs** - Entity Framework Core DbContext
- `Users` と `Scores` のDbSet定義
- SQLiteデータベースとのやり取りを管理

**Controllers/AuthController.cs** - 認証エンドポイント
- `POST /Auth/api/register` - ユーザー登録（BCryptでパスワードハッシュ化）
- `POST /Auth/api/login` - ログイン（JWT発行）
- JWT生成ロジック（7日間有効）

### Data Models

**User** (`Models/User.cs`)
- `Id` (int, PK)
- `Username` (string)
- `PasswordHash` (string, BCrypt)

**Score** (`Models/Score.cs`)
- `Id` (int, PK)
- `UserId` (int, FK)
- `Value` (int)
- `CreatedAt` (DateTime, UTC)

### Authentication Flow

1. ユーザー登録: クライアントが username/password を送信 → BCryptでハッシュ化してDB保存
2. ログイン: username/password 検証 → JWT発行（claims: id, username）
3. 認証が必要なエンドポイント: `[Authorize]` 属性でJWT検証

### Configuration

**JWT設定**
- キーは `appsettings.json` の `Jwt:Key` から取得
- 未設定の場合はデフォルト値 `SuperSecretKey12345` を使用
- トークン有効期限: 7日間
- HTTPS検証は無効化されている（開発用）

## Development Notes

- 現在 `ScoreController` は未実装（README.txt に記載されているが、コントローラーファイルが存在しない）
- `WeatherForecastController.cs` はテンプレートファイル（削除可能）
- SQLiteデータベースファイル `game.db` はプロジェクトルートに生成される
- マイグレーションは `Migrations/` ディレクトリに格納