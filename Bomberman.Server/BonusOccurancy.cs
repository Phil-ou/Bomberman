using Bomberman.Common.DataContracts;
using Bomberman.Common.Randomizer;

namespace Bomberman.Server
{
    public class BonusOccurancy : IOccurancy<EntityTypes>
    {
        public EntityTypes Value { get; set; }
        public int Occurancy { get; set; }
    }
}
