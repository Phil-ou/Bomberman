using System;
using Bomberman.Common.DataContracts;

namespace Bomberman.Server.Entities
{
    public class FlameEntity : Entity
    {
        public DateTime FadeoutTimeout { get; set; }

        public FlameEntity(int x, int y, TimeSpan delayInMs)
            : base(EntityTypes.Flames, x, y)
        {
            FadeoutTimeout = DateTime.Now.Add(delayInMs);
        }
    }
}
