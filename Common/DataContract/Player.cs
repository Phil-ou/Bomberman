using System.Runtime.Serialization;

namespace Common.DataContract
{
    [DataContract]
    public class Player : LivingObject
    {
        [DataMember]
        public string Username { get; set; }

        public override bool Compare(LivingObject objectToCompare)
        {
            if (GetType() == objectToCompare.GetType())
                return Username == ((Player) objectToCompare).Username;
            return false;
        }
    }
}
