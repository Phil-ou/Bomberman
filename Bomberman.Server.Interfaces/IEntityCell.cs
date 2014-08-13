using System.Collections.Generic;
using Bomberman.Common.DataContracts;

namespace Bomberman.Server.Interfaces
{
    public interface IEntityCell
    {
        IEntity GetEntity(EntityTypes type);
        List<IEntity> Entities { get; }
    }
}
