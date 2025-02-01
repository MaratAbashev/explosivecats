using Card = ExplosiveCatsClient.Card;

namespace ExplosivaCatsUi2;

public class Player(byte playerId)
{
    public byte PlayerId { get; } = playerId;
    public int RemainingMoves { get; set; } = 1;
    public int CardsCount { get; set; } = 8;
    public bool Exploded { get; set; } = false;
}

public class MainPlayer : Player
{
    public List<Card> Cards { get; }
    public new int CardsCount => Cards.Count;

    public MainPlayer(byte playerId, List<Card> cards): base(playerId)
    {
        Cards = [..cards];
    }
}