namespace ExplosiveCatsEnums;

public enum ActionType: byte
{
    PlayCard = 0x50,  // сыграть карту
    TakeCard = 0x54, // взять из колоды
    GiveCard = 0x47, // отдать карту после подлижись
    PlayNope = 0x4E, // сыграть неть
    PlayDefuse = 0x44, // сыграть обезвредь (вставить взрывного кота в колоду если имеется обезвредь)
    
}