using System.Runtime.Serialization;

namespace Common.DataContract
{
    [DataContract]
    [KnownType(typeof(Player))]
    [KnownType(typeof(LivingObject))]
    [KnownType(typeof(Position))]   
    public abstract class LivingObject
    {
        [DataMember]
        public Position ObjectPosition { get; set; }

        public bool ComparePosition(LivingObject objectToCompare)
        {
            return ObjectPosition.PositionX == objectToCompare.ObjectPosition.PositionX &&
                 ObjectPosition.PositionY == objectToCompare.ObjectPosition.PositionY;
        }
    }
}
