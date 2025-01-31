using ExplosiveCatsEnums;
using TcpChatServer;

namespace ExplosiveCatsClient;
using static PackageHelper;

public class PackageBuilder
{
    private readonly byte[] _package;

    public PackageBuilder(int sizeOfContent)
    {
        if (sizeOfContent > MaxPacketSize)
        {
            throw new ArgumentException(
                $"size of content must be less or equal {nameof(MaxPacketSize)}",
                nameof(sizeOfContent));
        }

        _package = new byte[MaxFreeBytes + sizeOfContent];
        CreateBasePackage();
    }

    private void CreateBasePackage()
    {
        Array.Copy(BasePackage, _package, BasePackage.Length);
    }

    public PackageBuilder WithCommand(ActionType command)
    {
        _package[Action] = (byte)command;
        return this;
    }
    
    public PackageBuilder WithCard(Card card)
    {
        _package[CardByte] = card.ToByte();
        return this;
    }

    public PackageBuilder WithExplosiveCatInsertionId(byte insertionId)
    {
        _package[ExplosiveCatInsertionId] = insertionId;
        return this;
    }

    public PackageBuilder WithPlayerId(byte playerId)
    {
        _package[PlayerId] = playerId;
        return this;
    }

    public PackageBuilder WithAnotherPlayerId(byte anotherPlayerId)
    {
        _package[AnotherPlayerId] = anotherPlayerId;
        return this;
    }

    public byte[] Build()
    {
        return _package;
    }
}

