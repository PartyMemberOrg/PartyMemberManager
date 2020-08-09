using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace PartyMemberManager.Framework.Models.JsonModels
{
    /// <summary>
    /// Json结果(专用于layui的表格组件）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DataContract]
    [Serializable]
    public class JsonResultDatasModel<T>
    {
        /// <summary>
        /// 状态代码，0表示成功,非0表示失败
        /// </summary>
        [JsonProperty("code")]
        [DataMember(Name = "code")]
        public int Code { get; set; } = 0;
        /// <summary>
        /// 是否成功
        /// </summary>
        [JsonProperty("msg")]
        [DataMember(Name = "msg")]
        public string Msg { get; set; } = "成功";
        /// <summary>
        /// 数据个数
        /// </summary>
        [JsonProperty("count")]
        [DataMember(Name = "count")]
        public int Count { get; set; }
        /// <summary>
        /// 数据
        /// </summary>
        [JsonProperty("data")]
        [DataMember(Name = "data")]
        public IEnumerable<T> Data { get; set; }
    }
}
