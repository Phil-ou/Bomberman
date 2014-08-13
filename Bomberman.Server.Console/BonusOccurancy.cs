using Bomberman.Common.DataContracts;
using Bomberman.Common.Randomizer;

namespace Bomberman.Server.Console
{
    public class BonusOccurancy : IOccurancy<EntityTypes>
    {
        public EntityTypes Value { get; set; }
        public int Occurancy { get; set; }
    }
}
