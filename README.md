# Unity Game API Server

ASP.NET Core Web API で作る、学習用のゲームサーバーです。
認証（JWT）＋ランキング（スコア送信・取得）をローカルで試すことを目的としています。

## 技術スタック

- .NET 9.0 / ASP.NET Core Web API
- Entity Framework Core + SQLite
- JWT 認証（BCrypt.Net-Next でパスワードハッシュ化）
- Swagger UI（開発環境）
- Unity WebRequest からのアクセス想定

## プロジェクト構成

```
app-server-ApiDemo/
├── GameServer.API/            # プレゼンテーション層
│   ├── Controllers/
│   │   ├── AuthController.cs      # ユーザー登録・ログイン（JWT）
│   │   └── ScoreController.cs     # スコア送信・ランキング取得
│   ├── Program.cs                 # アプリ起動・サービス登録
│   └── appsettings.json           # アプリ設定
├── GameServer.Application/    # アプリケーション層
│   ├── DTOs/                      # データ転送オブジェクト
│   │   ├── AuthDtos.cs
│   │   └── ScoreDtos.cs
│   ├── Interfaces/                # サービスインターフェース
│   │   ├── IAuthService.cs
│   │   └── IScoreService.cs
│   └── Models/                    # ドメインモデル
│       ├── User.cs
│       └── Score.cs
└── GameServer.Infrastructure/ # インフラ層
    ├── Data/
    │   └── AppDbContext.cs        # EF Core DbContext
    ├── Services/                  # サービス実装
    │   ├── AuthService.cs
    │   └── ScoreService.cs
    ├── Migrations/                # DBマイグレーション
    └── DependencyInjection.cs     # 依存注入設定
```

## セットアップ

### 1. 必要なツールのインストール

```bash
# .NET 9.0 SDK をインストール（未インストールの場合）
# https://dotnet.microsoft.com/download

# Entity Framework Core ツールのインストール
dotnet tool install --global dotnet-ef
```

### 2. データベースの初期化

```bash
# マイグレーションを適用してデータベースを作成
dotnet ef database update --project GameServer.Infrastructure --startup-project GameServer.API
```

### 3. アプリケーションの起動

```bash
# 開発サーバーを起動
dotnet run --project GameServer.API
```

起動後、Swagger UI でAPIをテストできます：
`https://localhost:<port>/swagger`

## API エンドポイント

### 認証（Authentication）

#### ユーザー登録
```
POST /api/auth/register
Content-Type: application/json

{
  "username": "your_username",
  "password": "your_password"
}
```

#### ログイン
```
POST /api/auth/login
Content-Type: application/json

{
  "username": "your_username",
  "password": "your_password"
}

Response:
{
  "token": "eyJhbGci..."
}
```

### スコア（Score）

#### スコア送信（JWT認証必須）
```
POST /api/scores
Authorization: Bearer <token>
Content-Type: application/json

{
  "score": 1000
}
```

#### ランキング取得
日付フィルタリングに対応しています。
```
GET /api/scores/ranking?limit=10&startDate=2025-10-01&endDate=2025-10-31

Query Parameters:
- limit (optional, default: 10, max: 100): 取得件数
- startDate (optional): 開始日時 (ISO 8601形式)
- endDate (optional): 終了日時 (ISO 8601形式)

Response:
[
  {
    "username": "player1",
    "score": 1500,
    "submittedAt": "2025-10-12T12:34:56Z"
  },
  ...
]
```

#### 自分のベストスコア取得（JWT認証必須）
```
GET /api/scores/me/best
Authorization: Bearer <token>

Response:
{
  "score": 1000,
  "submittedAt": "2025-10-12T12:34:56Z"
}
```

## Swagger UI での使い方

1. アプリを起動後、ブラウザで `https://localhost:<port>/swagger` を開く
2. `/api/auth/register` でユーザー登録
3. `/api/auth/login` でログインし、レスポンスからトークンをコピー
4. **右上の「Authorize」ボタン**をクリック
5. トークンを入力（`Bearer` プレフィックスは不要）
6. 「Authorize」をクリックして認証完了
7. 認証が必要なエンドポイント（`/api/scores`, `/api/scores/me/best`）にアクセス可能

## 開発コマンド

```bash
# プロジェクトのビルド
dotnet build GameServer.API/GameServer.API.csproj

# 新しいマイグレーションの作成
dotnet ef migrations add <MigrationName> --project GameServer.Infrastructure --startup-project GameServer.API

# データベースの更新
dotnet ef database update --project GameServer.Infrastructure --startup-project GameServer.API

# データベースの削除
dotnet ef database drop --project GameServer.Infrastructure --startup-project GameServer.API
```

## 設定

### JWT設定

`appsettings.Development.json` で JWT 鍵を設定できます：

```json
{
  "Jwt": {
    "Key": "your-secret-key-must-be-at-least-32-characters"
  }
}
```

**注意**: HS256アルゴリズムは256ビット（32文字）以上の鍵が必要です。

## データベーススキーマ

### Users テーブル
- `Id` (int, PK)
- `Username` (string)
- `PasswordHash` (string) - BCryptでハッシュ化

### Scores テーブル
- `Id` (int, PK)
- `UserId` (int, FK to Users)
- `Value` (int)
- `CreatedAt` (DateTime)

ランキングは `Value` 降順、同点の場合は `CreatedAt` 昇順（早い方が上位）でソートされます。
