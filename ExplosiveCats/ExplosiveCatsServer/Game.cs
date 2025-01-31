using ExplosiveCatsEnums;

namespace ExplosiveCats;

public class Game(List<Player> players)
{
    private static readonly Lazy<Game> Instance = new 
        (() => new Game(_playersInitial ?? new List<Player>()));
    private static List<Player>? _playersInitial;
    
    private List<Card> _deck = new();
    
    public static Game GameValue => Instance.Value;
    
    public Player? CurrentPlayer{ get; set; }
    
    public static void InitializePlayers(List<Player> playersInitial)
    {
        _playersInitial = playersInitial;
    }
    
    public void DistributeCards()
    {
        InitializeDeck();
        CurrentPlayer = players.FirstOrDefault(p => p.Id == 0);
    }
    
    private void InitializeDeck()
    {
        for (int i = 1; i < 53; i++)
        {
            _deck.Add(Card.FromByte((byte)i));
        }
        var random = new Random();
        _deck = _deck.OrderBy(x => random.Next()).ToList();
        
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
        
        var random2 = new Random();
        _deck = _deck.OrderBy(x => random2.Next()).ToList();
    }
}