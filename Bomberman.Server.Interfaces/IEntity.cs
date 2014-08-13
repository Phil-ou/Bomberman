using Bomberman.Common.DataContracts;

namespace Bomberman.Server.Interfaces
{
    public interface IEntity
    {
        EntityTypes Type { get; set; }
        int X { get; set; } // coordinates are stored to speed-up cell search from entity
        int Y { get; set; }

        bool IsPlayer { get; }
        bool IsFlames { get; }
        bool IsEmpty { get; }
        bool IsDust { get; }
        bool IsBomb { get; }
        bool IsWall { get; }
        bool IsBonus { get; }
    }
}
