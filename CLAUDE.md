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
- JWT認証設定（デフォルトキー: `SuperSecretKey12345SuperSecretKey12345`、または `appsettings.Development.json` の `Jwt:Key` から取得）
  - HS256アルゴリズムは256ビット（32バイト）以上の鍵が必要
- Swagger/OpenAPI設定（開発環境のみ）
  - HTTP Bearer認証方式（`SecuritySchemeType.Http`）でJWTトークンを自動的に `Bearer` プレフィックス付きで送信
- Controllerルーティング設定

**Models/AppDbContext.cs** - Entity Framework Core DbContext
- `Users` と `Scores` のDbSet定義
- SQLiteデータベースとのやり取りを管理
- User-Score の1対多リレーション設定（外部キー制約、カスケード削除）

**Controllers/AuthController.cs** - 認証エンドポイント
- `POST /Auth/api/register` - ユーザー登録（BCryptでパスワードハッシュ化）
- `POST /Auth/api/login` - ログイン（JWT発行）
- JWT生成ロジック（7日間有効、claims: id, username）

**Controllers/ScoreController.cs** - スコアランキングエンドポイント
- `POST /Score/api/submit` - スコア送信（JWT認証必須）
- `GET /Score/api/ranking?limit=10` - ランキング取得（トップN件、デフォルト10件、最大100件）
- `GET /Score/api/mybest` - 自分のベストスコア取得（JWT認証必須）

### Data Models

**User** (`Models/User.cs`)
- `Id` (int, PK)
- `Username` (string)
- `PasswordHash` (string, BCrypt)

**Score** (`Models/Score.cs`)
- `Id` (int, PK)
- `UserId` (int, FK to User)
- `Value` (int)
- `CreatedAt` (DateTime, UTC)
- `User` (ナビゲーションプロパティ) - ランキング取得時にユーザー名を取得するために使用

### Authentication & Authorization Flow

1. **ユーザー登録**: クライアントが username/password を送信 → BCryptでハッシュ化してDB保存
2. **ログイン**: username/password 検証 → JWT発行（claims: id, username、有効期限7日間）
3. **認証が必要なエンドポイント**: `[Authorize]` 属性でJWT検証
   - JWTトークンからユーザーIDを取得: `User.Claims.FirstOrDefault(c => c.Type == "id")`
   - スコア送信や自分のスコア取得などで使用

### Swagger UI での認証テスト

1. `/Auth/api/login` でログインし、レスポンスからトークンを取得
2. Swagger UI 右上の **「Authorize」ボタン** をクリック
3. トークンのみを入力（`Bearer` プレフィックスは不要、自動付与される）
4. 認証が必要なエンドポイント（`/Score/api/submit`, `/Score/api/mybest`）にアクセス可能

### Configuration

**JWT設定**
- キーは `appsettings.Development.json` の `Jwt:Key` から取得
- 未設定の場合はデフォルト値 `SuperSecretKey12345SuperSecretKey12345` を使用
- **重要**: HS256アルゴリズムは256ビット（32バイト、32文字）以上の鍵が必要。短い鍵を使用すると `IDX10720` エラーが発生
- トークン有効期限: 7日間
- HTTPS検証は無効化されている（開発用）

## Development Notes

- `WeatherForecastController.cs` はテンプレートファイル（削除可能）
- SQLiteデータベースファイル `game.db` はプロジェクトルートに生成される
- マイグレーションは `Migrations/` ディレクトリに格納
- ランキングは `Value` 降順、同点の場合は `CreatedAt` 昇順（早い方が上位）でソート