using System.Runtime.Serialization;
namespace Common.DataContract
{
    [DataContract]
    public enum ActionType
    {
        [EnumMember]
        MoveLeft,
        [EnumMember]
        MoveRight,
        [EnumMember]
        MoveDown,
        [EnumMember]
        MoveUp,
        [EnumMember]
        DropBomb,
        [EnumMember]
        ShootBomb,
    }
}
