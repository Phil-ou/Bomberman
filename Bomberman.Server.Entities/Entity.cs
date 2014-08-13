using Bomberman.Common.DataContracts;
using Bomberman.Server.Interfaces;

namespace Bomberman.Server.Entities
{
    public class Entity : IEntity
    {
        #region IEntity

        public EntityTypes Type { get; set; }
        public int X { get; set; } // coordinates are stored to speed-up cell search from entity
        public int Y { get; set; }

        public bool IsPlayer
        {
            get
            {
                return Type == EntityTypes.Player1
                       || Type == EntityTypes.Player2
                       || Type == EntityTypes.Player3
                       || Type == EntityTypes.Player4;
            }
        }

        public bool IsFlames
        {
            get { return Type == EntityTypes.Flames; }
        }

        public bool IsEmpty
        {
            get { return Type == EntityTypes.Empty; }
        }

        public bool IsDust
        {
            get { return Type == EntityTypes.Dust; }
        }

        public bool IsBomb
        {
            get { return Type == EntityTypes.Bomb; }
        }

        public bool IsWall
        {
            get { return Type == EntityTypes.Wall; }
        }

        public bool IsBonus
        {
            get
            {
                return Type == EntityTypes.BonusFireUp
                       || Type == EntityTypes.BonusNoClipping
                       || Type == EntityTypes.BonusBombUp
                       || Type == EntityTypes.BonusBombKick
                       || Type == EntityTypes.BonusFlameBomb
                       || Type == EntityTypes.BonusFireDown
                       || Type == EntityTypes.BonusBombDown
                       || Type == EntityTypes.BonusH
                       || Type == EntityTypes.BonusI
                       || Type == EntityTypes.BonusJ;
            }
        }

        #endregion

        public Entity(EntityTypes type, int x, int y)
        {
            Type = type;
            X = x;
            Y = y;
        }
    }
}
