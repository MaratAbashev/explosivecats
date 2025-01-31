using ExplosiveCatsEnums;
using ActionType = ExplosiveCatsEnums.ActionType;
namespace ExplosiveCats;

public static class PackageHelper
{
    public const int MaxPacketSize = 15;
    public const int MaxBasePacketBytes = 5;
    public const int MaxFreeBytes = MaxPacketSize - MaxBasePacketBytes;
    
    public const int Action = 4;
    public const int PlayerId = 5;
    public const int PlayerCard = 6;
    public const int ExplosiveCatInsertionId = 6;
    public const int AnotherPlayerId = 7;
    public const int SecondCardType = 7;
    public const int PlayersCount = 14;
    
    public static readonly byte[] ProtocolPackage = 
    {
        0x31, 0x34, 0x38, 0x38 
    };

    public static bool IsQueryValid(byte[] buffer, int contentLength) =>
        contentLength >= MaxBasePacketBytes &&
        contentLength <= MaxPacketSize &&
        IsCorrectAction(buffer) && 
        IsCorrectProtocol(buffer);

    public static ActionType DefineAction(byte[] buffer)
    {
        return (ActionType)buffer[Action];
    }
    public static bool HasPlayerId(byte[] buffer) =>
        buffer.Length > PlayerId;
    
    public static bool HasCardType(byte[] buffer) =>
        buffer.Length > PlayerCard;
    
    public static bool HasAnotherPlayerId(byte[] buffer) =>
        buffer.Length > AnotherPlayerId;
    
    public static bool IsJoin(byte[] buffer) =>
        buffer[Action] == (byte)ActionType.Join;
    
    public static bool IsReady(byte[] buffer) =>
        buffer[Action] == (byte)ActionType.Ready;
    
    public static bool IsPlayCard(byte[] buffer) =>
        buffer[Action] == (byte)ActionType.PlayCard;
    
    public static bool IsTakeCard(byte[] buffer) =>
        buffer[Action] == (byte)ActionType.TakeCard;
    
    public static Card GetCard(byte[] buffer) =>
        Card.FromByte(buffer[PlayerCard]);
    
    public static int GetExplosiveCatInsertionId(byte[] buffer) =>
        buffer[ExplosiveCatInsertionId];
    
    private static bool IsCorrectProtocol(byte[] buffer) => 
        buffer[..3].SequenceEqual(ProtocolPackage[..3]);
    
    private static bool IsCorrectAction(byte[] buffer)
    {
        var hasAction = buffer.Length >= MaxBasePacketBytes;
        if (!hasAction) return false;
        var action = buffer[Action];
        var isClientAction = Enum
            .GetValues<ActionType>()
            .Any(a => action == (byte)a);
        return isClientAction;
    } 
    
}