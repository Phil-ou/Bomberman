using System.Runtime.Serialization;

namespace Bomberman.Common.DataContracts
{
    [DataContract]
    public enum LoginResults
    {
        [EnumMember]
        Successful,
        [EnumMember]
        FailedInvalidName,
        [EnumMember]
        FailedTooManyPlayers,
        [EnumMember]
        FailedDuplicateName
    }
}
