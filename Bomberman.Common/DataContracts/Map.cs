using System.Runtime.Serialization;

namespace Bomberman.Common.DataContracts
{
    [DataContract]
    public class Map
    {
        [DataMember]
        public MapDescription Description { get; set; }

        [DataMember]
        public EntityTypes[] MapAsArray { get; set; } // EntityTypes is a flag so multiple entities can be placed on same cell

        //
        public EntityTypes GetEntity(int index)
        {
            return MapAsArray[index];
        }

        public EntityTypes GetEntity(int x, int y)
        {
            int index = GetIndex(x, y);
            return GetEntity(index);
        }

        public void TransformEntity(int index, EntityTypes oldEntity, EntityTypes newEntity)
        {
            DeleteEntity(index, oldEntity);
            AddEntity(index, newEntity);
        }

        public void TransformEntity(int x, int y, EntityTypes oldEntity, EntityTypes newEntity)
        {
            int index = GetIndex(x, y);
            TransformEntity(index, oldEntity, newEntity);
        }

        public void AddEntity(int index, EntityTypes entity)
        {
            MapAsArray[index] |= entity;
        }

        public void AddEntity(int x, int y, EntityTypes entity)
        {
            int index = GetIndex(x, y);
            AddEntity(index, entity);
        }

        public void DeleteEntity(int index, EntityTypes entity)
        {
            MapAsArray[index] &= ~entity;
        }

        public void DeleteEntity(int x, int y, EntityTypes entity)
        {
            int index = GetIndex(x, y);
            DeleteEntity(index, entity);
        }

        public int GetIndex(int x, int y)
        {
            return x + y * Description.Size;
        }

        public void GetLocation(int index, out int x, out int y)
        {
            x = index % Description.Size;
            y = index / Description.Size;
        }
    }
}
