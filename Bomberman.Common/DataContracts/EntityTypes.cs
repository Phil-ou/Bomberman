using System;
using System.Runtime.Serialization;

namespace Bomberman.Common.DataContracts
{
    [DataContract]
    [Flags]
    public enum EntityTypes
    {
        [EnumMember]
        Empty =             0x00000000,

        [EnumMember]
        Wall =              0x00000001,
        [EnumMember]
        Dust =              0x00000002,
        [EnumMember]
        Bomb =              0x00000004,

        [EnumMember]
        Player1 =           0x00000008,
        [EnumMember]
        Player2 =           0x00000010,
        [EnumMember]
        Player3 =           0x00000020,
        [EnumMember]
        Player4 =           0x00000040,

        [EnumMember]
        BonusFireUp =       0x00000400, // Increase bomb explosion range (max 10)
        [EnumMember]
        BonusNoClipping =   0x00000800, // No clipping, can pass thru every obstables
        [EnumMember]
        BonusBombUp =       0x00001000, // Increase simultaneous bomb drop
        [EnumMember]
        BonusBombKick =     0x00002000, // Can kick bomb
        [EnumMember]
        BonusFlameBomb =    0x00004000, // Bomb leave flames during x seconds
        [EnumMember]
        BonusFireDown =     0x00008000, // Decrease bomb explision range (min 1)
        [EnumMember]
        BonusBombDown =     0x00010000, // Decrease simultaneous bomb drop (min 1)
        [EnumMember]
        BonusH =            0x00020000,
        [EnumMember]
        BonusI =            0x00040000,
        [EnumMember]
        BonusJ =            0x00080000,

        [EnumMember]
        Flames =            0x10000000
    }
}
