using System.Runtime.Serialization;

namespace Common.DataContract
{
    [DataContract]
    public class Wall : LivingObject
    {
        [DataMember]
        public string Filename { get; set; }
        [DataMember]
        public WallType WallType { get; set; }

        public override bool Compare(LivingObject objectToCompare)
        {
            return WallType == ((Wall) objectToCompare).WallType;
        }
    }
    [DataContract]
    public enum WallType
    {
        [EnumMember]
        Destructible,
        [EnumMember]
        Undestructible
    }
}
