namespace TatehamaATS_v1.Config;

public enum EnvironmentType
{
    Local,
    Development,
    Production
}

public class EnvironmentDefinition
{
    public EnvironmentType Type { get; init; }
    public string DisplayName { get; init; } = "";
    public string ServerUrl { get; init; } = "";
    public bool RequiresAuthentication { get; init; }

    // コンパイル時定数として環境を定義
    // URLはServerAddress.csから取得
    public static readonly EnvironmentDefinition Local = new()
    {
        Type = EnvironmentType.Local,
        DisplayName = "ローカル",
        ServerUrl = ServerAddress.LocalUrl,
        RequiresAuthentication = false  // デバッグモード(=認証なし)
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

    // 選択された環境でServerAddressを初期化
    public static void Initialize(EnvironmentType environmentType)
    {
        var environment = GetByType(environmentType);
        ServerAddress.SignalAddress = environment.ServerUrl;
        ServerAddress.IsDebug = !environment.RequiresAuthentication;
    }
}
