using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audio900.Services
{
    public class JsonHelper
    {
        /// <summary>
        /// 将对象转换为JSON字符串
        /// </summary>
        public static string ConvertJson(object obj)
        {
            try
            {
                return JsonConvert.SerializeObject(obj, Formatting.Indented);
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "JSON序列化失败");
                return string.Empty;
            }
        }

        /// <summary>
        /// 将JSON字符串转换为对象
        /// </summary>
        public static T DeserializeJson<T>(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "JSON反序列化失败");
                return default(T);
            }
        }

        /// <summary>
        /// 将JSON字符串转换为匿名类型对象
        /// </summary>
        public static T DeserializeAnonymousType<T>(string json, T anonymousTypeObject)
        {
            try
            {
                return JsonConvert.DeserializeAnonymousType(json, anonymousTypeObject);
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "JSON反序列化匿名类型失败");
                return default(T);
            }
        }
    }
}
