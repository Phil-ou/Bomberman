using System.Runtime.Serialization;

namespace Common.DataContract
{
    [DataContract]
    public class Bonus : LivingObject
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
    }
}
