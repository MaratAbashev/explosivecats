using System.Net;
using System.Net.Sockets;
using System.Text;
using static TcpChatServer.PackageHelper;

namespace TcpChatServer;

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
            } while (_clients.Count != 0);
        }
        catch (TaskCanceledException tcex)
        {
            Console.WriteLine(tcex.Message);
            //ignored
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
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
            
            var buffer = new byte[MaxPacketSize];
            var contentLength = await socket.ReceiveAsync(buffer, SocketFlags.None, ctxSource.Token);
            
            if (IsQueryValid(buffer) && IsJoin(buffer[PackageHelper.Action]))
            {
                _clients.Add(socket, new Player
                {
                    Id = (byte)_clients.Count,
                    IsReady = false,
                    MoveCount = 0,
                });
            }
            else
            {
                Console.WriteLine($"К нам пытался ворваться клиент {GetRemoteIpAddress(socket)}");
                _clients.Remove(socket);
                await socket.DisconnectAsync(false);
            }

            while (socket.Connected)
            {
                //подготовка к игре
                buffer = new byte[MaxPacketSize];
                await socket.ReceiveAsync(buffer, SocketFlags.None, ctxSource.Token);
                if (IsReady(buffer[PackageHelper.Action]))
                {
                    _clients[socket].IsReady = true;
                    if (_clients.Select(pair => pair.Value).All(player => player.IsReady))
                    {
                        _game = Game.GameValue;
                        byte[] message = _game.DistributeCards();
                        await BroadcastMessageAsync(message, ctxSource.Token);
                    }
                }

                if (_game != null)
                {
                    //обработка запросов клиента
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
    
    private async Task BroadcastMessageAsync(byte[] message, 
        CancellationToken ctx)
    {
        var semaphore = new SemaphoreSlim(1);
        foreach (var client in _clients.Keys)
        {
            await semaphore.WaitAsync(ctx);
            await client.SendAsync(message, SocketFlags.None, ctx);
            semaphore.Release(1);
        }

        semaphore.Dispose();
    }
}