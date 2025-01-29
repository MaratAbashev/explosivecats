using ExplosiveCatsEnums;

namespace TcpChatServer;
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

    public PackageBuilder WithCard(CardType card)
    {
        _package[CardType] = (byte)card;

        return this;
    }

    public byte[] Build()
    {
        return _package;
    }
}

