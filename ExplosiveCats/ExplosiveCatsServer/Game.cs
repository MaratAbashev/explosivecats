namespace TcpChatServer;

public class Game
{
    private static readonly Lazy<Game> _instance = new Lazy<Game>(() => new Game());
    public static Game GameValue => _instance.Value;
    public byte[] DistributeCards()
    {
        throw new NotImplementedException();
    }
}