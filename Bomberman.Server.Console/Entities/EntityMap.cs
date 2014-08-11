using System.Linq;
using Bomberman.Common;
using Bomberman.Common.DataContracts;
using Bomberman.Server.Console.Interfaces;

namespace Bomberman.Server.Console.Entities
{
    public class EntityMap : IEntityMap
    {
        private EntityCell[,] _cells;

        #region IEntityMap

        public Map Map { get; private set; }

        public int Size { get; private set; }

        public void Initialize(Map map)
        {
            // Create cells from map
            Map = map;
            Size = map.Description.Size;
            _cells = new EntityCell[Size, Size];
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    EntityTypes type = map.GetEntity(x, y);
                    _cells[x, y] = new EntityCell
                        {
                            new Entity(type, x, y)
                        };
                }
        }

        public void Clear()
        {
            // Clear cells
            if (_cells != null)
                foreach (EntityCell cell in _cells)
                    cell.Clear();
        }

        public EntityCell GetCell(int x, int y)
        {
            return _cells[x, y];
        }

        public Entity GetEntity(EntityTypes type, int x, int y)
        {
            return _cells[x, y].GetEntity(type);
        }

        public void AddEntity(Entity entity)
        {
            EntityCell cell = _cells[entity.X, entity.Y];
            cell.Add(entity);
        }

        public bool RemoveEntity(Entity entity)
        {
            EntityCell cell = _cells[entity.X, entity.Y];
            return cell.Remove(entity);
        }

        public bool MoveEntity(Entity entity, int toX, int toY)
        {
            int fromX = entity.X;
            int fromY = entity.Y;
            EntityCell fromCell = _cells[fromX, fromY];
            bool entityFound = fromCell.Any(x => x == entity);
            if (entityFound)
            {
                EntityCell toCell = _cells[toX, toY];
                fromCell.Remove(entity);
                toCell.Add(entity);
                entity.X = toX;
                entity.Y = toY;
            }
            else
                Log.WriteLine(Log.LogLevels.Error, "Cell at {0},{1} doesn't contain {2}", entity.X, entity.Y, entity.Type);
            return entityFound;
        }

        // Helpers
        public void ComputeNewCoordinates(Entity entity, Directions direction, out int newX, out int newY)
        {
            int stepX, stepY;
            GetDirectionSteps(direction, out stepX, out stepY);
            newX = ComputeLocation(entity.X, stepX);
            newY = ComputeLocation(entity.Y, stepY);
        }

        public int ComputeLocation(int coord, int step)
        {
            coord = (coord + step) % Size;
            if (coord < 0)
                coord += Size;
            return coord;
        }

        public void GetDirectionSteps(Directions direction, out int stepX, out int stepY)
        {
            stepX = 0;
            stepY = 0;
            switch (direction)
            {
                case Directions.Left:
                    stepX = -1;
                    break;
                case Directions.Right:
                    stepX = +1;
                    break;
                case Directions.Up:
                    stepY = -1;
                    break;
                case Directions.Down:
                    stepY = +1;
                    break;
            }
        }

        #endregion
    }
}
