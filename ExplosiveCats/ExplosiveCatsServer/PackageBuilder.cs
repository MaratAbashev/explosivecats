using ExplosiveCatsEnums;

namespace ExplosiveCats;
using static PackageHelper;

public class PackageBuilder
{
    private readonly byte[] _package;

    public PackageBuilder(ServerActionType serverActionType, int sizeOfContent = MaxPacketSize)
    {
        if (sizeOfContent > MaxPacketSize || sizeOfContent < MaxBasePacketBytes)
        {
            throw new ArgumentException(
                $"size of content must be less or equal {nameof(MaxPacketSize)} " +
                $"and more or equal {nameof(MaxBasePacketBytes)}.",
                nameof(sizeOfContent));
        }

        _package = new byte[sizeOfContent];
        _package[Action] = (byte)serverActionType;
        CreateBasePackage();
    }

    private void CreateBasePackage()
    {
        Array.Copy(ProtocolPackage, _package, ProtocolPackage.Length);
    }
    
    public PackageBuilder WithCard(Card card)
    {
        if (_package.Length - 1 < PlayerCard)
        {
            throw new InvalidOperationException();
        }
        _package[PlayerCard] = card.ToByte();

        return this;
    }

    public PackageBuilder WithCards(ICollection<Card> cards)
    {
        if (_package.Length < PlayersCount)
        {
            throw new InvalidOperationException();
        }
        var byteArrayCards = cards.Select(card => card.ToByte()).ToArray();
        Array.Copy(byteArrayCards, 0, _package, PlayerCard, byteArrayCards.Length);
        return this;
    }
    public PackageBuilder WithPlayerCount(int playerCount)
    {
        if (_package.Length - 1 < PlayersCount)
        {
            throw new InvalidOperationException();
        }
        _package[PlayersCount] = (byte)playerCount;
        return this;    
    }

    public PackageBuilder WithPlayerId(byte playerId)
    {
        if (_package.Length - 1 < PlayerId)
        {
            throw new InvalidOperationException();
        }
        _package[PlayerId] = playerId;
        return this;
    }
    
    public byte[] Build()
    {
        return _package;
    }

    public static byte[] CreateFullPackage(
        byte playerId, 
        HashSet<Card> cards, 
        int playersCount)
    {
        return new PackageBuilder(ServerActionType.StartGame)
            .WithPlayerId(playerId)
            .WithCards(cards)
            .WithPlayerCount(playersCount)
            .Build();
    }

    public static byte[] CreateWelcomePackage(byte playerId)
    {
        return new PackageBuilder(ServerActionType.StartGame, PlayerId+1)
            .WithPlayerId(playerId)
            .Build();
    }
}

