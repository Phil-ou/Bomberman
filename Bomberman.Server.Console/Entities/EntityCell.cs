using System.Collections.Generic;
using System.Linq;
using Bomberman.Common.DataContracts;

namespace Bomberman.Server.Console.Entities
{
    public class EntityCell : List<Entity>
    {
        public Entity GetEntity(EntityTypes type)
        {
            return this.FirstOrDefault(x => x.Type == type);
        }
    }

}
