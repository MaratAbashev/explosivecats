using ExplosiveCatsClient;
using ExplosiveCatsEnums;

namespace ExplosiveCatsUi;

public class Game
{
    public List<Player> Players { get; }
    public Player CurrentPlayer { get; private set; }
    public int DeckCount { get; private set; }
    private Stack<GameAction> _actionHistory = new Stack<GameAction>();

    public Game(List<Player> players)
    {
        Players = players;
        CurrentPlayer = GetNextActivePlayer(Players[0]);
        DeckCount = 56 - 8 * Players.Count - (5 - Players.Count);
    }

    public void ProcessCardPlay(Card card, byte playerId)
    {
        if (Players[playerId] != CurrentPlayer) return;

        var action = new GameAction {
            Player = CurrentPlayer,
            Card = card,
            PreviousRemainingMoves = CurrentPlayer.RemainingMoves
        };

        switch (card.CardType)
        {
            case CardType.Skip:
                HandleSkip(card);
                break;
            
            case CardType.Attack:
                HandleAttack(card);
                break;
            
            case CardType.Nope when _actionHistory.Any():
                HandleNope(card);
                return; // Отмена действия
            
            default:
                HandleRegularCard(card);
                break;
        }

        _actionHistory.Push(action);
    }

    public void TakeCard(Card card)
    {
        AddCard(card, CurrentPlayer);
        CurrentPlayer.RemainingMoves--;
        DeckCount--;
        MoveToNextPlayer();
    }
    
    public void GiveCard(byte senderId, Card card, byte recieverId)
    {
        RemoveCard(card, Players[senderId]);
        AddCard(card, Players[recieverId]);
    }

    public void PlayDefuse(Card card)
    {
        if (card.CardType != CardType.Defuse)
            return;
        RemoveCard(card, CurrentPlayer);
        MoveToNextPlayer();
    }

    private void HandleSkip(Card card)
    {
        CurrentPlayer.RemainingMoves--;
        RemoveCard(card, CurrentPlayer);
        if (CurrentPlayer.RemainingMoves <= 0)
        {
            MoveToNextPlayer();
        }
    }

    private void RemoveCard(Card card, Player sourcePlayer)
    {
        if (sourcePlayer is MainPlayer mainPlayer)
        {
            mainPlayer.Cards.Remove(card);
        }
        else
        {
            sourcePlayer.CardsCount--;
        }
    }

    private void HandleAttack(Card card)
    {
        var nextPlayer = GetNextActivePlayer(CurrentPlayer);
        nextPlayer.RemainingMoves = CurrentPlayer.RemainingMoves > 1 
            ? CurrentPlayer.RemainingMoves + 2 
            : 2;
        RemoveCard(card, CurrentPlayer);
        CurrentPlayer.RemainingMoves = 0;
        MoveToNextPlayer();
    }

    private void HandleNope(Card card)
    {
        RemoveCard(card, CurrentPlayer);
        var lastAction = _actionHistory.Pop();
        lastAction.Player.RemainingMoves = lastAction.PreviousRemainingMoves;
        CurrentPlayer = lastAction.Player;
    }

    private void AddCard(Card card, Player sourcePlayer)
    {
        if (sourcePlayer is MainPlayer mainPlayer)
        {
            mainPlayer.Cards.Add(card);
        }
        else
        {
            sourcePlayer.CardsCount++;
        }
    }

    private void HandleRegularCard(Card card)
    {
        RemoveCard(card, CurrentPlayer);
    }

    private void MoveToNextPlayer()
    {
        do
        {
            CurrentPlayer = GetNextActivePlayer(CurrentPlayer);
            CurrentPlayer.RemainingMoves = CurrentPlayer.RemainingMoves > 0 
                ? CurrentPlayer.RemainingMoves 
                : 1;
        } 
        while (CurrentPlayer.Exploded);
    }

    private Player GetNextActivePlayer(Player current)
    {
        var nextIndex = (Players.IndexOf(current) + 1) % Players.Count;
        return Players[nextIndex];
    }
}

public class GameAction
{
    public Player Player { get; set; }
    public Card Card { get; set; }
    public int PreviousRemainingMoves { get; set; }
}