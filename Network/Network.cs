namespace TatehamaATS_v1.Network
{
    using System.Diagnostics;
    using System.Net.WebSockets;
    using System.Runtime.CompilerServices;
    using Microsoft.AspNetCore.SignalR.Client;
    using TatehamaATS_v1.Exceptions;
    using static System.Windows.Forms.VisualStyles.VisualStyleElement;

    public class Network
    {
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
        internal async Task TryConnect()
        {
            //Todo:通信できてない時無限に繰り返すようにしたい
            while (true)
            {
                try
                {
                    await Connect();
                }
                catch (ATSCommonException ex)
                {
                    AddExceptionAction.Invoke(ex);
                }
                catch (Exception ex)
                {
                    var e = new SocketException(3, "通信部なんかあった", ex);
                    AddExceptionAction.Invoke(e);
                }
                break;
            }
        }

        private async Task Connect()
        {
            var client = new HubConnectionBuilder()
                .WithUrl("http://localhost:5154/hub/train")
                .WithAutomaticReconnect()
                .Build();
            client.On<String, String>("RecieveMessage", (user, message) =>
            {
                Console.WriteLine("Hello");
            });

            try
            {
                await client.StartAsync();
                Console.WriteLine("Connected");
                while (true)
                {
                    var message = Console.ReadLine();
                    await client.InvokeAsync("SendMessage", "arai", message);
                }
            }
            catch (Exception ex)
            {
                throw new SocketConnectException(3, "通信部接続時になんかあった", ex);
            }
            finally
            {
                await client.StopAsync();
                await client.DisposeAsync();
                Debug.WriteLine("Cpnnection Closed");
            }
        }
    }
}