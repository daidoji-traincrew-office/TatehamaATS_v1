namespace TatehamaATS_v1.Network
{
    using System.Runtime.CompilerServices;
    using Microsoft.AspNetCore.SignalR.Client;

    public class Network
    {
        public static async Task Connect()
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
                Console.WriteLine("Error!");
            }
            finally
            {
                await client.StopAsync();
                await client.DisposeAsync();
                Console.WriteLine("Cpnnection Closed");
            }
        }
    }
}