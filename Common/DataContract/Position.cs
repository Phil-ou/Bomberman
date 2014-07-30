using System.Runtime.Serialization;

namespace Common.DataContract
{
    [DataContract]
    public class Position
    {
        [DataMember]
        public int PositionX { get; set; }
        [DataMember]
        public int PositionY { get; set; }
    }
}
