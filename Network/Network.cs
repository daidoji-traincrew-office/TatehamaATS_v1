using OpenIddict.Abstractions;
using OpenIddict.Client;

namespace TatehamaATS_v1.Network
{
    using Microsoft.AspNetCore.SignalR.Client;
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Net.WebSockets;
    using System.Text.RegularExpressions;
    using TatehamaATS_v1.Exceptions;
    using TrainCrewAPI;

    public class Network
    {
        public static HubConnection connection;
        public static bool connected { get; set; } = false;
        private static string _token = "";
        private readonly OpenIddictClientService _service;

        /// <summary>
        /// 送信するべきデータ
        /// </summary>
        private DataToServer SendData;

        /// <summary>
        /// サーバーから来たデータ
        /// </summary>
        DataFromServer DataFromServer;

        /// <summary>
        /// 車両データ
        /// </summary>
        private TrainCrewStateData TcData;

        /// <summary>
        /// 運転会列番
        /// </summary>
        internal string OverrideDiaName;

        /// <summary>
        /// 防護無線発報状態
        /// </summary>
        internal bool IsBougo;

        /// <summary>
        /// 早着撤去無視フラグ
        /// </summary>
        internal bool IsTherePreviousTrainIgnore;

        /// <summary>
        /// 故障発生
        /// </summary>
        internal event Action<ATSCommonException> AddExceptionAction;

        /// <summary>
        /// 接続状態変化
        /// </summary>
        internal event Action<bool> ConnectionStatusChanged;

        /// <summary>
        /// 信号制御情報変化
        /// </summary>
        internal event Action<DataFromServer, bool> ServerDataUpdate;

        /// <summary>
        /// 通信部疎通確認
        /// </summary>
        internal event Action NetworkWorking;

        /// <summary>
        /// 列番上限不一致情報変化
        /// </summary>
        internal event Action<bool> RetsubanInOutStatusChanged;

        private bool previousStatus;
        private bool connectErrorDialog = false;
        private bool previousDriveStatus;

        public Network(OpenIddictClientService service)
        {
            _service = service;
            StartUpdateLoop();
            OverrideDiaName = "9999";
            TcData = new TrainCrewStateData();
            IsBougo = false;
            SendData = new DataToServer();
            SendDataUpdate();
            previousStatus = false;
            connectErrorDialog = false;
        }

        public void TcDataUpdate(TrainCrewStateData trainCrewStateData)
        {
            TcData = trainCrewStateData;
            SendDataUpdate();
        }

        public void StartUpdateLoop()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        await UpdateLoop();
                    }
                    catch (Exception ex)
                    {
                        var e = new NetworkCountaException(7, "UpdateLoop再起動", ex);
                        AddExceptionAction.Invoke(e);
                    }
                }
            });
        }

        /// <summary>
        /// 定常ループ
        /// </summary>
        /// <returns></returns>
        private async Task UpdateLoop()
        {
            while (true)
            {
                var timer = Task.Delay(100);
                await timer;
                try
                {
                    if (!connected)
                    {
                        AddExceptionAction.Invoke(new NetworkConnectException(7, "未接続"));
                        continue;
                    }
                    await SendData_to_Server();
                }
                catch (Exception ex)
                {
                    var e = new NetworkCountaException(7, "UpdateLoopぶつ切り", ex);
                    AddExceptionAction.Invoke(e);
                }
            }
        }

        /// <summary>
        /// 認証処理
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Authorize()
        {
            using var source = new CancellationTokenSource(delay: TimeSpan.FromSeconds(90));
            try
            {
                // Ask OpenIddict to initiate the authentication flow (typically, by starting the system browser).
                var result = await _service.ChallengeInteractivelyAsync(new()
                {
                    CancellationToken = source.Token
                });

                // Wait for the user to complete the authorization process.
                var resultAuth = await _service.AuthenticateInteractivelyAsync(new()
                {
                    CancellationToken = source.Token,
                    Nonce = result.Nonce
                });
                _token = resultAuth.BackchannelAccessToken!;
                // 認証完了！      
                await Connect();
                return true;
            }
            catch (OperationCanceledException)
            {
                // その他別な理由で認証失敗      
                var e = new NetworkAuthorizeException(7, "認証タイムアウト");
                AddExceptionAction.Invoke(e);
                if (connectErrorDialog) return false;
                connectErrorDialog = true;
                DialogResult result = MessageBox.Show($"認証でタイムアウトしました。\n再認証してください。\n※いいえを選択した場合、再認証にはATS再起動が必要です。", "認証失敗 | 館浜ATS - ダイヤ運転会", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (result == DialogResult.Yes)
                {
                    var r = await Authorize();
                    connectErrorDialog = false;
                    return r;
                }
                return false;
            }
            catch (OpenIddictExceptions.ProtocolException exception) when (exception.Error ==
                                                               OpenIddictConstants.Errors.AccessDenied)
            {
                // ログインしたユーザーがサーバーにいないか、入鋏ロールがついてない
                var e = new NetworkAccessDenied(7, "認証拒否(サーバー非存在・未入鋏)", exception);
                AddExceptionAction.Invoke(e);
                MessageBox.Show($"認証が拒否されました。\n運転会サーバーに参加し入鋏を受けてください。", "認証拒否 | 館浜ATS - ダイヤ運転会");
                return false;
            }
            catch (OpenIddictExceptions.ProtocolException exception) when (exception.Error ==
                                                                           OpenIddictConstants.Errors.ServerError)
            {
                // サーバーでトラブル発生                                                                                                       
                var e = new NetworkAuthorizeException(7, "認証拒否以外", exception);
                AddExceptionAction.Invoke(e);
                if (connectErrorDialog) return false;
                connectErrorDialog = true;
                DialogResult result = MessageBox.Show($"認証に失敗しました。\n再認証しますか？\n※いいえを選択した場合、再認証にはATS再起動が必要です。\n\n{exception.Message}\n{exception.StackTrace}", "認証失敗 | 館浜ATS - ダイヤ運転会", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (result == DialogResult.Yes)
                {
                    var r = await Authorize();
                    connectErrorDialog = false;
                    return r;
                }
                return false;
            }
            catch (Exception exception)
            {
                // その他別な理由で認証失敗      
                var e = new NetworkAuthorizeException(7, "認証失敗理由不明", exception);
                AddExceptionAction.Invoke(e);
                if (connectErrorDialog) return false;
                connectErrorDialog = true;
                DialogResult result = MessageBox.Show($"認証に失敗しました。\n再認証しますか？\n※いいえを選択した場合、再認証にはATS再起動が必要です。\n\n{exception.Message}\n{exception.StackTrace}", "認証失敗 | 館浜ATS - ダイヤ運転会", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (result == DialogResult.Yes)
                {
                    var r = await Authorize();
                    connectErrorDialog = false;
                    return r;
                }
                return false;
            }
        }

        /// <summary>
        /// 接続処理
        /// </summary>
        /// <returns></returns>
        public async Task Connect()
        {
            AddExceptionAction?.Invoke(new NetworkConnectException(7, "通信部接続失敗"));
            ConnectionStatusChanged?.Invoke(connected);

            connection = new HubConnectionBuilder()
                .WithUrl($"{ServerAddress.SignalAddress}/hub/train?access_token={_token}")
                .WithAutomaticReconnect(Enumerable.Range(0, 721).Select(x => TimeSpan.FromSeconds(x == 0 ? 0 : 5)).ToArray())
                .Build();

            //connection.On<DataFromServer>("ReceiveData_ATS", DataFromServer =>
            //{
            //    Debug.WriteLine("受信");
            //    Debug.WriteLine(DataFromServer.ToString());
            //    ServerDataUpdate?.Invoke(DataFromServer);
            //});

            while (!connected)
            {
                try
                {
                    await connection.StartAsync();
                    Debug.WriteLine("Connected");
                    connected = true;
                    ConnectionStatusChanged?.Invoke(connected);
                }
                // 該当Hubにアクセスするためのロールが無いときのエラー 
                catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
                {
                    Debug.WriteLine("Forbidden");
                    connected = false;
                    ConnectionStatusChanged?.Invoke(connected);
                    var e = new NetworkAccessDenied(7, "通信部接続失敗", ex);
                    AddExceptionAction.Invoke(e);
                    if (connectErrorDialog) return;
                    connectErrorDialog = true;
                    DialogResult result = MessageBox.Show($"ロール不足です。\nアカウントを確認して再認証してください。\n再認証しますか？\n※いいえを選択した場合、再認証にはATS再起動が必要です。", "認証失敗 | 館浜ATS - ダイヤ運転会", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                    if (result == DialogResult.Yes)
                    {
                        await Authorize();
                    }
                    connectErrorDialog = false;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("connection Error!!");
                    connected = false;
                    ConnectionStatusChanged?.Invoke(connected);
                    var e = new NetworkConnectException(7, "通信部接続失敗", ex);
                    AddExceptionAction.Invoke(e);
                }
            }

            connection.Reconnecting += exception =>
            {
                connected = false;
                ConnectionStatusChanged?.Invoke(connected);
                Debug.WriteLine("reconnecting");
                return Task.CompletedTask;
            };

            connection.Reconnected += exeption =>
            {
                connected = true;
                ConnectionStatusChanged?.Invoke(connected);
                Debug.WriteLine("Connected");
                return Task.CompletedTask;
            };
            await Task.Delay(Timeout.Infinite);
        }



        /// <summary>
        /// 送信データ更新
        /// </summary>
        private void SendDataUpdate()
        {
            try
            {
                if (TcData != null)
                {
                    SendData.Speed = TcData.myTrainData.Speed;
                    SendData.PNotch = TcData.myTrainData.Pnotch;
                    SendData.BNotch = TcData.myTrainData.Bnotch;
                    SendData.CarStates = TcData.myTrainData.CarStates;
                }
                //単純に代入の皆様
                SendData.DiaName = OverrideDiaName;
                SendData.BougoState = IsBougo;

                if (TcData.trackCircuitList != null)
                {
                    var sendCircuit = new List<TrackCircuitData>();
                    var HasInvalidCharsFlag = false;
                    //軌道回路
                    foreach (var trackCircuit in TcData.trackCircuitList.ToList())
                    {
                        if (trackCircuit.On)
                        {
                            if (trackCircuit.Last == TcData.myTrainData.diaName)
                            {
                                trackCircuit.Last = OverrideDiaName;
                                if (HasInvalidChars(trackCircuit.Name)) HasInvalidCharsFlag = true;
                                sendCircuit.Add(trackCircuit);
                            }
                        }
                    }
                    if (!HasInvalidCharsFlag)
                    {
                        SendData.OnTrackList = sendCircuit;
                    }
                }
                SendData.Speed = TcData.myTrainData.Speed;
                SendData.CarStates = TcData.myTrainData.CarStates;
                // まだない
                SendData.Acceleration = 0.0f;
            }
            catch (Exception ex)
            {
                var e = new NetworkCountaException(7, "SendDataUpdateぶつ切り", ex);
                AddExceptionAction.Invoke(e);
            }
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
        /// サーバーにデータ
        /// </summary>
        /// <returns></returns>
        public async Task SendData_to_Server()
        {
            try
            {
                bool currentStatus = true;
                if (TcData == null)
                {
                    NetworkWorking?.Invoke();
                    return;
                }
                if (TcData.gameScreen == GameScreen.MainGame || TcData.gameScreen == GameScreen.MainGame_Pause)
                {
                    previousDriveStatus = true;
                    try
                    {
                        currentStatus = IsOddTrainNumber(OverrideDiaName) != IsOddTrainNumber(TcData.myTrainData.diaName);
                    }
                    catch
                    {
                    }

                    if (currentStatus)
                    {
                        //上下線不一致のため、強制Rとするが、在線情報は送信したいので、処理そのものは続行
                        RetsubanInOutStatusChanged.Invoke(true);
                    }
                    else if (!currentStatus && currentStatus != previousStatus)
                    {
                        RetsubanInOutStatusChanged.Invoke(false);
                    }

                    previousStatus = currentStatus;
                    //Debug.WriteLine($"{SendData}");        
                    DataFromServer dataFromServer;
                    dataFromServer = await connection.InvokeAsync<DataFromServer>("SendData_ATS", SendData);

                    if (dataFromServer.IsOnPreviousTrain)
                    {
                        currentStatus = true;
                    }
                    ServerDataUpdate?.Invoke(dataFromServer, currentStatus);
                }
                else
                {
                    if (previousDriveStatus)
                    {
                        IsTherePreviousTrainIgnore = false;
                        DriverGetsOff();
                    }
                    NetworkWorking?.Invoke();
                }
            }
            catch (WebSocketException e) when (e.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
            {
                // 再接続を試みる
                try
                {
                    await connection.StopAsync();
                    await connection.StartAsync();
                    connected = true;
                    ConnectionStatusChanged?.Invoke(connected);
                }
                catch (Exception rex)
                {
                    AddExceptionAction?.Invoke(new NetworkConnectException(7, "再接続失敗", rex));
                    connected = false;
                    ConnectionStatusChanged?.Invoke(connected);
                }
            }
            catch (InvalidOperationException ex)
            {
                var e = new NetworkConnectException(7, "切断と思われる", ex);
                AddExceptionAction.Invoke(e);
            }
            catch (Exception ex)
            {
                var e = new NetworkDataException(7, "", ex);
                AddExceptionAction.Invoke(e);
            }
        }

        public async void DriverGetsOff()
        {
            try
            {
                await connection.InvokeAsync<DataFromServer>("DriverGetsOff", OverrideDiaName);
                previousDriveStatus = false;
            }
            catch (WebSocketException e) when (e.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
            {
                // 再接続を試みる
                try
                {
                    await connection.StopAsync();
                    await connection.StartAsync();
                    connected = true;
                    ConnectionStatusChanged?.Invoke(connected);
                }
                catch (Exception rex)
                {
                    AddExceptionAction?.Invoke(new NetworkConnectException(7, "再接続失敗", rex));
                    connected = false;
                    ConnectionStatusChanged?.Invoke(connected);
                }
            }
            catch (InvalidOperationException ex)
            {
                var e = new NetworkConnectException(7, "切断と思われる", ex);
                AddExceptionAction.Invoke(e);
            }
            catch (Exception ex)
            {
                var e = new NetworkDataException(7, "", ex);
                AddExceptionAction.Invoke(e);
            }
        }

        /// <summary>
        /// 列番が奇数か偶数かを判定する
        /// </summary>
        /// <param name="trainNumber">列番</param>
        /// <returns>奇数ならtrue、偶数ならfalse</returns>
        private bool IsOddTrainNumber(string trainNumber)
        {
            try
            {
                var lastDiaNumber = trainNumber.Last(char.IsDigit) - '0';
                var isUp = lastDiaNumber % 2 == 0;
                return isUp;
            }
            catch (Exception)
            {
                throw new ArgumentException("列番が無効です。");
            }
        }

        public async Task Close()
        {
            await connection.StopAsync();
            await connection.DisposeAsync();
        }

        public void IsTherePreviousTrainIgnoreSet()
        {
            // 早着撤去無視フラグをセット、早着状態なら無視フラグ設定できる。
            if (DataFromServer == null)
            {
                return;
            }
            IsTherePreviousTrainIgnore = DataFromServer.IsTherePreviousTrain;
        }
    }
}