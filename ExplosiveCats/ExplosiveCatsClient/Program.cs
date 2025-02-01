using System.Net.Sockets;
using System.Text;
using ExplosiveCatsClient;
using static ExplosiveCatsClient.PackageHelper;
using PackageBuilder = ExplosiveCatsClient.PackageBuilder;
using PackageHelper = ExplosiveCatsClient.PackageHelper;

var client = new Client();
try
{
    var playerId = await client.StartClient("localhost", 5000);
    Console.ReadLine();
    await client.GetReady(playerId);
    var result = await client.GetResponse();
    Console.WriteLine($"Действие сервера:{result.Action}");
    Console.WriteLine($"Игроков всего:{result.PlayerCount}");
    Console.WriteLine("Карты игрока:" + string.Join(' ',result.Cards!.Select(card => card.CardType.ToString())));
    while (true)
    {
        result = await client.GetResponse();
        if (result.Action != 0)
        {
            Console.WriteLine($"Действие сервера:{result.Action}");
            Console.WriteLine($"Игроков всего:{result.PlayerCount}");
        }
    }
    
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
finally
{
    await client.CloseClient();
}
