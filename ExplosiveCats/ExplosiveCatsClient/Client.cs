using System.Net.Sockets;
using ExplosiveCatsEnums;
using static ExplosiveCatsClient.PackageHelper;
namespace ExplosiveCatsClient;

public class Client
{
    private readonly Socket _clientSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    public bool Connected => _clientSocket.Connected;
    public async Task<byte> StartClient(string host, int port)
    {
        try
        {
            await _clientSocket.ConnectAsync(host, port);
            Console.WriteLine($"Connected to {host}:{port}");
            var joinPackage = CreateJoinPackage();
            await _clientSocket.SendAsync(joinPackage, SocketFlags.None);
            var result = await GetResponse();
            if (!result.IsSuccess)
            {
                Console.WriteLine(result.ErrorMessage);
                return byte.MaxValue;
            }
            //любую другую обработку засунуть
            Console.WriteLine($"Действие сервера: {result.Action}.");
            Console.WriteLine($"Айди этого игрока: {result.PlayerId}.");
            return result.PlayerId;
        }
        catch (SocketException ex)
        {
            Console.WriteLine(ex.Message);
            return byte.MaxValue;
        }
    }

    public async Task CloseClient()
    {
        await _clientSocket.DisconnectAsync(false);
        _clientSocket.Close();
    }

    public async Task<Result> GetResponse()
    {
        var buffer = new byte[MaxPacketSize];
        var contentLength = await _clientSocket.ReceiveAsync(buffer, SocketFlags.None);
        if (IsQueryValid(buffer))
        {
            var result = buffer[PackageHelper.Action] switch
            {
                (byte)ServerActionType.Welcome => Result.Success(ServerActionType.Welcome,
                    buffer[PlayerId]),
                (byte)ServerActionType.Explode => Result.Success(ServerActionType.Explode,
                    buffer[PlayerId]),
                (byte)ServerActionType.StartGame => Result.Success(ServerActionType.StartGame,
                    buffer[PlayerId], buffer[PlayersCount], MakeCardList(buffer)),
                (byte)ServerActionType.GiveCard => Result.Success(ServerActionType.GiveCard, buffer[PlayerId],
                    [Card.FromByte(buffer[CardByte])], buffer[AnotherPlayerId]),
                (byte)ServerActionType.PlayCard => Result.Success(ServerActionType.PlayCard, buffer[PlayerId], 
                    MakeCardList(buffer, contentLength)),
                (byte)ServerActionType.TakeCard => Result.Success(ServerActionType.TakeCard, buffer[PlayerId], 
                    [Card.FromByte(buffer[CardByte])]),
                (byte)ServerActionType.PlayDefuse => Result.Success(ServerActionType.PlayDefuse, buffer[PlayerId], 
                    [Card.FromByte(buffer[CardByte])]),
                (byte)ServerActionType.PlayNope => Result.Success(ServerActionType.PlayNope, buffer[PlayerId], 
                    [Card.FromByte(buffer[CardByte])]),
                //добавить на отдельные экшн тайпы про парные карты и подлижись
                _ => Result.Failure("Неизвестная команда от сервера")
            };
            return result;
        }

        return Result.Failure("Не удалось разпознать команду от сервера");
    }

    public async Task GetReady(byte playerId)
    {
        if (!Connected)
            return;
        var package = CreateReadyPackage(playerId);
        await _clientSocket.SendAsync(package, SocketFlags.None);
    }
    
    public async Task PlayCard(byte playerId, Card card)
    {
        if (!Connected)
            return;
        var package = CreatePlayCardPackage(playerId, card);
        await _clientSocket.SendAsync(package, SocketFlags.None);
    }
    
    public async Task TakeCard(byte playerId)
    {
        if (!Connected)
            return;
        var package = CreateTakeCardPackage(playerId);
        await _clientSocket.SendAsync(package, SocketFlags.None);
    }
    
    public async Task PlayNope(byte playerId, Card card)
    {
        if (!Connected || card.CardType != CardType.Nope)
            return;
        var package = CreatePlayNopePackage(playerId, card);
        await _clientSocket.SendAsync(package, SocketFlags.None);
    }
    
    public async Task GiveCard(byte playerId, Card card, byte anotherPlayerId)
    {
        if (!Connected)
            return;
        var package = CreateTakeCardPackage(playerId);
        await _clientSocket.SendAsync(package, SocketFlags.None);
    }
    
    public async Task PlayDefuse(byte userId, Card card, byte insertionIndex)
    {
        if (!Connected || card.CardType != CardType.Defuse)
            return;
        var package = CreatePlayDefusePackage(userId, card, insertionIndex);
        await _clientSocket.SendAsync(package, SocketFlags.None);
    }
    
    private static List<Card> MakeCardList(byte[] buffer, int contentLength = 14)
    {
        var cardList = new List<Card>();
        for (var i = CardByte; i < contentLength; i++)
        {
            cardList.Add(Card.FromByte(buffer[i]));
        }
        return cardList;
    }
    
}