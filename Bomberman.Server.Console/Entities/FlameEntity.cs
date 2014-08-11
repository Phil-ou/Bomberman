using System;
using Bomberman.Common.DataContracts;

namespace Bomberman.Server.Console.Entities
{
    public class FlameEntity : Entity
    {
        public DateTime FadeoutTimeout { get; set; }

        public FlameEntity(int x, int y, int delayInMs)
            : base(EntityTypes.Flames, x, y)
        {
            FadeoutTimeout = DateTime.Now.AddMilliseconds(delayInMs);
        }
    }
}
