using System.Text;
using ExplosiveCatsEnums;

namespace TcpChatServer;

public static class PackageHelper
{
    public const int MaxPacketSize = 14;
    public const int MaxFreeBytes = MaxPacketSize;
    
    public const int PlayersCount = 4;
    public const int Action = 5;
    public const int PlayerId = 6;
    public const int CardType = 7;
    
    public static readonly byte[] BasePackage = 
    {
        0x31, 0x34, 0x38, 0x38 
    };

    public static byte[] GetContent(byte[] buffer, int contentLength) =>
        buffer.Skip(MaxFreeBytes - 1).Take(contentLength - MaxFreeBytes).ToArray();

    public static bool IsQueryValid(byte[] buffer) =>
        HasStart(buffer) && IsCorrectProtocol(buffer);

    public static bool HasStart(byte[] buffer) => 
        buffer[..3].SequenceEqual(BasePackage[..3]);
    
    public static bool IsCorrectProtocol(byte[] buffer) => 
        buffer[..3].SequenceEqual(BasePackage[..3]);
    
    
    public static byte[] CreatePackage(byte[] content, ActionType action, CardType card) => 
        new PackageBuilder(content.Length)
            .WithCommand(action)
            .WithCard(card)
            .Build();
    
    
}