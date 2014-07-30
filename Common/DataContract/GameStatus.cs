using System.Runtime.Serialization;
namespace Common.DataContract
{
    [DataContract]
    public enum GameStatus
    {
        [EnumMember]
        Created,
        [EnumMember]
        Started,
        [EnumMember]
        Paused,
        [EnumMember]
        Stopped
    }
}