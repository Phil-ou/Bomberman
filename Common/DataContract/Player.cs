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
            return Username == ((Player) objectToCompare).Username;
        }
    }
}
