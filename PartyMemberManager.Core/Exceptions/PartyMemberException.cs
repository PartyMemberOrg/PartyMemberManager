using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartyMemberManager.Core.Exceptions
{
    [Serializable]
    public class PartyMemberException:Exception
    {
        public string Key { get; set; }
        public PartyMemberException(string message) : base(message)
        {
            Key = string.Empty;
        }

        public PartyMemberException(string message, Exception innerException) : base(message, innerException)
        {
            Key = string.Empty;
        }
        public PartyMemberException(string key,string message) : base(message)
        {
            Key = key;
        }

        public PartyMemberException(string key,string message, Exception innerException) : base(message, innerException)
        {
            Key = key;
        }

        protected PartyMemberException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
        {
            throw new NotImplementedException();
        }
    }
}
