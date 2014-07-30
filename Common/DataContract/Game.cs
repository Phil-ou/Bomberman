using System.Runtime.Serialization;

namespace Common.DataContract
{
    [DataContract]
    [KnownType(typeof(Wall))]
    [KnownType(typeof(Map))]
    [KnownType(typeof(GameStatus))]
    [KnownType(typeof(Player))]
    [KnownType(typeof(LivingObject))]
    [KnownType(typeof(Bonus))]
    [KnownType(typeof(Position))]    
    public class Game
    {
        [DataMember]
        public Map Map { get; set; }
        [DataMember]
        public GameStatus CurrentStatus { get; set; }
    }
}
