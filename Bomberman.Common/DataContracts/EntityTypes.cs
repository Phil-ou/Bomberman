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
        BonusBombRange =    0x00000400, // immediate action (can be picked more than once)
        [EnumMember]
        BonusNoClipping =   0x00000800, // state (no additional effect if picked multiple times)
        [EnumMember]
        BonusMaxBomb =      0x00001000, // immediate action (can be picked more than once)
        [EnumMember]
        BonusBombKick =     0x00002000, // state (no additional effect if picked multiple times)
        [EnumMember]
        BonusE =            0x00004000,
        [EnumMember]
        BonusF =            0x00008000,
        [EnumMember]
        BonusG =            0x00010000,
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
