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
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new ConcurrentDictionary<Type, PropertyInfo[]>();
        private static readonly ConcurrentDictionary<Type, FieldInfo[]> FieldCache = new ConcurrentDictionary<Type, FieldInfo[]>();

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

        // SignalSet/UpdateRoute用の同時実行防止ロック
        private readonly object _routeSignalLock = new object();

        private Dictionary<string, string> StaNameById = new Dictionary<string, string>()
        {
            {"TH76","館浜"},
            {"TH75","駒野"},
            {"TH71","津崎"},
            {"TH70","浜園"},
            {"TH67","新野崎"},
            {"TH66S","江ノ原信号場"},
            {"TH65","大道寺"},
            {"TH64","藤江"},
            {"TH63","水越"},
            {"TH62","高見沢"},
            {"TH61","日野森"},
            {"TH59","西赤山"},
            {"TH58","赤山町"}
        };

        private Dictionary<string, string> StaStopById = new Dictionary<string, string>()
        {
            {"TH76","停車"},
            {"TH75","停車"},
            {"TH71","停車"},
            {"TH70","停車"},
            {"TH67","停車"},
            {"TH66S","停車"},
            {"TH65","停車"},
            {"TH64","停車"},
            {"TH63","停車"},
            {"TH62","停車"},
            {"TH61","停車"},
            {"TH59","停車"},
            {"TH58","停車"}
        };

        /// <summary>
        /// 運転会列番
        /// </summary>
        internal string OverrideDiaName;

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

        // SetSignalPhase レート制限用
        private const int MaxConcurrentSignalCommands = 3;           // 1バッチあたりの最大送信数
        private const int SignalCommandBaseIntervalMs = 250;          // バッチの基準間隔
        private const int SignalCommandMaxBackoffMs = 200_000;         // 同一信号向けの最大バックオフ
        private const double SignalCommandBackoffGrowFactor = 2.0;     // バックオフ増加係数（指数バックオフ）
        private const double SignalCommandBackoffDecayFactor = 1.5 / SignalCommandBackoffGrowFactor;    // バックオフ減少係数（対数的減少）
        private const int SignalHistoryTtlMs = 300_000;                 // 信号履歴の有効期間
        private const int SignalQueueTtlMs = 150_000;                    // キューに残す指示の最大寿命
        private readonly object _signalPhaseQueueLock = new object(); // キュー用ロック
        private readonly Queue<SignalPhaseCommand> _signalPhaseQueue = new Queue<SignalPhaseCommand>();
        private readonly Dictionary<string, SignalPhaseHistory> _signalPhaseHistory = new Dictionary<string, SignalPhaseHistory>();
        private DateTime _lastSignalBatchTime = DateTime.MinValue;
        private bool _isProcessingSignalQueue = false;               // キュー処理中フラグ
        private Task? _queueProcessingTask; // 処理中タスク
        // 現示優先度（値が小さいほど「下位現示」＝より厳しい）
        private static readonly Dictionary<Phase, int> SignalPhasePriority = new()
        {
            { Phase.R, 0 },
            { Phase.YY, 1 },
            { Phase.Y, 2 },
            { Phase.YG, 3 },
            { Phase.G, 4 },
            { Phase.None, 5 },
        };

        /// <summary>
        /// TrainCrew側データ要求コマンド
        /// (DataRequest, SetEmergencyLight, SetSignalPhase)
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
        private static bool IsValidCommand(string requestValues) =>
            new[] { "DataRequest", "SetEmergencyLight", "SetSignalPhase", "mode_req", "SetRoute", "DeleteRoute", "DeleteRoute2", "realtimeoffset" }.Contains(requestValues);

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
                           requestValues.All(str => str == "tc" || str == "tconlyontrain" || str == "tcall" || str == "signal" || str == "train");
                case "SetEmergencyLight":
                    return requestValues.Length == 2 && (requestValues[1] == "true" || requestValues[1] == "false");
                case "SetSignalPhase":
                    return requestValues.Length == 2 && (requestValues[1] == "None" || requestValues[1] == "R" || requestValues[1] == "YY" || requestValues[1] == "Y" || requestValues[1] == "YG" || requestValues[1] == "G");
                case "mode_req":
                    return requestValues.Length == 1 && (requestValues[0] == "hide_other" || requestValues[0] == "show_other" || requestValues[0] == "route_manual" || requestValues[0] == "route_auto" || requestValues[0] == "realtimemode_on");
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
            TrainCrewInput.RequestData(DataRequest.Signal);
            _webSocket = new ClientWebSocket();
        }

        /// <summary>
        /// 受信データ処理メソッド
        /// </summary>
        private void ProcessingReceiveData()
        {
            TrainCrewDataUpdated.Invoke(TcData);
            if (TcData.gameScreen == TrainCrewAPI.GameScreen.Menu)
            {
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
                    var stationId = StaNameById.FirstOrDefault(x => x.Value == interlockData.Name.Replace("連動装置", "")).Key;
                    var route = new Route
                    {

                        TcName = $"{stationId}_{interlockRoute.Name}",
                        RouteType = RouteType.SwitchRoute,
                        Indicator = "",
                        RouteState = null
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

        internal void SignalSet(List<SignalData> signalDatas)
        {
            if (TcData.gameScreen is not (TrainCrewAPI.GameScreen.MainGame or TrainCrewAPI.GameScreen.MainGame_Pause))
            {
                return;
            }

            if (status != ConnectionState.Connected)
            {
                return;
            }

            lock (_routeSignalLock)
            {
                SignalSetCore(signalDatas);
            }
        }

        private void SignalSetCore(List<SignalData>? signalDatas)
        {
            // 新しい信号データをDictionary化
            var newSignalDict = (signalDatas ?? []).ToDictionary(s => s.Name, s => s);
            Dictionary<string, SignalData> nowSignalDict;
            lock (TcData)
            {
                nowSignalDict = TcData.signalDataList.ToDictionary(s => s.Name, s => s);
            }

            // 追加・変更された信号
            var addedSignals = newSignalDict.Values
                .Where(newSignal => !nowSignalDict.TryGetValue(newSignal.Name, out var existingSignal) || existingSignal.phase != newSignal.phase)
                .ToList();

            // 削除された信号
            var removedSignals = nowSignalDict.Values
                .Where(existingSignal => !newSignalDict.ContainsKey(existingSignal.Name))
                .ToList();

            // 追加された信号の現示を送信
            foreach (var signalData in addedSignals)
            {
                // 個別信号の現示設定
                SetSignalPhase(signalData.Name, signalData.phase);
            }

            // 削除された信号の現示をRに設定して送信
            foreach (var signalData in removedSignals)
            {
                // 削除信号は停止現示に戻す
                SetSignalPhase(signalData.Name, Phase.R);
            }
        }

        internal void EMSet(List<EmergencyLightData> emergencyLightDatas)
        {
            foreach (var emergencyLightData in emergencyLightDatas.ToList())
            {
                SendSingleCommand("SetEmergencyLight", new string[] { emergencyLightData.Name, emergencyLightData.State ? "true" : "false" });
            }
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

        internal void UpdateRoute(List<Route> routes)
        {
            if (!(TcData.gameScreen == TrainCrewAPI.GameScreen.MainGame || TcData.gameScreen == TrainCrewAPI.GameScreen.MainGame_Pause))
            {
                return;
            }

            if (routes == null)
            {
                Debug.WriteLine("routes is null. Skipping UpdateRoute.");
                return;
            }

            if (TrainCrewRoutes == null)
            {
                Debug.WriteLine("TrainCrewRoutes is null. Initializing empty list.");
                TrainCrewRoutes = new List<Route>();
            }

            lock (_routeSignalLock)
            {
                UpdateRouteCore(routes);
            }
        }

        private void UpdateRouteCore(List<Route> routes)
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
                SetRoute(route);
            }

            foreach (var route in removedRoutes.ToList())
            {
                DeleteRoute(route);
            }
        }

        private void SetRoute(Route route)
        {
            try
            {
                if (!(TcData.gameScreen.HasFlag(TrainCrewAPI.GameScreen.MainGame) || TcData.gameScreen.HasFlag(TrainCrewAPI.GameScreen.MainGame_Pause)))
                {
                    return;
                }

                if (status != ConnectionState.Connected)
                {
                    return;
                }

                var r = route.TcName.Split('_').ToList();
                // staID仮対応
                var staName = StaNameById[r[0]] + "連動装置";

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
                        indicator = TypeString(OverrideDiaName);
                        SetStaStop(indicator);
                        break;
                    default:
                        indicator = "";
                        break;
                }

                //Debug.WriteLine($"☆API送信: SetRoute/{route.TcName}/{StaStopById[r[0]]}");
                SendSingleCommand("SetRoute", [staName, routeName, indicator, TcData.myTrainData.diaName, StaStopById[r[0]]]);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message}{ex.InnerException}");
            }
        }

        private void DeleteRoute(Route route)
        {
            try
            {
                if (!(TcData.gameScreen.HasFlag(TrainCrewAPI.GameScreen.MainGame) || TcData.gameScreen.HasFlag(TrainCrewAPI.GameScreen.MainGame_Pause)))
                {
                    return;
                }

                if (status != ConnectionState.Connected)
                {
                    return;
                }

                var r = route.TcName.Split('_').ToList();
                // staID仮対応
                var staName = StaNameById[r[0]] + "連動装置";

                // 末尾が "S[A-Z]" または "T[A-Z]" の場合に "[A-Z]" の部分だけを残す
                var routeName = System.Text.RegularExpressions.Regex.Replace(r[1], @"[ST]([A-Z])$", "$1");

                //Debug.WriteLine($"☆API送信: DeleteRoute2/{route.TcName}");
                SendSingleCommand("DeleteRoute2", [staName, routeName]);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message}{ex.InnerException}");
            }
        }

        internal void SetTime(int shiftTime)
        {
            try
            {
                if (Relay.shiftTime == shiftTime)
                {
                    return;
                }
                Relay.shiftTime = shiftTime;
                //Debug.WriteLine($"☆API送信: realtimeoffset/{shiftTime}");
                SendSingleCommand("realtimeoffset", [$"{shiftTime}"]);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message}{ex.InnerException}");
            }
        }


        internal string TypeString(string Retsuban)
        {
            Retsuban = Retsuban.Replace("X", "").Replace("Y", "").Replace("Z", "");
            if (Retsuban == "9999")
            {
                return "";
            }

            if (Retsuban.Contains("溝月"))
            {
                return "回送";
            }

            if (Retsuban.StartsWith("回"))
            {
                return "回送";
            }

            if (Retsuban.StartsWith("試"))
            {
                return "回送";
            }

            if (Retsuban.Contains("A"))
            {
                return "A特";
            }

            if (Retsuban.Contains("K"))
            {
                return "快速急行";
            }

            if (Retsuban.Contains("B"))
            {
                return "急行";
            }

            if (Retsuban.Contains("C"))
            {
                return "準急";
            }

            if (Retsuban.StartsWith("臨"))
            {
                return "回送";
            }

            if (int.TryParse(Retsuban, null, out _))
            {
                return "普通";
            }

            return "回送";
        }

        private void SetStaStop(string name)
        {
            switch (name)
            {
                case "普通":
                    StaStopById = new Dictionary<string, string>()
                        {
                            {"TH76","停車"},
                            {"TH75","停車"},
                            {"TH71","停車"},
                            {"TH70","停車"},
                            {"TH67","停車"},
                            {"TH66S","停車"},
                            {"TH65","停車"},
                            {"TH64","停車"},
                            {"TH63","停車"},
                            {"TH62","停車"},
                            {"TH61","停車"},
                            {"TH59","停車"},
                            {"TH58","停車"}
                        };
                    break;
                case "準急":
                    StaStopById = new Dictionary<string, string>()
                        {
                            {"TH76","停車"},
                            {"TH75","停車"},
                            {"TH71","通過"},
                            {"TH70","停車"},
                            {"TH67","停車"},
                            {"TH66S","停車"},
                            {"TH65","停車"},
                            {"TH64","停車"},
                            {"TH63","停車"},
                            {"TH62","停車"},
                            {"TH61","停車"},
                            {"TH59","停車"},
                            {"TH58","停車"}
                        };
                    break;
                case "急行":
                    StaStopById = new Dictionary<string, string>()
                        {
                            {"TH76","停車"},
                            {"TH75","通過"},
                            {"TH71","通過"},
                            {"TH70","通過"},
                            {"TH67","停車"},
                            {"TH66S","停車"},
                            {"TH65","停車"},
                            {"TH64","停車"},
                            {"TH63","停車"},
                            {"TH62","停車"},
                            {"TH61","停車"},
                            {"TH59","停車"},
                            {"TH58","停車"}
                        };
                    break;
                case "快速急行":
                    StaStopById = new Dictionary<string, string>()
                        {
                            {"TH76","停車"},
                            {"TH75","通過"},
                            {"TH71","通過"},
                            {"TH70","通過"},
                            {"TH67","停車"},
                            {"TH66S","停車"},
                            {"TH65","停車"},
                            {"TH64","停車"},
                            {"TH63","停車"},
                            {"TH62","通過"},
                            {"TH61","停車"},
                            {"TH59","通過"},
                            {"TH58","停車"}
                        };
                    break;
                case "A特":
                    StaStopById = new Dictionary<string, string>()
                        {
                            {"TH76","停車"},
                            {"TH75","通過"},
                            {"TH71","通過"},
                            {"TH70","通過"},
                            {"TH67","通過"},
                            {"TH66S","通過"},
                            {"TH65","通過"},
                            {"TH64","通過"},
                            {"TH63","通過"},
                            {"TH62","通過"},
                            {"TH61","通過"},
                            {"TH59","通過"},
                            {"TH58","通過"}
                        };
                    break;
                case "回送":
                    StaStopById = new Dictionary<string, string>()
                        {
                            {"TH76","停車"},
                            {"TH75","通過"},
                            {"TH71","通過"},
                            {"TH70","通過"},
                            {"TH67","通過"},
                            {"TH66S","停車"},
                            {"TH65","通過"},
                            {"TH64","通過"},
                            {"TH63","通過"},
                            {"TH62","通過"},
                            {"TH61","通過"},
                            {"TH59","通過"},
                            {"TH58","通過"}
                        };
                    break;
            }
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
                        var _trainCrewStateData = JsonConvert.DeserializeObject<TrainCrewStateData>(baseData.data.ToString());

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
                        var _recvBeaconStateData = JsonConvert.DeserializeObject<RecvBeaconStateData>(baseData.data.ToString());

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
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", CancellationToken.None);
                status = ConnectionState.DisConnect;
                ConnectionStatusChanged?.Invoke(status);
            }

            _webSocket.Dispose();
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
            var properties = PropertyCache.GetOrAdd(target.GetType(), t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));
            foreach (var property in properties)
            {
                if (property.CanWrite)
                {
                    var newValue = property.GetValue(source);
                    property.SetValue(target, newValue);
                }
            }

            // フィールドのキャッシュを取得または設定
            var fields = FieldCache.GetOrAdd(target.GetType(), t => t.GetFields(BindingFlags.Public | BindingFlags.Instance));
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

        public void ForceStopSignal(bool IsStop)
        {
            TrainCrewInput.GetTrainState();
            foreach (var signalData in TrainCrewInput.signals.ToList())
            {
                // 非常停止中は全信号R、それ以外は消灯
                var phase = IsStop ? Phase.R : Phase.None;
                SetSignalPhase(signalData.name, phase);
            }
        }

        /// <summary>
        /// 単一信号の現示を設定する
        /// </summary>
        /// <param name="signalName">信号機名</param>
        /// <param name="phase">設定する現示</param>
        internal void SetSignalPhase(string signalName, Phase phase)
        {
            if (signalName == "上り1閉塞")
            {
                return;
            }
            // 送信要求をキューに積んでレート制限付きで処理する
            lock (_signalPhaseQueueLock)
            {
                var now = DateTime.UtcNow;

                //Debug.WriteLine($"[SetSignalPhase] Enqueue request: {signalName} / {phase} at {now:HH:mm:ss:fff}");
                //Debug.WriteLine($"[SetSignalPhase] Queue size before enqueue: {_signalPhaseQueue.Count}");

                // 古い履歴を破棄（一定時間以上経過したもの）
                var expiredKeys = _signalPhaseHistory
                    .Where(kv => (now - kv.Value.CreatedAt).TotalMilliseconds > SignalHistoryTtlMs)
                    .Select(kv => kv.Key)
                    .ToList();
                foreach (var key in expiredKeys)
                {
                    _signalPhaseHistory.Remove(key);
                }

                // キューも古すぎるものは破棄（過去の指示が大量に残るのを防ぐ）
                if (_signalPhaseQueue.Count > 0)
                {
                    var temp = new Queue<SignalPhaseCommand>();
                    while (_signalPhaseQueue.Count > 0)
                    {
                        var cmd = _signalPhaseQueue.Dequeue();
                        if ((now - cmd.EnqueuedAt).TotalMilliseconds <= SignalQueueTtlMs)
                        {
                            temp.Enqueue(cmd);
                        }
                    }
                    while (temp.Count > 0)
                    {
                        _signalPhaseQueue.Enqueue(temp.Dequeue());
                    }
                }

                // 既に同一内容の指示がキュー内にある場合は積まない
                if (_signalPhaseQueue.Any(c => c.SignalName == signalName && c.Phase == phase))
                {
                    return;
                }

                // 履歴取得または作成
                if (!_signalPhaseHistory.TryGetValue(signalName, out var history))
                {
                    history = new SignalPhaseHistory
                    {
                        BackoffMs = SignalCommandBaseIntervalMs,
                        LastAttemptTime = DateTime.MinValue,
                        CreatedAt = now
                    };
                    _signalPhaseHistory[signalName] = history;
                    //Debug.WriteLine($"[SetSignalPhase] New history created for {signalName}, backoff={history.BackoffMs}ms");
                }

                // TC側から送られてくる現在の現示と比較して、下位／上位を判定する
                string relation = "First";
                int newPriority = 0;
                int prevPriority = 0;
                // 現在の現示を TcData.signalDataList から取得

                SignalData? currentSignal;
                lock (TcData)
                {
                   currentSignal = TcData.signalDataList.FirstOrDefault(s => s.Name == signalName);
                }
                if (currentSignal != null && SignalPhasePriority.TryGetValue(phase, out newPriority) && SignalPhasePriority.TryGetValue(currentSignal.phase, out prevPriority))
                {
                    relation = newPriority < prevPriority ? "Lower" :
                                newPriority > prevPriority ? "Upper" :
                                "Same";
                    //Debug.WriteLine($"[SetSignalPhase] PriorityCheck {signalName}: current={currentSignal.phase}({prevPriority}), new={phase}({newPriority}) => {relation}");
                }
                else
                {
                    //Debug.WriteLine($"[SetSignalPhase] PriorityCheck {signalName}: no current phase, new={phase}({newPriority})");
                }

                // 次回許可時刻を計算（Upper/Same はバックオフ期間中は破棄、Lower は常に受付）
                var nowMs = now;
                var diffMs = (nowMs - history.LastAttemptTime).TotalMilliseconds;

                if (relation == "Lower")
                {
                    // 下位現示はいつでも積む・バックオフもリセット
                    history.BackoffMs = SignalCommandBaseIntervalMs;
                }
                else
                {
                    if (diffMs < history.BackoffMs)
                    {
                        // バックオフ期間中の Upper/Same 要求は、常にバックオフを増加させてドロップ（指数バックオフ）
                        var nextBackoff = (int)Math.Min(history.BackoffMs * SignalCommandBackoffGrowFactor, SignalCommandMaxBackoffMs);
                        //Debug.WriteLine($"[SetSignalPhase] Drop {signalName} ({phase}) by backoff. relation={relation}, diff={diffMs}ms, backoff {history.BackoffMs}ms -> {nextBackoff}ms");
                        history.BackoffMs = nextBackoff;
                        return;
                    }

                    // バックオフ期間を経過した Upper/Same 要求は受け付けるが、
                    // 前回は期間内・今回は期間外（しきい値を初めて越えた）場合は一度増加させる。
                    // 前回も今回も期間外の場合のみ、対数的（係数指定）に減少させていく。
                    if (history.BackoffMs > SignalCommandBaseIntervalMs && history.LastAttemptTime != DateTime.MinValue)
                    {
                        if (history.LastWasInWindow)
                        {
                            var grown = (int)Math.Min(history.BackoffMs * SignalCommandBackoffGrowFactor, SignalCommandMaxBackoffMs);
                            //Debug.WriteLine($"[SetSignalPhase] Grow backoff for {signalName} after first cooldown crossing: {history.BackoffMs}ms -> {grown}ms, relation={relation}, diff={diffMs}ms");
                            history.BackoffMs = grown;
                        }
                        else
                        {
                            // 緩やかにベース間隔へ近づける（下限は SignalCommandBaseIntervalMs）
                            var decayed = (int)Math.Max(SignalCommandBaseIntervalMs, history.BackoffMs * SignalCommandBackoffDecayFactor);
                            //Debug.WriteLine($"[SetSignalPhase] Decay backoff for {signalName} from {history.BackoffMs}ms to {decayed}ms after cooldown. relation={relation}, diff={diffMs}ms");
                            history.BackoffMs = decayed;
                        }
                    }
                }

                // 今回評価時に「期間内」だったかどうかを保存しておく
                history.LastWasInWindow = diffMs < history.BackoffMs;

                var nextAllowed = nowMs.AddMilliseconds(history.BackoffMs);

                Debug.WriteLine($"[SetSignalPhase] {signalName}: diff={diffMs}ms, backoff={history.BackoffMs}ms, nextAllowed={nextAllowed:HH:mm:ss:fff}, lastAttempt={history.LastAttemptTime:HH:mm:ss:fff}, relation={relation}");

                _signalPhaseQueue.Enqueue(new SignalPhaseCommand
                {
                    SignalName = signalName,
                    Phase = phase,
                    EnqueuedAt = now,
                    EarliestSendTime = nextAllowed
                });

                //Debug.WriteLine($"[SetSignalPhase] Enqueued: {signalName}, earliest={nextAllowed:HH:mm:ss:fff}, backoff={history.BackoffMs}ms");
                //Debug.WriteLine($"[SetSignalPhase] Queue size after enqueue: {_signalPhaseQueue.Count}");

                // バッチ処理をトリガー（多重起動を防ぐ）
                if (!_isProcessingSignalQueue)
                {
                    Debug.WriteLine("[SetSignalPhase] Starting ProcessSignalPhaseQueueAsync");
                    _isProcessingSignalQueue = true;
                    _queueProcessingTask = Task.Run(ProcessSignalPhaseQueueAsync);
                }
            }
        }

        /// <summary>
        /// SetSignalPhase キューをレート制限付きで処理する
        /// </summary>
        private async Task ProcessSignalPhaseQueueAsync()
        {
            try
            {
                while (true)
                {
                    List<SignalPhaseCommand> batch;
                    int delayMs;

                    lock (_signalPhaseQueueLock)
                    {
                        //Debug.WriteLine($"[ProcessQueue] Loop start, queue size={_signalPhaseQueue.Count}, lastBatch={_lastSignalBatchTime:HH:mm:ss:fff}");
                        if (_signalPhaseQueue.Count == 0)
                        {
                            // キューが空なら処理終了
                            //Debug.WriteLine("[ProcessQueue] Queue empty, stopping processor");
                            _isProcessingSignalQueue = false;
                            return;
                        }

                        var now = DateTime.UtcNow;

                        // 前回バッチからの経過時間に基づいて待機時間を計算
                        var elapsedSinceLastBatch = (now - _lastSignalBatchTime).TotalMilliseconds;
                        delayMs = elapsedSinceLastBatch >= SignalCommandBaseIntervalMs
                            ? 0
                            : SignalCommandBaseIntervalMs - (int)elapsedSinceLastBatch;

                        //Debug.WriteLine($"[ProcessQueue] elapsedSinceLastBatch={elapsedSinceLastBatch}ms, delayMs={delayMs}ms");

                        // 送信可能なものを最大数まで取り出す
                        batch = new List<SignalPhaseCommand>(MaxConcurrentSignalCommands);
                        var remaining = new Queue<SignalPhaseCommand>();
                        while (_signalPhaseQueue.Count > 0)
                        {
                            var cmd = _signalPhaseQueue.Dequeue();

                            // まだ送信許可時刻に達していないもの、またはバッチ上限超過分は次回以降に回す
                            if (cmd.EarliestSendTime > now || batch.Count >= MaxConcurrentSignalCommands)
                            {
                                if (cmd.EarliestSendTime > now)
                                {
                                    //Debug.WriteLine($"[ProcessQueue] Defer (not yet allowed): {cmd.SignalName}, earliest={cmd.EarliestSendTime:HH:mm:ss:fff}");
                                }
                                else
                                {
                                    //Debug.WriteLine($"[ProcessQueue] Defer (over limit): {cmd.SignalName}");
                                }
                                remaining.Enqueue(cmd);
                                continue;
                            }

                            batch.Add(cmd);
                        }

                        // 残りのキュー要素を保持
                        while (remaining.Count > 0)
                        {
                            _signalPhaseQueue.Enqueue(remaining.Dequeue());
                        }

                        //Debug.WriteLine($"[ProcessQueue] Built batch size={batch.Count}, remaining queue size={_signalPhaseQueue.Count}");

                        if (batch.Count == 0)
                        {
                            // 送信可能なものがない場合は少し待って再試行
                            delayMs = SignalCommandBaseIntervalMs;
                        }
                        else
                        {
                            _lastSignalBatchTime = now;
                        }
                    }

                    if (delayMs > 0)
                    {
                        //Debug.WriteLine($"[ProcessQueue] Waiting {delayMs}ms before sending batch");
                        await Task.Delay(delayMs);
                    }

                    if (batch == null || batch.Count == 0)
                    {
                        continue;
                    }

                    // バッチ送信（並列にしないことで急激な集中を避ける）
                    //Debug.WriteLine($"[ProcessQueue] Sending batch, size={batch.Count}");
                    foreach (var cmd in batch)
                    {
                        try
                        {
                            Debug.WriteLine($"☆信号名：{cmd.SignalName}／現示：{cmd.Phase.ToString()}");
                            // 実送信
                            await SendSingleCommand("SetSignalPhase", new[] { cmd.SignalName, cmd.Phase.ToString() });

                            // 成功したので履歴を更新
                            lock (_signalPhaseQueueLock)
                            {
                                if (_signalPhaseHistory.TryGetValue(cmd.SignalName, out var history))
                                {
                                    history.LastAttemptTime = DateTime.UtcNow;
                                    history.LastPhase = cmd.Phase;
                                }
                            }
                        }
                        catch
                        {
                            // 送信エラーは SendSingleCommand 内で処理されるため、ここでは再スローしない
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // ここでの例外は握りつぶして処理ルーチンを継続可能にする
                Debug.WriteLine("[ProcessQueue] Exception occurred in processor loop");
                Debug.WriteLine("[ProcessQueue] {0}\n{1}",ex.Message, ex.StackTrace);
            }
            finally
            {
                lock (_signalPhaseQueueLock)
                {
                    Debug.WriteLine("[ProcessQueue] Finally: clearing processing flag");
                    _isProcessingSignalQueue = false;
                }
            }
        }

        // SetSignalPhase 用のキューエントリ
        private sealed class SignalPhaseCommand
        {
            public string SignalName { get; init; } = string.Empty;
            public Phase Phase { get; init; }
            public DateTime EnqueuedAt { get; init; }
            public DateTime EarliestSendTime { get; init; }
        }

        // 同一信号のバックオフ管理用
        private sealed class SignalPhaseHistory
        {
            public int BackoffMs { get; set; }
            public DateTime LastAttemptTime { get; set; }
            public Phase? LastPhase { get; set; }
            public DateTime CreatedAt { get; set; }
            // 直前評価時に diffMs < BackoffMs だったかどうか
            public bool LastWasInWindow { get; set; }
        }

    }
}