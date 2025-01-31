using ExplosiveCatsEnums;

namespace ExplosiveCatsClient;

public class Result
{
    public ServerActionType Action { get; }
    public List<Card>? Cards { get; }
    public byte PlayerId { get; }
    public byte AnotherPlayerId { get; }
    public byte PlayerCount { get; }
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }

    private Result(string errorMessage)
    {
        IsSuccess = false;
        ErrorMessage = errorMessage;
    }

    private Result(ServerActionType action)
    {
        IsSuccess = true;
        Action = action;
    }

    private Result(ServerActionType action, byte playerId): this(action)
    {
        PlayerId = playerId;
    }

    private Result(ServerActionType action, byte playerId, List<Card> cards) : this(action, playerId)
    {
        Cards = cards;
    }
    
    private Result(ServerActionType action, byte playerId, byte playerCount, List<Card> cards) : this(action, playerId)
    {
        Cards = cards;
        PlayerCount = playerCount;
    }
    
    private Result(ServerActionType action, byte playerId, List<Card> cards, byte anotherPlayerId) : this(action, playerId, cards)
    {
        AnotherPlayerId = anotherPlayerId;
    }

    public static Result Success(ServerActionType action) => new(action);
    
    public static Result Failure(string errormessage) => new(errormessage);

    public static Result Success(ServerActionType action, byte playerId) => new(action, playerId);
    
    public static Result Success(ServerActionType action, byte playerId, List<Card> cards) => new(action, playerId, cards);
    
    public static Result Success(ServerActionType action, byte playerId, byte playerCount, List<Card> cards) => new(action, playerId, playerCount, cards);
    
    public static Result Success(ServerActionType action, byte playerId, List<Card> cards, byte anotherPlayerId) => new(action, playerId, cards, anotherPlayerId);
}