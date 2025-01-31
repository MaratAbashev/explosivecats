using System.Net;
using System.Net.Sockets;
using static ExplosiveCats.PackageHelper;

namespace ExplosiveCats;

public class TcpServer
{
    private readonly object _lock = new();
    private readonly Socket _socket;
    private const int MaxTimeout = 5 * 60 * 1000;
    private readonly Dictionary<Socket,Player> _clients = new();
    private Game _game;
    private bool _isGameInitialized;

    public TcpServer(IPAddress address, int port)
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _socket.Bind(new IPEndPoint(address, port));
    }

    public async Task StartAsync()
    {
        try
        {
            _socket.Listen();
            do
            {
                var cancellationToken = new CancellationTokenSource();

                cancellationToken.CancelAfter(MaxTimeout);
                cancellationToken.Token.Register(async () =>
                {
                    if (_clients.Count == 0)
                    {
                        StopAsync();
                    }
                });

                var connectionSocket = await _socket.AcceptAsync(cancellationToken.Token);
                var innerCancellationToken = new CancellationTokenSource();
                _ = Task.Run(
                    async () =>
                        await ProcessSocketConnect(connectionSocket,
                            innerCancellationToken), innerCancellationToken.Token);
            } while (true);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            StopAsync();
        }
    }

    private void StopAsync()
    {
        _socket.Close();
    }
    

    private async Task ProcessSocketConnect(Socket socket, CancellationTokenSource ctxSource)
    {
        try
        {
            socket.ReceiveTimeout = MaxTimeout;
            socket.SendTimeout = MaxTimeout;
            
            var isJoin = await TryHandleJoin(socket, ctxSource.Token);
            
            if (!isJoin)
            {
                Console.WriteLine($"К нам пытался ворваться клиент {GetRemoteIpAddress(socket)}");
                await socket.DisconnectAsync(false);
                return;
            }

            lock (_lock)
            {
                if (_clients.Count < 5)
                {
                    _clients.Add(socket,new Player
                    {
                        Id = (byte)_clients.Count,
                        IsReady = false,
                        Cards = new HashSet<Card>()
                    });
                }
            }
            byte[] message = PackageBuilder.CreateWelcomePackage(_clients[socket].Id);
            await socket.SendAsync(message, SocketFlags.None, ctxSource.Token);
            
            var playerHandler = new PlayerActionsHandler(socket);
            
            while (socket.Connected)
            {
                //подготовка к игре
                var result = await TryHandleReady(socket, ctxSource.Token);
                if (result)
                {
                    Console.WriteLine("Game is starting");
                    continue;
                }
                if (_isGameInitialized && !result)
                {
                    //обработка запросов клиента
                    playerHandler.SetGame(_game);
                    var isValidAction = await playerHandler.TryHandleGameActions(ctxSource.Token);
                    if (!isValidAction)
                    {
                        Console.WriteLine("Сервер не смог обработать запрос");
                        await socket.DisconnectAsync(false);
                    }
                }
            }
        }
        catch
        {
            await socket.DisconnectAsync(false);
        }
    }

    private string GetRemoteIpAddress(Socket socket)
    {
        return  IPAddress.Parse(((IPEndPoint)socket.RemoteEndPoint!).Address.ToString()) 
                     + ":" + ((IPEndPoint)socket.RemoteEndPoint).Port;        
    }
    
    private async Task<bool> TryHandleJoin(Socket socket, CancellationToken cancellationToken = default)
    {
        if (_clients.Count >= 5) return false;
        var buffer = new byte[5];
        var contentLength = await TryGetContentLength(socket, buffer, cancellationToken);
        if (contentLength == -1) return false;

        return IsQueryValid(buffer, contentLength) && IsJoin(buffer);
    }
    
    private async Task<int> TryGetContentLength(Socket socket, byte[] buffer,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await socket
                .ReceiveAsync(buffer, SocketFlags.None, cancellationToken);
        }
        catch (Exception e)
        {
            return -1;
        }
    }

    public async Task<bool> TryHandleReady(Socket playerSocket, CancellationToken ctx)
    {
        if (_clients[playerSocket].IsReady) return false;
        var buffer = new byte[6];
        var contentLength = await TryGetContentLength(playerSocket, buffer, ctx);
        if (contentLength == -1) return false;
        if (IsReady(buffer) && IsQueryValid(buffer, contentLength))
        {
            _clients[playerSocket].IsReady = true;
            var players = _clients.Select(pair => pair.Value).ToList();
            lock (_lock)
            {
                if (_isGameInitialized)
                {
                    return false;
                }

                if (players.All(player => player.IsReady))
                {
                    Game.InitializeClients(_clients);
                    _game = Game.GameValue;
                    _isGameInitialized = true;
                }
                else
                {
                    return false;
                }
            }

            await BroadCastMessage(GetGameStartMessage, ctx);
            return true;
        }

        return false;
    }
    
    private async Task BroadCastMessage(Func<Player, byte[]> getMessage, CancellationToken ctx)
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