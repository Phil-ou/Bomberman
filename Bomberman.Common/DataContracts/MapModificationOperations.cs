using System.Runtime.Serialization;

namespace Bomberman.Common.DataContracts
{
    [DataContract]
    public enum MapModificationOperations
    {
        [EnumMember]
        Add,
        [EnumMember]
        Delete,
        [EnumMember]
        Explosion, // a bomb explosion has affected cell
    }
}
