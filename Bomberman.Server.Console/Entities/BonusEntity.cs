using System;
using Bomberman.Common.DataContracts;

namespace Bomberman.Server.Console.Entities
{
    public class BonusEntity : Entity
    {
        public DateTime FadeoutTimeout { get; set; }

        public BonusEntity(EntityTypes type, int x, int y, TimeSpan delayInMs)
            : base(type, x, y)
        {
            FadeoutTimeout = DateTime.Now.Add(delayInMs);
        }
    }
}
