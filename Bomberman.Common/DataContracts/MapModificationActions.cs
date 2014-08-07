using System.Runtime.Serialization;

namespace Bomberman.Common.DataContracts
{
    [DataContract]
    public enum MapModificationActions
    {
        [EnumMember]
        Add,
        [EnumMember]
        Delete,
    }
}
