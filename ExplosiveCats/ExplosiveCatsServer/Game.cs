using ExplosiveCatsEnums;

namespace ExplosiveCats;

public class Game(List<Player> players)
{
    private static readonly Lazy<Game> Instance = new
        (() => new Game(_playersInitial ?? new List<Player>()));

    private static List<Player>? _playersInitial;

    private List<Card> _deck = new();

    public static Game GameValue => Instance.Value;

    public Player? CurrentPlayer { get; private set; }
    public Card? LastDeletedExplosiveCard { get; set; }

    public Player NextPlayer
    {
        get
        {
            var playerPosition = players.IndexOf(CurrentPlayer);
            if (playerPosition == players.Count - 1)
            {
                return players[0];
            }

            return players[playerPosition + 1];
        }
    }

    public static void InitializePlayers(List<Player> playersInitial)
    {
        _playersInitial = playersInitial;
    }

    public void DistributeCards()
    {
        InitializeDeck();
        CurrentPlayer = players.FirstOrDefault(p => p.Id == 0);
    }

    public Card? GetLastCardFromDeck()
    {
        var card = _deck.LastOrDefault();
        if (card == null) return null;
        _deck.Remove(card);
        return card;
    }

    public bool IsExistDefuseInPlayerCards()
    {
        return CurrentPlayer.Cards.Any(card => card.CardType == CardType.Defuse);
    }

    public void RemovePlayer()
    {
        players.Remove(CurrentPlayer);
    }

    public void ShuffleDeck()
    {
        var random = new Random();
        _deck = _deck.OrderBy(x => random.Next()).ToList();
    }

    public List<Card> GetLastThreeCards()
    {
        if (_deck.Count < 3)
        {
            return _deck;
        }

        return _deck.Skip(_deck.Count - 3).ToList();
    }

    public void SetNextPlayer()
    {
        CurrentPlayer = NextPlayer;
    }

    public void ProcessDefuseCard(Card defuseCard, int index)
    {
        if (LastDeletedExplosiveCard == null) return;
        var defusedCardInHand = CurrentPlayer.Cards
            .FirstOrDefault(card => defuseCard.CardType == card.CardType &&
                                    defuseCard.CardId == card.CardId);
        if (defusedCardInHand == null) return;
        CurrentPlayer.Cards.Remove(defusedCardInHand);
        _deck.Insert(index, defuseCard);
    }

    public void ProcessAttackCard()
    {
        if (CurrentPlayer.MoveCount > 1)
        {
            NextPlayer.MoveCount = (byte)(CurrentPlayer.MoveCount + 2);
        }
        else
        {
            NextPlayer.MoveCount = 2;
        }
    }

    public bool ProcessManyMoveCount()
    {
        if (CurrentPlayer.MoveCount > 1)
        {
            CurrentPlayer.MoveCount--;
            return false;
        }

        NextPlayer.MoveCount = 1;
        return true;
    }

    private void InitializeDeck()
    {
        for (int i = 1; i < 53; i++)
        {
            _deck.Add(Card.FromByte((byte)i));
        }

        ShuffleDeck();
        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            var defuseCard = _deck.First(c => c.CardType == CardType.Defuse);
            player.MoveCount = 1;
            player.Cards?.Add(defuseCard);
            _deck.Remove(defuseCard);
            for (int j = 0; j < 7; j++)
            {
                var randomCard = _deck.First();
                player.Cards?.Add(randomCard);
                _deck.Remove(randomCard);
            }
        }

        var explosiveCatsNumber = Math.Min(players.Count - 1, 4);

        for (int i = 53; i < 53 + explosiveCatsNumber; i++)
        {
            _deck.Add(Card.FromByte((byte)i));
        }

        ShuffleDeck();
    }
}