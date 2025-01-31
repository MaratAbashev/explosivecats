namespace ExplosiveCatsEnums;

public enum ActionType : byte
{
    PlayCard = 0x50, // сыграть карту
    TakeCard = 0x54, // взять из колоды
    GiveCard = 0x47, // отдать карту после подлижись
    PlayNope = 0x4E, // сыграть неть
    PlayDefuse = 0x44, // сыграть обезвредь (вставить взрывного кота в колоду если имеется обезвредь)
    Ready = 0x52, //готов начать игру
    Join = 0x4a, //вошел в игру
    PlayFavor = 0x46, //подлижись
    PlayCatCard = 0x43 //сыграть кошкокарту (парные)
}