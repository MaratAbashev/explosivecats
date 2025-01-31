using System.Collections;
using System.Net.Sockets;
using ExplosiveCatsEnums;
using System.Linq;
using static ExplosiveCats.PackageHelper;
namespace ExplosiveCats;

public class PlayerActionsHandler(Socket playerSocket)
{
    private readonly object _gameInitializeLock = new();
    private Game? _game;
    private Dictionary<Socket,Player>? _clients;
    
    public bool IsGameInitialized { get; private set; }
    public async Task<bool> TryHandleJoin(int clientsCount, CancellationToken cancellationToken = default)
    {
        if (clientsCount >= 5) return false;
        var buffer = new byte[5];
        var contentLength = await TryGetContentLength(buffer, cancellationToken);
        if (contentLength == -1) return false;

        return IsQueryValid(buffer, contentLength) && IsJoin(buffer);
    }

    public async Task<bool> TryHandleReady(Dictionary<Socket,Player> allClients, 
        CancellationToken ctx)
    {
        if (allClients[playerSocket].IsReady) return false;
        var buffer = new byte[6];
        var contentLength = await TryGetContentLength(buffer, ctx);
        if (contentLength == -1) return false;
        if (IsReady(buffer) && IsQueryValid(buffer, contentLength))
        {
            allClients[playerSocket].IsReady = true;
            var players = allClients.Select(pair => pair.Value).ToList();
            lock (_gameInitializeLock)
            {
                if (IsGameInitialized)
                {
                    return false;
                }
                if (players.All(player => player.IsReady))
                {
                    Game.InitializePlayers(players);
                    _game = Game.GameValue;
                    _clients = allClients;
                    _game.DistributeCards();
                    IsGameInitialized = true;
                }
            }
            await BroadCastMessage(GetGameStartMessage, ctx);
            return true;
        }

        return false;
    }

    public async Task<bool> TryHandleGameActions(CancellationToken ctx)
    {
        if (_clients == null || _game == null) return false;
        if (!(_game.CurrentPlayer ?? new Player()).Equals(_clients[playerSocket])) return false;
        var buffer = new byte[MaxPacketSize];
        var contentLength = await TryGetContentLength(buffer, ctx);
        if (contentLength == -1) return false;
        if (!IsQueryValid(buffer, contentLength)) return false;
        var action = DefineAction(buffer);
        switch (action)
        {
            case ActionType.TakeCard:
                await HandleTakeCard(ctx);
                break;
            case ActionType.PlayDefuse:
                await HandleDefuse(buffer, ctx);
                break;
            case ActionType.PlayCard:
                await HandlePlayCard(buffer, ctx);
                break;
            default:
                Console.WriteLine($"Неизвестное действие");
                return false;
        }

        return true;
    }

    private async Task HandleTakeCard(CancellationToken ctx)
    {
        var card = _game.GetLastCardFromDeck();
        if (card == null) return;
        if (card.CardType == CardType.ExplosiveCat)
        {
            if (_game.IsExistDefuseInPlayerCards())
            {
                _game.LastDeletedExplosiveCard = card;
                await BroadCastMessageWithCard(GetTakeCardExplosiveMessage, card, ctx);
            }
            else
            {
                await BroadCastMessage(GetExplodePlayerMessage, ctx);
                _game.RemovePlayer();
                await playerSocket.DisconnectAsync(false, ctx);
            }
        }
        else
        {
            _game.CurrentPlayer.Cards.Add(card);
            await BroadCastMessageWithCard(GetTakeCardMessage, card, ctx);
            var isSuccess = _game.ProcessManyMoveCount();
            if (!isSuccess) return;
            _game.SetNextPlayer();
        }
    }

    private async Task HandleDefuse(byte[] buffer, CancellationToken ctx)
    {
        var card = GetCard(buffer);
        var insertionIndex = GetExplosiveCatInsertionId(buffer);
        _game.ProcessDefuseCard(card, insertionIndex);
        await BroadCastMessage(GetPlayDefuseCardMessage, ctx);
        var isSuccess = _game.ProcessManyMoveCount();
        if (!isSuccess) return;
        _game.SetNextPlayer();
        
    }

    private async Task HandlePlayCard(byte[] buffer, CancellationToken ctx)
    {
        var card = GetCard(buffer);
        switch (card.CardType)
        {
            case CardType.Skip:
                var isSuccess = _game.ProcessManyMoveCount();
                if (!isSuccess) return;
                await BroadCastMessage(GetSkipCardMessage, ctx);
                _game.SetNextPlayer();
                break;
            case CardType.Attack:
                _game.ProcessAttackCard();
                await BroadCastMessage(GetAttackCardMessage, ctx);
                _game.SetNextPlayer();
                break;
            case CardType.Shuffle:
                _game.ShuffleDeck();
                await BroadCastMessage(GetShuffleDeckMessage, ctx);
                break;
            case CardType.SeeTheFuture:
                var seemingCards = _game.GetLastThreeCards();
                await BroadCastMessageWithCards(GetSeeTheFutureMessage, seemingCards, ctx);
                break;
        }
    }
    private async Task<int> TryGetContentLength(byte[] buffer,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await playerSocket
                .ReceiveAsync(buffer, SocketFlags.None, cancellationToken);
        }
        catch (Exception e)
        {
            return -1;
        }
    }
    private async Task BroadCastMessage(Func<Player, byte[]> getMessage, CancellationToken ctx)
    {
        var semaphore = new SemaphoreSlim(1);
        foreach (var (client, player) in _clients)
        {
            await semaphore.WaitAsync(ctx);
            var message = getMessage(player);
            await client.SendAsync(message, SocketFlags.None, ctx);
            semaphore.Release(1);
        }
        
        semaphore.Dispose();
    }

    private byte[] GetGameStartMessage(Player player)
    {
        return PackageBuilder
            .CreateFullPackage(player.Id, player.Cards, _clients.Count);
    }

    private byte[] GetExplodePlayerMessage(Player player)
    {
        return new PackageBuilder(ServerActionType.Explode, PlayerId + 1)
            .WithPlayerId(_game.CurrentPlayer.Id)
            .Build();
    }
    
    private async Task BroadCastMessageWithCard(Func<Player, Card, byte[]> getMessage, Card card, CancellationToken ctx)
    {
        var semaphore = new SemaphoreSlim(1);
        foreach (var (client, player) in _clients)
        {
            await semaphore.WaitAsync(ctx);
            var message = getMessage(player, card);
            await client.SendAsync(message, SocketFlags.None, ctx);
            semaphore.Release(1);
        }
        semaphore.Dispose();
    }
    private byte[] GetTakeCardMessage(Player player, Card card)
    {
        if (player.Equals(_game.CurrentPlayer))
        {
            return new PackageBuilder(ServerActionType.TakeCard, PlayerCard + 1)
                .WithPlayerId(player.Id)
                .WithCard(card)
                .Build();
        }
        return new PackageBuilder(ServerActionType.TakeCard, PlayerCard + 1)
            .WithPlayerId(_game.CurrentPlayer.Id)
            .WithCard(Card.FromByte((byte)CardType.None))
            .Build();
    }

    private byte[] GetTakeCardExplosiveMessage(Player player, Card card)
    {
        return new PackageBuilder(ServerActionType.TakeCard, PlayerCard + 1)
            .WithPlayerId(_game.CurrentPlayer.Id)
            .WithCard(card)
            .Build();
    }

    private byte[] GetPlayDefuseCardMessage(Player player)
    {
        return new PackageBuilder(ServerActionType.PlayDefuse, PlayerId + 1)
            .WithPlayerId(_game.CurrentPlayer.Id)
            .Build();
    }

    private byte[] GetShuffleDeckMessage(Player player)
    {
        return new PackageBuilder(ServerActionType.PlayCard, PlayerCard + 1)
            .WithPlayerId(_game.CurrentPlayer.Id)
            .WithCard(Card.FromByte((byte)CardType.Shuffle))
            .Build();
    }
    private async Task BroadCastMessageWithCards(
        Func<Player, ICollection<Card>, byte[]> getMessage, 
        ICollection<Card> cards, CancellationToken ctx)
    {
        var semaphore = new SemaphoreSlim(1);
        foreach (var (client, player) in _clients)
        {
            await semaphore.WaitAsync(ctx);
            var message = getMessage(player, cards);
            await client.SendAsync(message, SocketFlags.None, ctx);
            semaphore.Release(1);
        }
        
        semaphore.Dispose();
    }
    private byte[] GetSeeTheFutureMessage(Player player, ICollection<Card> cards)
    {
        if (player.Equals(_game.CurrentPlayer))
        {
            return new PackageBuilder(ServerActionType.PlayCard, PlayerCard + 3)
                .WithPlayerId(player.Id)
                .WithCards(cards)
                .Build();
        }

        var noneCards = Enumerable.Range(1,3).Select(i => Card.FromByte(0)).ToList();
        
        return new PackageBuilder(ServerActionType.PlayCard, PlayerCard + 3)
            .WithPlayerId(_game.CurrentPlayer.Id)
            .WithCards(noneCards)
            .Build();
    }

    private byte[] GetSkipCardMessage(Player player)
    {
        return new PackageBuilder(ServerActionType.PlayCard, PlayerCard + 1)
            .WithPlayerId(_game.CurrentPlayer.Id)
            .WithCard(Card.FromByte((byte)CardType.Skip))
            .Build();
    }

    private byte[] GetAttackCardMessage(Player player)
    {
        return new PackageBuilder(ServerActionType.PlayCard, PlayerCard + 1)
            .WithPlayerId(_game.CurrentPlayer.Id)
            .WithCard(Card.FromByte((byte)CardType.Attack))
            .Build();
    }
}