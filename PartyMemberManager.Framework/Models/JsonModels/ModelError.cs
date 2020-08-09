using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace PartyMemberManager.Framework.Models.JsonModels
{
    [DataContract]
    [Serializable]
    public class ModelError
    {
        /// <summary>
        /// 错误字段
        /// </summary>
        [JsonProperty("key")]
        [DataMember(Name = "key")]
        public string Key { get; set; }
        /// <summary>
        /// 错误字段
        /// </summary>
        [JsonProperty("message")]
        [DataMember(Name = "message")]
        public string Message { get; set; }
    }
}
