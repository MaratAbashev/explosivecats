namespace TcpChatServer;

public class Player
{
    public byte Id { get; set; }
    public byte MoveCount { get; set; }
    public HashSet<Card>? Type { get; set; }
    public bool IsReady { get; set; }
}