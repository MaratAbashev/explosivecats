namespace ExplosiveCatsEnums;

public enum ServerActionType: byte
{
    PlayCard = 0x50, // сыграть карту
    TakeCard = 0x54, // взять из колоды
    GiveCard = 0x47, // отдать карту после подлижись
    PlayNope = 0x4E, // сыграть неть
    PlayDefuse = 0x44,
    StartGame = 0x53,
    Explode = 0x45
}