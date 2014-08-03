using System.Runtime.Serialization;

namespace Bomberman.Common.DataContracts
{
    [DataContract]
    public class Location
    {
        [DataMember]
        public int X { get; set; }

        [DataMember]
        public int Y { get; set; }
    }
}
