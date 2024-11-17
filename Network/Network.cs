namespace TatehamaATS_v1.Network
{
    using System.Runtime.CompilerServices;
    using Microsoft.AspNetCore.Connections;

    using Microsoft.AspNetCore.SignalR.Client;

    public class Network
    {
        public static HubConnection connection;
        public static async Task Connect()
        {
            connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5154/hub/train")
                .WithAutomaticReconnect()
                .Build();

            connection.On<String, String>("RecieveMessage", (user, message) =>
            {
                Console.WriteLine("Hello");
            });

            try
            {
                await connection.StartAsync();
                Console.WriteLine("Connected");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error!");
            }
            finally
            {
                await connection.StopAsync();
                await connection.DisposeAsync();
                Console.WriteLine("Cpnnection Closed");
            }
        }

        public async Task SendData_to_Server(DataFromServer sendData)
        {
            await connection.SendAsync("SendData_ATS", sendData); 
        }
    }
}