using System.Net.Sockets;
using static ExplosiveCats.PackageHelper;
namespace ExplosiveCats;

public class PlayerActionsHandler(Socket playerSocket)
{
    private readonly object _gameInitializeLock = new();
    private Game? _game;
    private Dictionary<Socket,Player>? _clients;
    
    public bool IsGameInitialized { get; private set; }
    public async Task<bool> TryHandleJoin(int clientsCount, CancellationToken cancellationToken = default)
    {
        if (clientsCount >= 5) return false;
        var buffer = new byte[5];
        var contentLength = await TryGetContentLength(buffer, cancellationToken);
        if (contentLength == -1) return false;

        return IsQueryValid(buffer, contentLength) && IsJoin(buffer);
    }

    public async Task<bool> TryHandleReady(Dictionary<Socket,Player> allClients, 
        CancellationToken ctx)
    {
        if (allClients[playerSocket].IsReady) return false;
        var buffer = new byte[6];
        var contentLength = await TryGetContentLength(buffer, ctx);
        if (contentLength == -1) return false;
        if (IsReady(buffer) && IsQueryValid(buffer, contentLength))
        {
            allClients[playerSocket].IsReady = true;
            var players = allClients.Select(pair => pair.Value).ToList();
            lock (_gameInitializeLock)
            {
                if (IsGameInitialized)
                {
                    return false;
                }
                if (players.All(player => player.IsReady))
                {
                    Game.InitializePlayers(players);
                    _game = Game.GameValue;
                    _clients = allClients;
                    _game.DistributeCards();
                    IsGameInitialized = true;
                }
            }
            await BroadCastMessage(GetGameStartMessage, ctx);
            return true;
        }

        return false;
    }

    public async Task<bool> TryHandleGameActions(CancellationToken ctx)
    {
        if (_clients == null || _game == null) return false;
        if (!(_game.CurrentPlayer ?? new Player()).Equals(_clients[playerSocket])) return false;
        return true;
    }
    
    private async Task<int> TryGetContentLength(byte[] buffer,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await playerSocket
                .ReceiveAsync(buffer, SocketFlags.None, cancellationToken);
        }
        catch (Exception e)
        {
            return -1;
        }
    }
    private async Task BroadCastMessage(Func<Player,byte[]> getMessage, CancellationToken ctx)
    {
        var semaphore = new SemaphoreSlim(1);
        foreach (var (client, player) in _clients)
        {
            await semaphore.WaitAsync(ctx);
            var message = getMessage(player);
            await client.SendAsync(message, SocketFlags.None, ctx);
            semaphore.Release(1);
        }
        
        semaphore.Dispose();
    }

    private byte[] GetGameStartMessage(Player player)
    {
        return PackageBuilder
            .CreateFullPackage(player.Id, player.Cards, _clients.Count);
    }
}