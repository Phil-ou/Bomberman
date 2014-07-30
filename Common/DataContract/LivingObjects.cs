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

        public abstract bool Compare(LivingObject objectToCompare);
    }
}
