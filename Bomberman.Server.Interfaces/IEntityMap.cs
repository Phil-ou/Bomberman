using Bomberman.Common.DataContracts;

namespace Bomberman.Server.Interfaces
{
    public interface IEntityMap
    {
        Map Map { get; }
        int Size { get; }

        void Initialize(Map map);
        void Clear();

        IEntityCell GetCell(int x, int y);
        IEntity GetEntity(EntityTypes type, int x, int y);
        void AddEntity(IEntity entity);
        bool RemoveEntity(IEntity entity);
        bool MoveEntity(IEntity entity, int toX, int toY);

        // Helpers
        void ComputeNewCoordinates(IEntity entity, Directions direction, out int newX, out int newY);
        int ComputeLocation(int coord, int step);
        void GetDirectionSteps(Directions direction, out int stepX, out int stepY);
    }
}
