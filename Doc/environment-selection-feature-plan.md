# 環境選択機能 実装計画書

## 概要

TatehamaATS_v1アプリケーションに、接続時にサーバー環境を選択可能な機能を追加します。

**重要な設計方針**:
- サーバーURLはコンパイル時定数として別ファイル(`ServerAddress.cs`)に定義
- `ServerAddress.cs`はCD/CIで自動生成、ローカルでは手動作成を想定
- 設定ファイルは使用せず、前回の選択は記憶しない(毎回選択)
- URLが空文字の環境は選択肢に表示しない(ローカル用定義を配布時に無効化可能)

## 現状分析

### 現在の構成

- **サーバーURL**: `ServerAddress.cs`にハードコードされた定数
- **UI フレームワーク**: Windows Forms (.NET 8.0)
- **接続プロトコル**: SignalR over WebSocket + OAuth2認証(OpenIddict)
- **設定管理**: なし(コンパイル時定数のみ)

### 問題点

1. サーバー環境を変更するにはソースコードの編集と再コンパイルが必要
2. Dev環境とProd環境でexeを別にする必要があり、ファイル管理の煩雑さが高い
3. デバッグモードの切り替えもハードコード

## 機能要件

### 必須機能

1. **環境選択ダイアログ**
   - アプリケーション起動時に毎回表示
   - プリセット環境の選択
   - 前回選択の記憶はしない(設定ファイル不使用)

2. **コンパイル時定数(別ファイル管理)**
   - 以下の項目は`ServerAddress.cs`に定義
       - 環境とサーバーURLのペア
       - その環境が認証が必要であるかどうか(デバッグモードのオンオフ)
   - `ServerAddress.cs`はCD/CIで自動生成、ローカルでは手動作成

3. **環境プリセット(コンパイル時定数)**
   - ローカル開発環境 (認証不要・デバッグモード)
   - Devサーバー (認証必要)
   - Prodサーバー (認証必要)

4. **UI表示**
   - 環境名のみを表示(URLは表示しない)
   - URLが空文字の環境は表示しない
   - ユーザーはURLを意識する必要がない

## 技術設計

### 1. サーバーURL定義システム

#### 1.1 サーバーURL定数ファイル(CD/CI自動生成)

**新規ファイル**: `ServerAddress.cs` (リポジトリには含めない、.gitignoreに追加)

```csharp
// このファイルはCD/CIで自動生成されます
// ローカル開発時は手動で作成してください
//
// ローカル開発用テンプレート:
// public static class ServerAddress
// {
//     public const string Version = "v0.00";
//     public const string LocalUrl = "https://localhost:7232";
//     public const string DevelopmentUrl = "";  // Dev環境のアドレスを手元で入れといてデバッグ用に 
//     public const string ProductionUrl = "";   // Prod環境のアドレスを手元で入れといてデバッグ用に 
// }

public static class ServerAddress
{
    // バージョン
    public const string Version = "v0.00";
    // ローカル開発環境
    public const string LocalUrl = ""; // 空文字で無効化

    // Devサーバー (CD/CIで設定)
    public const string DevelopmentUrl = "";  // CD/CIで置き換え

    // Prodサーバー (CD/CIで設定)
    public const string ProductionUrl = "";  // CD/CIで置き換え
}
```

**CD/CI設定例(GitHub Actions)**:
```yaml
- name: Generate ServerAddress.cs
  run: |
    echo "${{ secrets.SERVER_ADDRESS_CS }}" > ServerAddress.cs
```

**GitHub Secretsの設定例**:
- `SERVER_ADDRESS_CS`: Dev環境用のServerAddress.cs全体


#### 1.2 環境定義クラスの作成

**新規ファイル**: `Config/EnvironmentDefinitions.cs`

```csharp
public enum EnvironmentType
{
    Local,
    Development,
    Production
}

public class EnvironmentDefinition
{
    public EnvironmentType Type { get; init; }
    public string DisplayName { get; init; }
    public string ServerUrl { get; init; }
    public bool RequiresAuthentication { get; init; }

    // コンパイル時定数として環境を定義
    // URLはServerAddress.csから取得
    public static readonly EnvironmentDefinition Local = new()
    {
        Type = EnvironmentType.Local,
        DisplayName = "ローカル開発環境",
        ServerUrl = ServerAddress.LocalUrl,
        RequiresAuthentication = false  // デバッグモード
    };

    public static readonly EnvironmentDefinition Development = new()
    {
        Type = EnvironmentType.Development,
        DisplayName = "Devサーバー",
        ServerUrl = ServerAddress.DevelopmentUrl,
        RequiresAuthentication = true
    };

    public static readonly EnvironmentDefinition Production = new()
    {
        Type = EnvironmentType.Production,
        DisplayName = "Prodサーバー",
        ServerUrl = ServerAddress.ProductionUrl,
        RequiresAuthentication = true
    };

    // 全環境のリスト(URLが空でないものだけ)
    public static IReadOnlyList<EnvironmentDefinition> Available =>
        All.Where(e => !string.IsNullOrEmpty(e.ServerUrl)).ToList();

    // すべての環境定義
    private static readonly EnvironmentDefinition[] All = new[]
    {
        Local,
        Development,
        Production
    };

    public static EnvironmentDefinition GetByType(EnvironmentType type)
    {
        return All.First(e => e.Type == type);
    }
}
```

### 2. 環境選択ダイアログ

#### 2.1 UI設計

**新規フォーム**: `MainWindow/EnvironmentSelectForm.cs`

**レイアウト**:
```
┌─────────────────────────────────────────┐
│  環境選択                                │
├─────────────────────────────────────────┤
│                                         │
│  接続先環境を選択してください:            │
│                                         │
│  ○ ローカルサーバー                      │
│                                         │
│  ○ Devサーバー                          │
│                                         │
│  ○ Prodサーバー                          │
│                                         │
│                                         │
│                   [接続]                 │
└─────────────────────────────────────────┘
```

**デザインのポイント**:
- URLは表示しない(ユーザーには見せない)
- 環境名のみを表示
- URLが空文字の環境は表示しない
- シンプルで分かりやすいUI
- デバッグモードはコンパイル時定数で決定されるため、UI上での切り替えは不要
- 設定保存機能なし(毎回選択)

#### 2.2 コンポーネント

- `RadioButton` × 動的生成: 環境選択(URLが空でない環境のみ)
- `Button` × 2: 接続、キャンセル
- `Label` × 2: 説明文、注意書き

#### 2.3 実装例

```csharp
public partial class EnvironmentSelectForm : Form
{
    private EnvironmentType? _selectedEnvironment;
    public EnvironmentType SelectedEnvironment { get; private set; }

    public EnvironmentSelectForm()
    {
        InitializeComponent();

        // URLが空でない環境のみラジオボタンを生成
        int yPosition = 60;
        var availableEnvironments = EnvironmentDefinition.Available;

        if (!availableEnvironments.Any())
        {
            MessageBox.Show(
                "利用可能な環境が定義されていません。\nServerAddress.csを確認してください。",
                "エラー",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            Application.Exit();
            return;
        }

        foreach (var env in availableEnvironments)
        {
            var radioButton = new RadioButton
            {
                Text = env.DisplayName,  // URLではなく環境名のみ表示
                Tag = env.Type,
                Location = new Point(30, yPosition),
                AutoSize = true,
                Checked = (yPosition == 60)  // 最初の環境をデフォルト選択
            };
            radioButton.CheckedChanged += RadioButton_CheckedChanged;
            this.Controls.Add(radioButton);
            yPosition += 35;

            if (radioButton.Checked)
            {
                _selectedEnvironment = env.Type;
            }
        }
    }

    private void RadioButton_CheckedChanged(object sender, EventArgs e)
    {
        if (sender is RadioButton rb && rb.Checked)
        {
            _selectedEnvironment = (EnvironmentType)rb.Tag;
        }
    }

    private void ConnectButton_Click(object sender, EventArgs e)
    {
        if (!_selectedEnvironment.HasValue)
        {
            MessageBox.Show("環境を選択してください。", "エラー",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        SelectedEnvironment = _selectedEnvironment.Value;
        DialogResult = DialogResult.OK;
        Close();
    }
}
```

### 3. アーキテクチャ変更

#### 3.1 ServerAddress.csの変更
1.1を参照のこと

**影響範囲**:
- `Program.cs:66` - OpenIddict設定
- `Network.cs:457` - SignalR接続URL

#### 3.2 起動フローの変更

**変更前のフロー**:
```
Main() → DI設定 → MainWindow表示 → 接続開始
```

**変更後のフロー**:
```
Main() → 環境選択ダイアログ → DI設定 → MainWindow表示 → 接続開始
```

**Program.cs の変更**:

```csharp
[STAThread]
static void Main()
{
    ApplicationConfiguration.Initialize();

    // 1. 環境選択ダイアログ表示
    EnvironmentType selectedEnvironment;
    using (var selectForm = new EnvironmentSelectForm())
    {
        if (selectForm.ShowDialog() != DialogResult.OK)
        {
            return; // キャンセルされた場合は終了
        }

        selectedEnvironment = selectForm.SelectedEnvironment;
    }

    // 2. ServerAddressクラスを初期化
    ServerAddress.Initialize(selectedEnvironment);

    // 3. DI設定(既存のコード)
    var host = Host.CreateDefaultBuilder()
        .ConfigureServices(services =>
        {
            // OpenIddict設定でServerAddress.SignalAddressを使用
            services.AddOpenIddict()
                .AddClient(options =>
                {
                    options.SetIssuer(new Uri(ServerAddress.SignalAddress));
                    // ...
                });
            // ...
        })
        .ConfigureWinFormsLifetime()
        .UseWinFormsLifetime<MainWindow.MainWindow>()
        .Build();

    host.Start();
    Application.Run(Application.OpenForms[0] as Form);
}
```

### 4. データベース接続への影響

**現在の実装**: OAuth2トークンをSQLiteに保存(`trancrew-multiats-client.sqlite3`)

**問題**: サーバーごとに異なる認証トークンが必要

**対策(オプションA採用): サーバーごとに別DBファイル**

環境ごとに異なるDBファイルを使用することで、トークンの混在を防ぎます。

```csharp
// DBInitWorker.cs または Network.cs 内
private string GetDatabaseFileName(EnvironmentType environmentType)
{
    var envName = environmentType.ToString().ToLower();
    return $"trancrew-multiats-{envName}.sqlite3";
}

// 使用例
// ローカル: trancrew-multiats-local.sqlite3
// Dev: trancrew-multiats-development.sqlite3
// Prod: trancrew-multiats-production.sqlite3
```

**メリット**:
- 環境ごとに完全に独立したトークン管理
- 環境切り替え時の再認証が不要(各環境のトークンが保持される)
- トークンクリア処理が不要

**実装箇所**:
- `DBInitWorker.cs`: DB初期化時にファイル名を動的に決定
- `Network.cs`: OpenIddict設定でDBファイル名を使用

### 5. エラーハンドリング

#### 5.1 接続エラー

```csharp
// MainWindow.cs - MainWindow_Load
private async void MainWindow_Load(object sender, EventArgs e)
{
    try
    {
        CableIO.StartRelay();
        await CableIO.NetworkAuthorize();
    }
    catch (HttpRequestException ex)
    {
        var result = MessageBox.Show(
            $"サーバーへの接続に失敗しました。\n\n{ex.Message}\n\n環境設定をやり直しますか?",
            "接続エラー",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Error
        );

        if (result == DialogResult.Yes)
        {
            // 環境選択ダイアログを再表示
            Application.Restart();
        }
        else
        {
            Application.Exit();
        }
    }
}
```

#### 5.2 認証エラー

```csharp
// Network.cs - Authorize
catch (OpenIddictClientException ex)
{
    MessageBox.Show(
        $"認証に失敗しました。\n\nサーバー: {ServerAddress.SignalAddress}\nエラー: {ex.Message}",
        "認証エラー",
        MessageBoxButtons.OK,
        MessageBoxIcon.Error
    );
    throw;
}
```

### 6. テスト戦略

#### 6.1 手動テストケース

| # | テスト項目 | 手順 | 期待結果 |
|---|-----------|------|---------|
| 1 | 初回起動 | アプリ起動 | 環境選択ダイアログが表示される |
| 2 | ローカル環境選択 | ローカル開発環境選択→接続 | localhost:7232に接続、認証スキップ |
| 3 | Devサーバー選択 | Devサーバー選択→接続 | Devサーバーに接続、認証実行 |
| 4 | Prodサーバー選択 | Prodサーバー選択→接続 | Prodサーバーに接続、認証実行 |
| 5 | 設定保存 | チェックON→接続→再起動 | 前回の設定が保持される |
| 6 | 設定非保存 | チェックOFF→接続→再起動 | 環境選択ダイアログが再表示 |
| 7 | キャンセル | キャンセルボタン | アプリが終了する |
| 8 | 接続失敗 | Devサーバー選択→接続 | エラーダイアログ表示、再選択可能 |
| 9 | サーバー切り替え | 環境A→B切り替え | 再認証が実行される |
| 10 | 環境名表示確認 | ダイアログ表示 | URLではなく環境名のみ表示される |

#### 6.2 自動テスト(将来実装)

自動テストは将来の実装として保留します。現在は手動テストで品質を担保します。

**将来実装予定のテスト項目**:
- `EnvironmentDefinition.Available`が空URL環境を除外することの検証
- `EnvironmentDefinition.GetByType()`の動作確認
- 環境選択ダイアログのUI動作テスト
- DB接続の環境別ファイル名生成テスト

## 実装手順

### Phase 1: 基盤実装(優先度: 高)

1. **ServerAddress.csの作成**
   - [ ] CD/CI設定ファイル作成(GitHub Actions等)
   - [ ] シークレット変数の設定(SERVER_ADDRESS_CS)

2. **環境定義システムの作成**
   - [ ] `Config/EnvironmentDefinitions.cs` 作成
   - [ ] `EnvironmentType` enum定義
   - [ ] `EnvironmentDefinition`クラスとコンパイル時定数定義
   - [ ] `Available`プロパティで空URL除外実装

3. **ServerAddress.csの変更**
   - [ ] constをstaticプロパティに変更
   - [ ] `Initialize(EnvironmentType)`メソッド追加
   - [ ] 既存コードの動作確認

### Phase 2: UI実装(優先度: 高)

4. **環境選択ダイアログの作成**
   - [ ] `MainWindow/EnvironmentSelectForm.cs` 作成
   - [ ] `MainWindow/EnvironmentSelectForm.Designer.cs` デザイン
   - [ ] ラジオボタンの動的生成(URLが空でない環境のみ)
   - [ ] 環境名のみ表示、URL非表示
   - [ ] 利用可能環境なしの場合のエラー処理

5. **起動フローの統合**
   - [ ] `Program.cs`の変更(設定ファイル読込削除)
   - [ ] ダイアログ表示タイミング調整
   - [ ] DIコンテナへの環境情報注入

### Phase 3: 認証連携(優先度: 中)

6. **データベース接続管理(オプションA)**
   - [ ] 環境別DBファイル名生成実装
   - [ ] `DBInitWorker.cs`でのDB初期化変更
   - [ ] `Network.cs`でのOpenIddict設定変更

7. **エラーハンドリング**
   - [ ] 接続エラー処理
   - [ ] 認証エラー処理
   - [ ] 再試行ダイアログ実装

### Phase 4: 品質向上(優先度: 低)

8. **テスト実装**
   - [ ] 手動テストケース実行
   - [ ] 空URL環境の非表示確認
   - [ ] CD/CIビルドテスト
   - [ ] バグ修正
   - [ ] ユーザビリティ改善

9. **ドキュメント作成**
   - [ ] ユーザーマニュアル更新
   - [ ] ServerAddress.csセットアップガイド作成
   - [ ] CD/CI設定ドキュメント作成
   - [ ] トラブルシューティングガイド

### Phase 5: オプション機能(優先度: 低)

10. **接続テスト機能**
    - [ ] 接続テストボタン追加
    - [ ] タイムアウト処理
    - [ ] ステータス表示

11. **実行中の切り替え**
    - [ ] メニューバーに環境切り替え追加
    - [ ] 接続の切断/再接続処理
    - [ ] 状態保存

## ファイル構成

### 新規作成ファイル

```
TatehamaATS_v1/
├── ServerAddress.cs                    [新規] サーバーURL定数(CD/CIで自動生成)
├── Config/
│   └── EnvironmentDefinitions.cs    [新規] 環境定義(コンパイル時定数)
├── MainWindow/
│   ├── EnvironmentSelectForm.cs     [新規] 環境選択ダイアログ
│   └── EnvironmentSelectForm.Designer.cs [新規] デザイナーファイル
├── .gitignore                       [変更] ServerAddress.csを追加
└── Doc/
    └── environment-selection-feature-plan.md [本ドキュメント]
```

### 変更が必要なファイル

```
TatehamaATS_v1/
├── Program.cs                       [変更] 起動フロー変更
├── ServerAddress.cs                 [変更] 定数→プロパティ化
├── Network/
│   └── Network.cs                   [変更] 認証トークン管理
├── MainWindow/
│   └── MainWindow.cs                [変更] エラーハンドリング
└── .gitignore                       [変更] ServerAddress.csを追加
```

### .gitignoreへの追加

```
# サーバーURL定数ファイル(CD/CIで自動生成)
ServerAddress.cs
```

### ServerAddress.csテンプレート

ローカル開発時に手動作成する`ServerAddress.cs`のテンプレート:

```csharp
public static class ServerAddress
{
    // バージョン
    public const string Version = "v0.00";

    // ローカル開発環境のみ有効化
    public const string LocalUrl = "https://localhost:7232";

    // 開発・本番は空文字で無効化(配布時はここを編集しない)
    public const string DevelopmentUrl = "";
    public const string ProductionUrl = "";
}
```

### 環境定義(コンパイル時定数)

以下の情報はコードに定義され、ユーザーには表示されません:

| 環境タイプ | 表示名 | サーバーURL | 認証要否 |
|-----------|-------|-----------|---------|
| `Local` | ローカル開発環境 | `ServerAddress.LocalUrl` | 不要 (デバッグモード) |
| `Development` | Devサーバー | `ServerAddress.DevelopmentUrl` (CD/CIで設定) | 必要 |
| `Production` | Prodサーバー | `ServerAddress.ProductionUrl` (CD/CIで設定) | 必要 |

**注意**: 実際のURLはドキュメント上でマスクし、CD/CIのシークレット変数で管理します。

## セキュリティ考慮事項

### 1. URL保護

- サーバーURLはコンパイル時定数として定義
- ユーザーにはURLを表示しない
- カスタムURL入力機能は提供しない(セキュリティリスク回避)

### 2. 認証トークン

- サーバーごとに別トークン管理
- トークンファイルの権限設定(読取専用)

### 3. エラー情報

- 詳細なエラーメッセージはデバッグモードのみ
- 本番環境では最小限の情報のみ表示

## 今後の拡張性

### 1. 追加環境の定義

- `ServerAddress.cs`に新しい環境を追加
- `EnvironmentType` enumに追加
- `EnvironmentDefinitions.cs`で定義を追加

### 2. サーバー情報の自動取得

- サーバーからバージョン情報を取得
- 互換性チェック
- アップデート通知

### 3. 高度な設定

- タイムアウト値の設定
- リトライ回数の設定
- ログレベルの設定

### 4. コマンドライン引数での環境指定

```bash
# 環境を指定して起動(ダイアログをスキップ)
TatehamaATS.exe --env Local
TatehamaATS.exe --env Development
TatehamaATS.exe --env Production
```

## 見積もり

### 開発工数

| フェーズ | 工数(時間) | 備考 |
|---------|-----------|------|
| Phase 1 | 3-4時間 | ServerAddress.cs + 環境定義実装 |
| Phase 2 | 4-5時間 | UI実装(シンプル化、設定ファイル不要) |
| Phase 3 | 4-6時間 | 認証連携 |
| Phase 4 | 3-5時間 | テスト/修正/CD設定 |
| Phase 5 | 6-8時間 | オプション機能 |
| **合計** | **20-28時間** | 約2.5-3.5日 |

### リスク

| リスク | 影響度 | 対策 |
|-------|-------|------|
| OpenIddict設定の複雑性 | 高 | 事前検証、段階的実装 |
| 既存認証フローへの影響 | 高 | 徹底的なテスト |
| ServerAddress.cs未作成 | 中 | READMEに明記、ビルドエラーで気づける |
| CD/CIシークレット設定漏れ | 中 | ビルドパイプラインでの検証 |
| UIレイアウト崩れ | 低 | レスポンシブデザイン |

## 承認・レビュー

### レビューポイント

- [ ] アーキテクチャ設計の妥当性
- [ ] セキュリティ要件の充足
- [ ] ユーザビリティの確認
- [ ] 既存機能への影響範囲
- [ ] テスト計画の妥当性

### 承認者

- プロジェクトリーダー: _______________
- 技術責任者: _______________
- 承認日: _______________

## 参考資料

- [OpenIddict Documentation](https://documentation.openiddict.com/)
- [SignalR Client Configuration](https://learn.microsoft.com/en-us/aspnet/core/signalr/configuration)
- [.NET Configuration](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration)
- [Windows Forms Best Practices](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/)

---

**ドキュメントバージョン**: 1.0
**作成日**: 2025-11-11
**最終更新日**: 2025-11-11
**作成者**: Claude Code
**ステータス**: 提案中
