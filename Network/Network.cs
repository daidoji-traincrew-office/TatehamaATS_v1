namespace TatehamaATS_v1.Network
{
    using System.Diagnostics;
    using System.Net.WebSockets;
    using System.Runtime.CompilerServices;
    using Microsoft.AspNetCore.Connections;

    using Microsoft.AspNetCore.SignalR.Client;
    using TatehamaATS_v1.Exceptions;
    using static System.Windows.Forms.VisualStyles.VisualStyleElement;

    public class Network
    {
        public static HubConnection connection;
        /// <summary>
        /// 故障発生
        /// </summary>
        internal event Action<ATSCommonException> AddExceptionAction;

        public Network()
        {

        }

        /// <summary>
        /// WebSocket接続試行
        /// </summary>
        /// <returns></returns>
        // internal async Task TryConnect()
        // {
        //     //Todo:通信できてない時無限に繰り返すようにしたい
        //     while (true)
        //     {
        //         try
        //         {
        //             await Connect();
        //         }
        //         catch (ATSCommonException ex)
        //         {
        //             AddExceptionAction.Invoke(ex);
        //         }
        //         catch (Exception ex)
        //         {
        //             var e = new SocketException(3, "通信部なんかあった", ex);
        //             AddExceptionAction.Invoke(e);
        //         }
        //         break;
        //     }
        // }

        public async Task Connect()
        {
            connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5154/hub/train")
                .WithAutomaticReconnect()
                .Build();

            connection.On<DataFromServer>("ReceiveData_ATS", DataFromServer =>
            {
                throw new NotImplementedException();
            });

            bool connected = false;
            while (!connected)
            {
                try
                {
                    await connection.StartAsync();
                    Console.WriteLine("Connected");
                    connected = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("connection Error!!");
                    // throw new SocketConnectException(3, "通信部接続時になんかあった", ex);
                    await Task.Delay(3000);
                }
            }

            connection.Reconnecting += exception =>
            {
                Console.WriteLine("再接続中");
                return Task.CompletedTask;
            };
            await Task.Delay(Timeout.Infinite);
        }

        public async Task SendData_to_Server(DataToServer sendData)
        {
            await connection.SendAsync("SendData_ATS", sendData);
        }

        public async Task Close()
        {
            await connection.StopAsync();
            await connection.DisposeAsync();
        }
    }
}