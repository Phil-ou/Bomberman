using System.Runtime.Serialization;

namespace Bomberman.Common.DataContracts
{
    [DataContract]
    public class MapDescription
    {
        [DataMember] 
        public int Id { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public int Size { get; set; }
    }
}
