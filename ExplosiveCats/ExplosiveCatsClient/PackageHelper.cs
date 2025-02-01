using ExplosiveCatsEnums;

namespace ExplosiveCatsClient;

public static class PackageHelper
{
    public const int MaxPacketSize = 15;
    public const int MaxBasePacketBytes = 5;
    public const int MaxFreeBytes = MaxPacketSize - MaxBasePacketBytes;

    public const int Action = 4;
    public const int PlayerId = 5;
    public const int CardByte = 6;
    public const int ExplosiveCatInsertionId = 7;
    public const int AnotherPlayerId = 7;
    public const int SecondCardType = 7;
    public const int PlayersCount = 14;

    public static readonly byte[] BasePackage =
    {
        0x31, 0x34, 0x38, 0x38
    };

    public static bool IsQueryValid(byte[] buffer) =>
        IsCorrectProtocol(buffer) && HasAction(buffer);

    public static bool IsCorrectProtocol(byte[] buffer) =>
        buffer[..4].SequenceEqual(BasePackage);

    public static bool HasAction(byte[] buffer) =>
        Enum.GetValues<ServerActionType>().Any(a => (byte)a == buffer[Action]);
    
    public static byte[] CreateJoinPackage() => 
        new PackageBuilder(MaxBasePacketBytes)
            .WithCommand(ActionType.Join)
            .Build();

    public static byte[] CreateReadyPackage(byte playerId) =>
        new PackageBuilder(MaxBasePacketBytes + 1)
            .WithCommand(ActionType.Ready)
            .WithPlayerId(playerId)
            .Build();
    
    public static byte[] CreatePlayCardPackage(byte playerId, Card card) =>
        new PackageBuilder(MaxBasePacketBytes + 2)
            .WithCommand(ActionType.PlayCard)
            .WithPlayerId(playerId)
            .WithCard(card)
            .Build();
    public static byte[] CreateTakeCardPackage(byte playerId) =>
        new PackageBuilder(MaxBasePacketBytes + 1)
            .WithCommand(ActionType.TakeCard)
            .WithPlayerId(playerId)
            .Build();

    public static byte[] CreateGiveCardPackage(byte playerId, Card card, byte anotherplayerId) =>
        new PackageBuilder(MaxBasePacketBytes + 3)
            .WithCommand(ActionType.GiveCard)
            .WithPlayerId(playerId)
            .WithCard(card)
            .WithAnotherPlayerId(anotherplayerId)
            .Build();

    public static byte[] CreatePlayDefusePackage(byte playerId,Card card, byte insertionId) =>
        new PackageBuilder(MaxBasePacketBytes + 3)
            .WithCommand(ActionType.PlayDefuse)
            .WithPlayerId(playerId)
            .WithCard(card)
            .WithExplosiveCatInsertionId(insertionId)
            .Build();
    
    public static byte[] CreatePlayNopePackage(byte playerId, Card card) =>
        new PackageBuilder(MaxBasePacketBytes + 2)
            .WithCommand(ActionType.PlayNope)
            .WithPlayerId(playerId)
            .WithCard(card)
            .Build();
}