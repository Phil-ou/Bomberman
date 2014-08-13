using System.Collections.Generic;
using System.Linq;
using Bomberman.Common.DataContracts;
using Bomberman.Server.Interfaces;

namespace Bomberman.Server.Entities
{
    public class EntityCell : List<IEntity>, IEntityCell
    {
        #region IEntityCell

        public IEntity GetEntity(EntityTypes type)
        {
            return this.FirstOrDefault(x => x.Type == type);
        }

        public List<IEntity> Entities
        {
            get { return this; }
        }

        #endregion
    }

}
