using ExplosiveCatsEnums;

namespace ExplosiveCats;

public class Card
{
    public CardType CardType { get; }
    public byte CardId { get; }
    
    private Card(CardType cardType, byte cardNum = 0)
    {
        CardType = cardType;
        CardId = (byte)(cardNum - (byte)cardType);
    }

    public byte ToByte()
    {
        return (byte)((byte)CardType + CardId);
    }

    private static Card CreateNoneCard()
    {
        return new Card(CardType.None);
    }
    public static Card FromByte(byte number)
    {
        if (number == 0) return CreateNoneCard(); 
        if (number >= 1 && number <= 6)
            return new Card(CardType.Defuse, number);
        if (number >= 7 && number <= 10)
            return new Card(CardType.Attack, number);
        if (number >= 11 && number <= 14)
            return new Card(CardType.Favor, number);
        if (number >= 15 && number <= 19)
            return new Card(CardType.Nope, number);
        if (number >= 20 && number <= 23)
            return new Card(CardType.Shuffle, number);
        if (number >= 24 && number <= 27)
            return new Card(CardType.Skip, number);
        if (number >= 28 && number <= 32)
            return new Card(CardType.SeeTheFuture, number);
        if (number >= 33 && number <= 36)
            return new Card(CardType.TacoCat, number);
        if (number >= 37 && number <= 40)
            return new Card(CardType.MelonCat, number);
        if (number >= 41 && number <= 44)
            return new Card(CardType.PotatoCat, number);
        if (number >= 45 && number <= 48)
            return new Card(CardType.BeardCat, number);
        if (number >= 49 && number <= 52)
            return new Card(CardType.RainbowCat, number);
        if (number >= 53 && number <= 57)
            return new Card(CardType.ExplosiveCat, number);

        throw new ArgumentOutOfRangeException(nameof(number), "Число не соответствует ни одному интервалу.");
    }
}
