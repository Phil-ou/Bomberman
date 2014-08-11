using System.Runtime.Serialization;

namespace Bomberman.Common.DataContracts
{
    [DataContract]
    public enum PlaceBombResults
    {
        [EnumMember]
        Successful,
        [EnumMember]
        TooManyBombs,
    }
}
