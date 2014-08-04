using System.Runtime.Serialization;

namespace Bomberman.Common.DataContracts
{
    [DataContract]
    public class Map
    {
        [DataMember]
        public MapDescription Description { get; set; }

        [DataMember]
        public EntityTypes[] MapAsArray { get; set; } // EntityTypes is a flag so mutiple entity can be place on same cell
    }
}
