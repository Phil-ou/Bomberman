using System.Runtime.Serialization;

namespace Bomberman.Common.DataContracts
{
    [DataContract]
    public class MapModification
    {
        [DataMember]
        public int X { get; set; }
        [DataMember]
        public int Y { get; set; }
        
        [DataMember]
        public EntityTypes Entity { get; set; }

        [DataMember]
        public MapModificationActions Action { get; set; }
    }
}
