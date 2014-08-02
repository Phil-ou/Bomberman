using System.Runtime.Serialization;

namespace Common.DataContract
{
    [DataContract]
    public class Player : LivingObject
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public int Score { get; set; }
        [DataMember]
        public bool IsCreator { get; set; }


        public bool CompareId(LivingObject objectToCompare)
        {
            if (GetType() == objectToCompare.GetType())
                return Id == ((Player) objectToCompare).Id;
            return false;
        }
    }
}
