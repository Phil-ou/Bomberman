using System.Runtime.Serialization;

namespace Bomberman.Common.DataContracts
{
    [DataContract]
    public enum Directions
    {
        [EnumMember]
        Up,
        [EnumMember]
        Right,
        [EnumMember]
        Down,
        [EnumMember]
        Left
    }
}
