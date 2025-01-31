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
            
            var playerHandler = new PlayerActionsHandler(socket);
            
            var isJoin = await playerHandler.TryHandleJoin(_clients.Count, ctxSource.Token);
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
            
            while (socket.Connected)
            {
                //подготовка к игре
                var result = await playerHandler.TryHandleReady(_clients, ctxSource.Token);
                if (result)
                {
                    Console.WriteLine("Game is starting");
                    continue;
                }
                if (playerHandler.IsGameInitialized && !result)
                {
                    //обработка запросов клиента
                    var isValidAction = await playerHandler.TryHandleGameActions(ctxSource.Token);
                    if (!isValidAction)
                    {
                        Console.WriteLine("Сервер не смог обработать запрос");
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
}