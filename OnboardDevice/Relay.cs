using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using TatehamaATS_v1.Exceptions;
using System.Xml.Linq;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using TrainCrewAPI;
using TrainCrew;
using Microsoft.AspNetCore.Routing;
using System.ComponentModel;
using System.Configuration;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace TatehamaATS_v1.OnboardDevice
{
    enum ConnectionState
    {
        /// <summary>
        /// 切断
        /// </summary>
        DisConnect,

        /// <summary>
        /// 接続中
        /// </summary>
        Connecting,

        /// <summary>
        /// 接続完了
        /// </summary>
        Connected
    }

    /// <summary>
    /// <strong>継電部</strong>
    /// TCとのWS通信を担当
    /// </summary>
    internal partial class Relay
    {
        // 正規表現
        [System.Text.RegularExpressions.GeneratedRegex(@"[ST]([A-Z])$")]
        private static partial System.Text.RegularExpressions.Regex NormalizeRouteRegex();

        // WebSocket関連のフィールド
        private ClientWebSocket _webSocket = new ClientWebSocket();
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private static readonly Encoding _encoding = Encoding.UTF8;
        private readonly string _connectUri = "ws://127.0.0.1:50300/"; //TRAIN CREWのポート番号は50300

        // キャッシュ用の静的辞書
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache =
            new ConcurrentDictionary<Type, PropertyInfo[]>();

        private static readonly ConcurrentDictionary<Type, FieldInfo[]> FieldCache =
            new ConcurrentDictionary<Type, FieldInfo[]>();

        // JSONシリアライザ設定
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        // データ関連フィールド
        private string _command = "DataRequest";
        private string[] _request = { "tconlyontrain", "interlock", "signal" };

        // プロパティ
        public TrainCrewStateData TcData { get; private set; } = new TrainCrewStateData();
        public RecvBeaconStateData BeaconData { get; private set; } = new RecvBeaconStateData();

        private int brake;
        private ConnectionState status = ConnectionState.DisConnect;
        private int BeforeBrake = 0;

        private List<Route> ServerRoutes = new List<Route>();
        private List<Route> TrainCrewRoutes = new List<Route>();
        private int RouteCounta = 0;
        internal static int shiftTime = 0;
        private HashSet<string> NextSignalNameSet = [];

        // SignalSet/UpdateRoute用の同時実行防止ロック
        private readonly SemaphoreSlim _routeSignalLock = new(1, 1);

        /// <summary>
        /// SetSignalPhases 差分送信用キャッシュ。
        /// キー: 信号機名、値: 直近で TrainCrew に送信した現示。
        /// 差分検出のため、SetSignalPhases 送信完了時に更新する。
        /// </summary>
        private readonly Dictionary<string, Phase> _lastSentSignalPhases = new();

        /// <summary>
        /// 次回の SetSignalPhases で「全信号を再送」するためのフラグ。
        /// true の場合、差分判定をスキップしてキャッシュを破棄→全件送信する。
        /// 初期値は true（ソフト起動直後は全信号送信が必要なため）。
        /// シナリオ読み込み完了時 / ForceStopSignal 経由でも true に戻される。
        /// </summary>
        private bool _resendAllSignals = true;

        /// <summary>
        /// 定期キャッシュクリアの間隔 (ms)。1秒ごとに全信号再送をトリガーする。
        /// </summary>
        private const int CacheClearIntervalMs = 1000;

        /// <summary>
        /// <see cref="CacheClearIntervalMs"/> ごとに <see cref="InvalidateSignalPhaseCache"/> を呼び出すタイマー。
        /// </summary>
        private readonly System.Threading.Timer _cacheClearTimer;

        internal StopPassManager StopPassManager;

        /// <summary>
        /// 運転会列番
        /// </summary>
        internal string OverrideDiaName { get; set; }

        // イベント
        internal event Action<TimeSpan> TC_TimeUpdated;
        internal event Action<ConnectionState> ConnectionStatusChanged;
        internal event Action<TrainCrewStateData> TrainCrewDataUpdated;
        internal event Action<RecvBeaconStateData> BeaconChenged;

        /// <summary>
        /// 故障発生
        /// </summary>
        internal event Action<ATSCommonException> AddExceptionAction;

        private int hasInvalidCharsTimes = 0;

        /// <summary>
        /// TrainCrew側データ要求コマンド
        /// (DataRequest, SetEmergencyLight, SetSignalPhase, SetSignalPhases)
        /// </summary>
        public string Command
        {
            get => _command;
            set
            {
                if (value == null)
                {
                    var e = new RelayException(5, "無効なコマンドです。null");
                    AddExceptionAction.Invoke(e);
                }

                if (IsValidCommand(value))
                {
                    _command = value;
                }
                else
                {
                    var e = new RelayException(5, "無効なコマンドです。");
                    AddExceptionAction.Invoke(e);
                }
            }
        }

        /// <summary>
        /// TrainCrew側データ要求引数
        /// (all, tc, tconlyontrain, tcall, signal, train)
        /// </summary>
        public string[] Request
        {
            get => _request;
            set
            {
                if (value == null)
                {
                    var e = new RelayException(5, "無効なコマンドです。");
                    AddExceptionAction.Invoke(e);
                }

                if (IsValidRequest(_command, value))
                {
                    _request = value;
                }
                else
                {
                    var e = new RelayException(5, "無効な要求です。");
                    AddExceptionAction.Invoke(e);
                }
            }
        }

        /// <summary>
        /// コマンドの検証
        /// </summary>
        /// <param name="requestValues"></param>
        /// <returns></returns>
        private static readonly HashSet<string> ValidSignalPhases = new()
        {
            "None", "R", "YY", "Y", "YG", "G"
        };

        private static bool IsValidCommand(string requestValues) =>
            new[]
            {
                "DataRequest", "SetEmergencyLight", "SetSignalPhase", "SetSignalPhases", "mode_req", "SetRoute",
                "DeleteRoute", "DeleteRoute2", "realtimeoffset"
            }.Contains(requestValues);

        /// <summary>
        /// リクエストの検証
        /// </summary>
        /// <param name="commandValue"></param>
        /// <param name="requestValues"></param>
        /// <returns></returns>
        private static bool IsValidRequest(string commandValue, string[] requestValues)
        {
            switch (commandValue)
            {
                case "DataRequest":
                    return requestValues.Length == 1 && requestValues[0] == "all" ||
                           requestValues.All(str =>
                               str == "tc" || str == "tconlyontrain" || str == "tcall" || str == "signal" ||
                               str == "train");
                case "SetEmergencyLight":
                    return requestValues.Length == 2 && (requestValues[1] == "true" || requestValues[1] == "false");
                case "SetSignalPhase":
                    return requestValues.Length == 2 && ValidSignalPhases.Contains(requestValues[1]);
                case "SetSignalPhases":
                    if (requestValues.Length < 2 || requestValues.Length % 2 != 0)
                    {
                        return false;
                    }

                    for (int i = 0; i < requestValues.Length; i += 2)
                    {
                        if (string.IsNullOrEmpty(requestValues[i])) return false;
                        if (!ValidSignalPhases.Contains(requestValues[i + 1])) return false;
                    }

                    return true;
                case "mode_req":
                    return requestValues.Length == 1 && (requestValues[0] == "hide_other" ||
                                                         requestValues[0] == "show_other" ||
                                                         requestValues[0] == "route_manual" ||
                                                         requestValues[0] == "route_auto" ||
                                                         requestValues[0] == "realtimemode_on");
                case "SetRoute":
                    return requestValues.Length == 5;
                case "DeleteRoute":
                case "DeleteRoute2":
                    return requestValues.Length == 2;
                case "realtimeoffset":
                    return requestValues.Length == 1;
                default:
                    return false;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Relay()
        {
            OverrideDiaName = "9999";
            TrainCrewInput.Init();
            _webSocket = new ClientWebSocket();

            _cacheClearTimer = new System.Threading.Timer(
                _ => InvalidateSignalPhaseCache(),
                null,
                CacheClearIntervalMs,
                CacheClearIntervalMs);
        }

        /// <summary>
        /// 受信データ処理メソッド
        /// </summary>
        private void ProcessingReceiveData()
        {
            TrainCrewDataUpdated.Invoke(TcData);
            if (TcData.gameScreen == TrainCrewAPI.GameScreen.Menu)
            {
                TrainCrewInput.RequestData(DataRequest.Signal);
                SetRouteMode(true);
                SetOther(true);
                SetTimeMode();
            }

            // TrainCrewRoutesにTcData.interlockDataListから展開した進路情報を格納する
            TrainCrewRoutes = ConvertToRoutes(TcData.interlockDataList);
        }

        /// <summary>
        /// InterlockDataのリストをRouteのリストに変換します。
        /// </summary>
        /// <param name="interlockDataList">InterlockDataのリスト</param>
        /// <returns>Routeのリスト</returns>
        public List<Route> ConvertToRoutes(List<InterlockData> interlockDataList)
        {
            var routes = new List<Route>();

            foreach (var interlockData in interlockDataList.ToList())
            {
                foreach (var interlockRoute in interlockData.routes)
                {
                    // StaNameByIdを逆向きに使用して、interlockData.Nameを駅IDにする
                    // interlockData.Nameには"連動装置"が末尾に含まれているため、削除してから検索する
                    var stationId = StopPassManager.GetStationIdByName(interlockData.Name.Replace("連動装置", ""));
                    var route = new Route
                    {
                        TcName = $"{stationId}_{interlockRoute.Name}",
                        RouteType = RouteType.SwitchRoute,
                        Indicator = ""
                    };

                    routes.Add(route);
                }
            }

            return routes;
        }

        /// <summary>
        /// WebSocket接続試行
        /// </summary>
        /// <returns></returns>
        internal async Task TryConnectWebSocket()
        {
            status = ConnectionState.DisConnect;
            ConnectionStatusChanged?.Invoke(status);
            while (true)
            {
                _webSocket = new ClientWebSocket();

                try
                {
                    status = ConnectionState.Connecting;
                    ConnectionStatusChanged?.Invoke(status);
                    // 接続処理
                    await SendAndReceiveDataRequest();
                    break;
                }
                catch (ATSCommonException ex)
                {
                    AddExceptionAction.Invoke(ex);
                }
                catch (Exception ex)
                {
                    status = ConnectionState.DisConnect;
                    ConnectionStatusChanged?.Invoke(status);
                    var e = new RelayConnectException(5, "", ex);
                    AddExceptionAction.Invoke(e);
                    await Task.Delay(1000);
                }
            }
        }

        /// <summary>
        /// Websocketの接続処理
        /// </summary>
        /// <returns></returns>
        private async Task ConnectWebSocketAsync()
        {
            const int maxRetry = 5;
            if (_webSocket.State == WebSocketState.Open)
            {
                return;
            }

            for (var i = 1; i <= maxRetry; i++)
            {
                try
                {
                    await _webSocket.ConnectAsync(new(_connectUri), CancellationToken.None);
                }
                catch (InvalidOperationException)
                {
                    if (i == maxRetry)
                    {
                        throw;
                    }

                    _webSocket.Dispose();
                    _webSocket = new();
                    continue;
                }

                status = ConnectionState.Connected;
                ConnectionStatusChanged?.Invoke(status);
                break;
            }
        }

        /// <summary>
        /// TraincrewにDataRequestの送信を行い、データの受信をする。
        /// </summary>
        /// <returns></returns>
        private async Task SendAndReceiveDataRequest()
        {
            // 送信処理
            await SendMessages();
            // 受信処理
            await ReceiveMessages();
        }

        /// <summary>
        /// WebSocket送信処理
        /// </summary>
        private async Task SendMessages()
        {
            try
            {
                await ConnectWebSocketAsync();
                CommandToTrainCrew requestCommand = new CommandToTrainCrew()
                {
                    command = _command,
                    args = _request
                };

                // JSON形式にシリアライズ
                string json = JsonConvert.SerializeObject(requestCommand, JsonSerializerSettings);
                byte[] bytes = _encoding.GetBytes(json);

                // WebSocket送信
                await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                status = ConnectionState.DisConnect;
                ConnectionStatusChanged?.Invoke(status);
                throw new RelayFirstConnectException(5, "50300弾かれ", ex);
            }
        }

        private async Task SendMessages(string command, string[] request)
        {
            try
            {
                await ConnectWebSocketAsync();
                CommandToTrainCrew requestCommand = new CommandToTrainCrew()
                {
                    command = command,
                    args = request
                };

                //Debug.WriteLine(requestCommand);

                // JSON形式にシリアライズ
                string json = JsonConvert.SerializeObject(requestCommand, JsonSerializerSettings);
                byte[] bytes = _encoding.GetBytes(json);

                // WebSocket送信
                await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                status = ConnectionState.DisConnect;
                ConnectionStatusChanged?.Invoke(status);
                throw new RelayConnectException(5, "指示コマンド送信失敗", ex);
            }
        }

        internal async Task SignalSet(List<SignalData> signalDatas)
        {
            if (TcData.gameScreen is not (TrainCrewAPI.GameScreen.MainGame or TrainCrewAPI.GameScreen.MainGame_Pause))
            {
                return;
            }

            if (status != ConnectionState.Connected)
            {
                return;
            }

            await _routeSignalLock.WaitAsync();
            try
            {
                await SignalSetCore(signalDatas);
            }
            finally
            {
                _routeSignalLock.Release();
            }
        }

        private async Task SignalSetCore(List<SignalData>? signalDatas)
        {
            var toSend = (signalDatas ?? [])
                .Select(s => (s.Name, s.phase))
                .ToList();
            if (toSend.Count == 0)
            {
                return;
            }
            await SetSignalPhases(toSend);
        }

        private static Phase NormalizeSignalPhase(Phase phase) =>
            phase == Phase.R ? Phase.R : Phase.None;

        /// <summary>
        /// 複数の信号機の現示をまとめて TrainCrew に送信する。
        /// 起動直後 / シナリオ読み込み直後（<see cref="_resendAllSignals"/> が true）は全信号を送信し、
        /// それ以降は前回送信値（<see cref="_lastSentSignalPhases"/>）との差分のみ送信して帯域を節約する。
        /// </summary>
        /// <param name="items">送信候補となる (信号機名, 現示) のリスト</param>
        internal async Task SetSignalPhases(IReadOnlyList<(string Name, Phase Phase)> items)
        {
            // 候補なしなら何もしない
            if (items == null || items.Count == 0)
            {
                return;
            }

            // ゲーム本編（プレイ中 / 一時停止中）以外では送信しない
            if (TcData.gameScreen is not (TrainCrewAPI.GameScreen.MainGame or TrainCrewAPI.GameScreen.MainGame_Pause))
            {
                return;
            }

            // TrainCrew との WebSocket が未接続なら送信しない
            if (status != ConnectionState.Connected)
            {
                return;
            }

            // 「上り1閉塞」は送信対象外のため、差分判定の前に除外しておく
            // （キャッシュにも入れない＝以降の差分計算からも常に除外）
            var filtered = items.Where(x => x.Name != "上り1閉塞").ToList();
            if (filtered.Count == 0)
            {
                return;
            }

            // 全送信モードではキャッシュを破棄し、以降の Where 条件を素通りさせて全件送信する
            if (_resendAllSignals)
            {
                _lastSentSignalPhases.Clear();
            }

            // 実際に WebSocket で送る信号:
            //   全送信モード → キャッシュ空なので全件
            //   差分送信モード → キャッシュに無い／前回値と異なるものだけ
            var toSend = filtered
                .Where(item => !_lastSentSignalPhases.TryGetValue(item.Name, out var prev) || prev != item.Phase)
                .ToList();

            // 差分0件なら WebSocket 送信自体をスキップ
            if (toSend.Count == 0)
            {
                return;
            }

            // SetSignalPhases コマンドの args 配列を構築
            // 形式: [信号名1, 現示1, 信号名2, 現示2, ...]
            // 同時にキャッシュ（_lastSentSignalPhases）を今回の送信値で更新する
            var args = new string[toSend.Count * 2];
            for (int i = 0; i < toSend.Count; i++)
            {
                args[i * 2] = toSend[i].Name;
                args[i * 2 + 1] = toSend[i].Phase.ToString();
                _lastSentSignalPhases[toSend[i].Name] = toSend[i].Phase;
                // Debug.WriteLine($"☆信号名：{toSend[i].Name}／現示：{toSend[i].Phase}");
            }

            // 全送信モードはここまで来たら役目を終えるので解除する
            _resendAllSignals = false;

            await SendSingleCommand("SetSignalPhases", args);
        }

        /// <summary>
        /// 信号現示の送信履歴キャッシュを無効化し、次回の <see cref="SetSignalPhases"/> 呼び出しで
        /// 全信号を強制的に再送させる。
        /// 呼び出し元:
        ///   - シナリオ読み込み完了時（<see cref="CableIO"/> 側で GameScreen 遷移を検知して呼ぶ）
        ///   - <see cref="ForceStopSignal"/>（強制停止で TrainCrew 側現示を上書きするため、解除後に整合性を取り直す）
        /// </summary>
        internal void InvalidateSignalPhaseCache()
        {
            // 実キャッシュのクリアは次回 SetSignalPhases 内で行う
            // （送信前にクリアして競合状態を作らないよう、フラグだけ立てる）
            _resendAllSignals = true;
        }

        internal void EMSet(List<EmergencyLightData> emergencyLightDatas)
        {
            foreach (var emergencyLightData in emergencyLightDatas.ToList())
            {
                SendSingleCommand("SetEmergencyLight",
                    new string[] { emergencyLightData.Name, emergencyLightData.State ? "true" : "false" });
            }
        }

        internal void SetNextSignalNames(List<string>? nextSignalNames)
        {
            if (nextSignalNames is not { Count: > 0 })
            {
                return;
            }

            NextSignalNameSet = nextSignalNames.ToHashSet();
        }

        internal void SetOther(bool isHide)
        {
            SendSingleCommand("mode_req", new string[] { isHide ? "hide_other" : "show_other" });
        }

        internal void SetRouteMode(bool isManual)
        {
            SendSingleCommand("mode_req", new string[] { isManual ? "route_manual" : "route_auto" });
        }

        internal void SetTimeMode()
        {
            SendSingleCommand("mode_req", new string[] { "realtimemode_on" });
        }

        internal async Task UpdateRoute(List<Route> routes)
        {
            if (!(TcData.gameScreen == TrainCrewAPI.GameScreen.MainGame ||
                  TcData.gameScreen == TrainCrewAPI.GameScreen.MainGame_Pause))
            {
                return;
            }

            if (routes == null)
            {
                //Debug.WriteLine("routes is null. Skipping UpdateRoute.");
                return;
            }

            if (TrainCrewRoutes == null)
            {
                //Debug.WriteLine("TrainCrewRoutes is null. Initializing empty list.");
                TrainCrewRoutes = new List<Route>();
            }

            await _routeSignalLock.WaitAsync();
            try
            {
                await UpdateRouteCore(routes);
            }
            finally
            {
                _routeSignalLock.Release();
            }
        }

        private async Task UpdateRouteCore(List<Route> routes)
        {
            // デバッグ出力
            //Debug.WriteLine("TrainCrewRoutes:");
            //foreach (var route in TrainCrewRoutes.ToList())
            //{
            //    Debug.WriteLine($"  TcName: {route.TcName}");
            //}

            // 差分計算時もToList()でスナップショット
            var currentRoutes = TrainCrewRoutes.ToList();
            var newRoutes = routes.ToList();

            // TrainCrewRoutesをDictionaryに変換
            var currentRoutesByTcName = currentRoutes.ToDictionary(r => r.TcName);

            // 新しいルートを正規化名でDictionaryに変換
            var newRoutesByNormalizeName = newRoutes
                .ToDictionary(
                    r => NormalizeRouteRegex().Replace(r.TcName, "$1"),
                    r => r
                );

            // 追加されたルート: newRoutesの正規化名がcurrentRoutesに存在しないもの)
            var addedRoutes = newRoutes
                .Where(r => !currentRoutesByTcName.ContainsKey(NormalizeRouteRegex().Replace(r.TcName, "$1")))
                .ToList();

            // 削除されたルート: currentRoutesの名前がnewRoutesの正規化名に存在しないもの
            var removedRoutes = currentRoutes
                .Where(r => !newRoutesByNormalizeName.ContainsKey(r.TcName))
                .ToList();

            foreach (var route in addedRoutes.ToList())
            {
                await SetRoute(route);
            }

            foreach (var route in removedRoutes.ToList())
            {
                await DeleteRoute(route);
            }
        }

        private async Task SetRoute(Route route)
        {
            try
            {
                if (!(TcData.gameScreen.HasFlag(TrainCrewAPI.GameScreen.MainGame) ||
                      TcData.gameScreen.HasFlag(TrainCrewAPI.GameScreen.MainGame_Pause)))
                {
                    return;
                }

                if (status != ConnectionState.Connected)
                {
                    return;
                }

                var r = route.TcName.Split('_').ToList();
                // staID仮対応          
                var staName = StopPassManager.GetStationNameById(r[0]) + "連動装置";

                // 末尾が "S[A-Z]" または "T[A-Z]" の場合に "[A-Z]" の部分だけを残す
                var routeName = System.Text.RegularExpressions.Regex.Replace(r[1], @"[ST]([A-Z])$", "$1");

                // Todo: 出発の場合は、列選表示とする。
                string indicator;
                switch (route.RouteType)
                {
                    case RouteType.Arriving:
                    case RouteType.SwitchSignal:
                    case RouteType.SwitchRoute:
                    case RouteType.Guide:
                        indicator = route.Indicator;
                        break;
                    case RouteType.Departure:
                        indicator = StopPassManager.TypeNameTC;
                        break;
                    default:
                        indicator = "";
                        break;
                }
                Debug.WriteLine($"☆API送信: SetRoute/{route.TcName}/{StopPassManager.GetStopDataById(r[0])}/{indicator}");
                SendSingleCommand("SetRoute", [staName, routeName, indicator, TcData.myTrainData.diaName, StopPassManager.GetStopDataById(r[0])]);
            }
            catch (Exception ex)
            {
                //Debug.WriteLine($"{ex.Message}{ex.InnerException}");
            }
        }

        private async Task DeleteRoute(Route route)
        {
            try
            {
                if (!(TcData.gameScreen.HasFlag(TrainCrewAPI.GameScreen.MainGame) ||
                      TcData.gameScreen.HasFlag(TrainCrewAPI.GameScreen.MainGame_Pause)))
                {
                    return;
                }

                if (status != ConnectionState.Connected)
                {
                    return;
                }

                var r = route.TcName.Split('_').ToList();
                // staID仮対応          
                var staName = StopPassManager.GetStationNameById(r[0]) + "連動装置";

                // 末尾が "S[A-Z]" または "T[A-Z]" の場合に "[A-Z]" の部分だけを残す
                var routeName = System.Text.RegularExpressions.Regex.Replace(r[1], @"[ST]([A-Z])$", "$1");

                //Debug.WriteLine($"☆API送信: DeleteRoute2/{route.TcName}");
                await SendSingleCommand("DeleteRoute2", [staName, routeName]);
            }
            catch (Exception ex)
            {
                //Debug.WriteLine($"{ex.Message}{ex.InnerException}");
            }
        }

        internal async Task SetTime(int shiftTime)
        {
            Relay.shiftTime = shiftTime;
        }



        internal async Task SendSingleCommand(string command, string[] request)
        {
            try
            {
                // コマンドとリクエストを検証
                if (IsValidCommand(command) && IsValidRequest(command, request))
                {
                    await SendMessages(command, request);
                }
                else
                {
                    throw new RelayException(5, $"無効なコマンド({command})または要求{string.Join(",", request)}です。");
                }
            }
            catch (ATSCommonException ex)
            {
                AddExceptionAction.Invoke(ex);
            }
            catch (Exception ex)
            {
                status = ConnectionState.DisConnect;
                ConnectionStatusChanged?.Invoke(status);
                var e = new RelayConnectException(5, "指示コマンド送信失敗", ex);
                AddExceptionAction.Invoke(e);
                await Task.Delay(1000);
            }
        }

        /// <summary>
        /// WebSocket受信処理
        /// </summary>
        /// <returns></returns>
        private async Task ReceiveMessages()
        {
            TrainCrewInput.GetTrainState();
            var buffer = new byte[2048];
            var messageBuilder = new StringBuilder();

            while (_webSocket.State == WebSocketState.Open)
            {
                List<byte> messageBytes = new List<byte>();
                _stopwatch.Restart();
                WebSocketReceiveResult result;
                do
                {
                    result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        // サーバーからの切断要求を受けた場合
                        await CloseAsync();
                        status = ConnectionState.DisConnect;
                        ConnectionStatusChanged?.Invoke(status);
                        await TryConnectWebSocket();
                        return;
                    }
                    else
                    {
                        messageBytes.AddRange(buffer.Take(result.Count));
                    }
                } while (!result.EndOfMessage);

                // データが揃ったら文字列へエンコード
                string jsonResponse = _encoding.GetString(messageBytes.ToArray());
                messageBytes.Clear();

                // 文字化けチェック
                if (HasInvalidChars(jsonResponse))
                {
                    Debug.WriteLine("☆文字化け元データ");
                    Debug.WriteLine(jsonResponse);
                    hasInvalidCharsTimes++;
                    if (hasInvalidCharsTimes > 20)
                    {
                        var e = new RelayOtherInfoAbnormal(5, $"文字化け検知${hasInvalidCharsTimes}回目");
                        AddExceptionAction.Invoke(e);
                    }

                    continue;
                }
                else
                {
                    hasInvalidCharsTimes = 0;
                }

                // 一旦Data_Base型でデシリアライズ
                var baseData = JsonConvert.DeserializeObject<Data_Base>(jsonResponse, JsonSerializerSettings);

                if (baseData != null)
                {
                    // Typeプロパティに応じて処理
                    if (baseData.type == "TrainCrewStateData")
                    {
                        // Debug.WriteLine(baseData.data);
                        // Data_Base.DataをTrainCrewStateData型にデシリアライズ
                        var _trainCrewStateData =
                            JsonConvert.DeserializeObject<TrainCrewStateData>(baseData.data.ToString());

                        if (_trainCrewStateData != null)
                        {
                            // JSON受信データ処理
                            lock (TcData)
                            {
                                UpdateFieldsAndProperties(TcData, _trainCrewStateData);
                                // Form関連処理
                                TC_TimeUpdated?.Invoke(TcData.nowTime.ToTimeSpan());
                            }

                            // その他処理
                            ProcessingReceiveData();
                        }
                        else
                        {
                            var e = new RelayCarInfoAbnormal(5, "TcData作成失敗");
                            AddExceptionAction.Invoke(e);
                        }
                    }
                    else if (baseData.type == "RecvBeaconStateData")
                    {
                        // Data_Base.DataをRecvBeaconStateData型にデシリアライズ
                        var _recvBeaconStateData =
                            JsonConvert.DeserializeObject<RecvBeaconStateData>(baseData.data.ToString());

                        if (_recvBeaconStateData != null)
                        {
                            // JSON受信データ処理
                            lock (BeaconData)
                            {
                                UpdateFieldsAndProperties(BeaconData, _recvBeaconStateData);
                                // Form関連処理
                                BeaconChenged?.Invoke(BeaconData);
                            }
                        }
                        else
                        {
                            var e = new TransponderInfoAbnormal(5, "BeaconData作成失敗");
                            AddExceptionAction.Invoke(e);
                        }
                    }
                    else if (baseData.type == "APIMessage")
                    {
                        // Data_Base.DataをAPIMessage型にデシリアライズ
                        var _APIMessage = JsonConvert.DeserializeObject<APIMessage>(baseData.data.ToString());
                        //Debug.WriteLine($"☆API応答：{_APIMessage.title}：{_APIMessage.message}");
                    }
                    else
                    {
                        var e = new RelayOtherInfoAbnormal(5, "不明タイプ");
                        AddExceptionAction.Invoke(e);
                    }
                }

                if (TcData.nowTime.hour != shiftTime)
                {
                    try
                    {
                        Debug.WriteLine($"☆API送信: realtimeoffset/{shiftTime}");
                        SendSingleCommand("realtimeoffset", [$"{shiftTime}"]);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"{ex.Message}{ex.InnerException}");
                    }
                }

                _stopwatch.Stop();
                string s = (_stopwatch.Elapsed.TotalSeconds * 1000).ToString("F2");
            }
        }

        /// <summary>
        /// WebSocket終了処理
        /// </summary>
        /// <returns></returns>
        private async Task CloseAsync()
        {
            SetOther(false);
            SetRouteMode(false);
            if (_webSocket != null && _webSocket.State == WebSocketState.Open)
            {
                // 正常に接続を閉じる
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing",
                    CancellationToken.None);
                status = ConnectionState.DisConnect;
                ConnectionStatusChanged?.Invoke(status);
            }

            _webSocket.Dispose();
            _cacheClearTimer.Dispose();
        }

        /// <summary>
        /// 文字列に不正な文字が含まれているか判定する
        /// </summary>
        public static bool HasInvalidChars(string input)
        {
            foreach (char c in input)
            {
                // 制御文字（改行・タブを除く）またはU+FFFD（�）が含まれていたら文字化け
                if ((char.IsControl(c) && c != '\r' && c != '\n' && c != '\t') || c == '\uFFFD')
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// フィールド・プロパティ置換メソッド
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <exception cref="ArgumentNullException"></exception>
        private void UpdateFieldsAndProperties<T>(T target, T source) where T : class
        {
            if (target == null || source == null)
            {
                var e = new RelayException(5, "ターゲットまたはソースは null にできません");
                AddExceptionAction.Invoke(e);
            }

            // プロパティのキャッシュを取得または設定
            var properties = PropertyCache.GetOrAdd(target.GetType(),
                t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));
            foreach (var property in properties)
            {
                if (property.CanWrite)
                {
                    var newValue = property.GetValue(source);
                    property.SetValue(target, newValue);
                }
            }

            // フィールドのキャッシュを取得または設定
            var fields = FieldCache.GetOrAdd(target.GetType(),
                t => t.GetFields(BindingFlags.Public | BindingFlags.Instance));
            foreach (var field in fields)
            {
                var newValue = field.GetValue(source);
                field.SetValue(target, newValue);
            }
        }

        public void SetEB(bool State, bool force = false)
        {
            try
            {
                if (State)
                {
                    TrainCrewInput.SetATO_Notch(-8);
                    brake = -8;
                }
                else if ((brake == -8 || force) && !State)
                {
                    TrainCrewInput.SetATO_Notch(0);
                    brake = 0;
                }
            }
            catch (Exception ex)
            {
                SetEB(State, force);
            }
        }

        public void ATSResetPush()
        {
            //Todo:ATS復帰入力
        }

        /// <summary>
        /// 全信号を強制的に R（停止）または None（解除）で TrainCrew に送信する。
        /// 緊急停止系の動作で TrainCrew 側現示を一括上書きするためのルート。
        /// </summary>
        /// <param name="IsStop">true=全信号 R、false=全信号 None</param>
        public void ForceStopSignal(bool IsStop)
        {
            TrainCrewInput.GetTrainState();
            var phase = IsStop ? Phase.R : Phase.None;
            var items = TrainCrewInput.signals
                .ToList()
                .Select(s => (Name: s.name, Phase: phase))
                .ToList();
            // このルートでは通常の差分送信ルートを通さずに現示を上書きしているため、
            // 差分送信キャッシュと実機状態がずれる。次回 SetSignalPhases で全信号を再送して整合を取る
            InvalidateSignalPhaseCache();
            _ = SetSignalPhases(items);
        }

    }
}