using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace PartyMemberManager.Web.Core.Helpers
{
    public static class JsonHelper
    {
        /// <summary>
        /// 把对象转换为JSON字符串
        /// </summary>
        /// <param name="o">对象</param>
        /// <returns>JSON字符串</returns>
        public static string ToJson(this object o)
        {
            if (o == null)
            {
                return null;
            }
            return JsonConvert.SerializeObject(o);
        }
        /// <summary>
        /// 把Json文本转为实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public static T FromJson<T>(this string input)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(input);
            }
            catch (Exception ex)
            {
                return default(T);
            }
        }
        public static string Serialize<T>(T value)
        {
            var serializer = GetJsonSerializer<T>();
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, value);
                return Encoding.Default.GetString(stream.ToArray());
            }
        }

        public static T Deserialize<T>(string json)
        {
            var serializer = GetJsonSerializer<T>();
            var bytes = Encoding.Default.GetBytes(json);
            using (var stream = new MemoryStream(bytes))
            {
                return (T)serializer.ReadObject(stream);
            }
        }

        private static DataContractJsonSerializer GetJsonSerializer<T>()
        {
            return new DataContractJsonSerializer(typeof(T));
        }
    }
}
