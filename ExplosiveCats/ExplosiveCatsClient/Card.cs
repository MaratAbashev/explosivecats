using ExplosiveCatsEnums;

namespace ExplosiveCatsClient;

public class Card
{
    public CardType CardType { get; }
    public byte CardId { get; }

    private Card(CardType cardType, byte cardNum)
    {
        CardType = cardType;
        CardId = (byte)(cardNum - (byte)cardType);
    }

    public byte ToByte()
    {
        return (byte)((byte)CardType + CardId);
    }

    public static Card FromByte(byte number)
    {
        if (number == 0)
            return new Card(CardType.None, number);
        if (number <= 6)
            return new Card(CardType.Defuse, number);
        if (number <= 10)
            return new Card(CardType.Attack, number);
        if (number <= 14)
            return new Card(CardType.Favor, number);
        if (number <= 19)
            return new Card(CardType.Nope, number);
        if (number <= 23)
            return new Card(CardType.Shuffle, number);
        if (number <= 27)
            return new Card(CardType.Skip, number);
        if (number <= 32)
            return new Card(CardType.SeeTheFuture, number);
        if (number <= 36)
            return new Card(CardType.TacoCat, number);
        if (number <= 40)
            return new Card(CardType.MelonCat, number);
        if (number <= 44)
            return new Card(CardType.PotatoCat, number);
        if (number <= 48)
            return new Card(CardType.BeardCat, number);
        if (number <= 52)
            return new Card(CardType.RainbowCat, number);
        if (number <= 57)
            return new Card(CardType.ExplosiveCat, number);

        throw new ArgumentOutOfRangeException(nameof(number), "Число не соответствует ни одному интервалу.");
    }
}