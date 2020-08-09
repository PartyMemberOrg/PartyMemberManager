﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace PartyMemberManager.Framework.Models.JsonModels
{
    /// <summary>
    /// Json结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DataContract]
    [Serializable]
    public class JsonResultModel<T>
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
        [JsonProperty("message")]
        [DataMember(Name = "message")]
        public string Message { get; set; } = "成功";
        /// <summary>
        /// 数据
        /// </summary>
        [JsonProperty("datas")]
        [DataMember(Name = "datas")]
        public IEnumerable<T> Datas { get; set; }
    }
}
