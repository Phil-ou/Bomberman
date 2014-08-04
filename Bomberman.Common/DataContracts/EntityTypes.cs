using System;
using System.Runtime.Serialization;

namespace Bomberman.Common.DataContracts
{
    [DataContract]
    [Flags]
    public enum EntityTypes
    {
        [EnumMember]
        Empty = 0x0000,

        [EnumMember]
        Wall = 0x0001,
        [EnumMember]
        Dust = 0x0002,

        [EnumMember]
        Player1 = 0x0004,
        [EnumMember]
        Player2 = 0x0008,
        [EnumMember]
        Player3 = 0x0010,
        [EnumMember]
        Player4 = 0x0020,

        [EnumMember]
        BombPlayer1 = 0x0040,
        [EnumMember]
        BombPlayer2 = 0x0080,
        [EnumMember]
        BombPlayer3 = 0x0100,
        [EnumMember]
        BombPlayer4 = 0x0200,

        [EnumMember]
        BonusA = 0x0400,
        [EnumMember]
        BonusB = 0x0800,
        [EnumMember]
        BonusC = 0x1000,
        [EnumMember]
        BonusD = 0x2000,
        [EnumMember]
        BonusE = 0x4000,
        [EnumMember]
        BonusF = 0x8000,
    }
}
