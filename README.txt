# Unity Game API Server

ASP.NET Core Web API で作る、学習用のゲームサーバーです。  
認証（JWT）＋ランキング（スコア送信・取得）をローカルで試すことを目的としています。

## 構成

- **Controllers/**
  - `AuthController` : ユーザー登録・ログイン（JWT）
  - `ScoreController` : スコア送信・ランキング取得
- **Data/** : EF Core DbContext
- **Models/** : User, Score モデル
- **Migrations/** : DBマイグレーション
- **Program.cs** : アプリ起動・サービス登録


## 技術スタック
- .NET 8 / ASP.NET Core Web API
- Entity Framework Core + SQLite
- JWT 認証
- Unity WebRequest からのアクセス想定
