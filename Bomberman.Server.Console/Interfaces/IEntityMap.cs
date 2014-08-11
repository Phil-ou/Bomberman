using Bomberman.Common.DataContracts;
using Bomberman.Server.Console.Entities;

namespace Bomberman.Server.Console.Interfaces
{
    public interface IEntityMap
    {
        Map Map { get; }
        int Size { get; }

        void Initialize(Map map);
        void Clear();

        EntityCell GetCell(int x, int y);
        Entity GetEntity(EntityTypes type, int x, int y);
        void AddEntity(Entity entity);
        bool RemoveEntity(Entity entity);
        bool MoveEntity(Entity entity, int toX, int toY);

        // Helpers
        void ComputeNewCoordinates(Entity entity, Directions direction, out int newX, out int newY);
        int ComputeLocation(int coord, int step);
        void GetDirectionSteps(Directions direction, out int stepX, out int stepY);
    }
}
