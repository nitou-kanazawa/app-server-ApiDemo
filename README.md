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
GameServer/
├── Controllers/
│   ├── AuthController.cs      # ユーザー登録・ログイン（JWT）
│   └── ScoreController.cs     # スコア送信・ランキング取得
├── Models/
│   ├── AppDbContext.cs        # EF Core DbContext
│   ├── User.cs                # ユーザーモデル
│   └── Score.cs               # スコアモデル
├── Migrations/                # DBマイグレーション
├── Program.cs                 # アプリ起動・サービス登録
└── appsettings.Development.json  # 開発環境設定（JWT鍵）
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
dotnet ef database update --project GameServer
```

### 3. アプリケーションの起動

```bash
# 開発サーバーを起動
dotnet run --project GameServer
```

起動後、Swagger UI でAPIをテストできます：
`https://localhost:<port>/swagger`

## API エンドポイント

### 認証（Authentication）

#### ユーザー登録
```
POST /Auth/api/register
Content-Type: application/json

{
  "username": "your_username",
  "password": "your_password"
}
```

#### ログイン
```
POST /Auth/api/login
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
POST /Score/api/submit
Authorization: Bearer <token>
Content-Type: application/json

{
  "score": 1000
}
```

#### ランキング取得
```
GET /Score/api/ranking?limit=10

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
GET /Score/api/mybest
Authorization: Bearer <token>

Response:
{
  "score": 1000,
  "submittedAt": "2025-10-12T12:34:56Z"
}
```

## Swagger UI での使い方

1. アプリを起動後、ブラウザで `https://localhost:<port>/swagger` を開く
2. `/Auth/api/register` でユーザー登録
3. `/Auth/api/login` でログインし、レスポンスからトークンをコピー
4. **右上の「Authorize」ボタン**をクリック
5. トークンを入力（`Bearer` プレフィックスは不要）
6. 「Authorize」をクリックして認証完了
7. 認証が必要なエンドポイント（`/Score/api/submit`, `/Score/api/mybest`）にアクセス可能

## 開発コマンド

```bash
# プロジェクトのビルド
dotnet build GameServer/GameServer.csproj

# 新しいマイグレーションの作成
dotnet ef migrations add <MigrationName> --project GameServer

# データベースの更新
dotnet ef database update --project GameServer

# データベースの削除
dotnet ef database drop --project GameServer
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
