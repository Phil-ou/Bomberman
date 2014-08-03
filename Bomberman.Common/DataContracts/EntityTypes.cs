using System.Runtime.Serialization;

namespace Bomberman.Common.DataContracts
{
    [DataContract]
    public enum EntityTypes
    {
        [EnumMember]
        Empty,

        [EnumMember]
        Wall,
        [EnumMember]
        Dust,

        [EnumMember]
        Player1,
        [EnumMember]
        Player2,
        [EnumMember]
        Player3,
        [EnumMember]
        Player4,

        [EnumMember]
        BombPlayer1,
        [EnumMember]
        BombPlayer2,
        [EnumMember]
        BombPlayer3,
        [EnumMember]
        BombPlayer4,

        [EnumMember]
        BonusA,
        [EnumMember]
        BonusB,
        [EnumMember]
        BonusC,
        [EnumMember]
        BonusD,
        [EnumMember]
        BonusE,
        [EnumMember]
        BonusF,
    }
}
