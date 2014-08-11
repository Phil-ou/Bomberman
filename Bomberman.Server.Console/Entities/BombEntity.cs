using System;
using Bomberman.Common.DataContracts;
using Bomberman.Server.Console.Interfaces;

namespace Bomberman.Server.Console.Entities
{
    public class BombEntity : Entity
    {
        public IPlayer Player { get; private set; }

        public int Range { get; private set; }
        public DateTime ExplosionTimeout { get; private set; }

        public bool IsMoving { get; private set; }
        public Directions Direction { get; private set; }
        public int MoveDelay { get; private set; }
        public DateTime MoveTimeout { get; private set; }

        public BombEntity(IPlayer player, int x, int y, int range, int delayInMs)
            : base(EntityTypes.Bomb, x, y)
        {
            Player = player;
            Range = range;
            ExplosionTimeout = DateTime.Now.AddMilliseconds(delayInMs);
        }

        public void InitMove(Directions direction, int delayInMs)
        {
            MoveDelay = delayInMs;
            if (!IsMoving) // Cannot change direction if already moving
            {
                Direction = direction;
                IsMoving = true;
            }
            MoveTimeout = DateTime.Now.AddMilliseconds(delayInMs);
        }
    }
}
