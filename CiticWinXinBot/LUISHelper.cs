using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace LUISLibrary
{
    public class LUISHelper
    {
        public static async Task<LUISResult> MakeRequest(string message, int type)
        {
            string SubscriptionKey = "bc983f3ae6a44ca7";
            string LUISid = "c5a82f9a-02db-472";
            if (type == 1)
                LUISid = "c5a82f9a-02db-472c";
                //LUISid = "34897f32-be4a-4814-b019-e4e1d93b74ed";
            else
                LUISid = "5ed4e43a-4c26-414f";
           //34897f32-be4a-4814-b019-e4e1d93b74ed?subscription-key=bc983f3ae6a44ca7b44f73a615a4bd7a&verbose=true
            var client = new HttpClient();
            var queryString = HttpUtility.UrlEncode(message);
            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", SubscriptionKey);
            var uri = "https://api.projectoxford.ai/luis/v1/application?id=" + LUISid + "&q=" + queryString;
            var response = await client.GetAsync(uri);
            string JSON = await response.Content.ReadAsStringAsync();
            LUIS luis = JsonHelper.Deserialize<LUIS>(JSON);

            Intent intent = null;
            //Entity entity = null;
           List<Entity> entitys = null;
            if (luis.intents != null && luis.intents.Count != 0)
                intent = luis.intents.OrderByDescending(sn => sn.score).FirstOrDefault();
            //if (luis.entities != null && luis.entities.Count != 0)
            //    entity = luis.entities.OrderByDescending(sn => sn.score).FirstOrDefault();
            if (luis.entities != null && luis.entities.Count != 0)
            {
                entitys = new List<LUISLibrary.Entity>();

                entitys = luis.entities.OrderByDescending(sn => sn.score).ToList();
            }
            return new LUISResult()
            {
                LUISIntent = intent,
                // LUISEntity = entity
                LUISEntitys = entitys
            };
        }

        public static async Task<TULINGResult> MakeRequestTuling(string message,string conversationID)
        {
            //string SubscriptionKey = "bc983f3ae6a44ca7b4";
            string APIkey = "b0ff7ee98dce44abb5";
           
            var client = new HttpClient();
            var queryString = HttpUtility.UrlEncode(message);
            // Request headers
            //client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", SubscriptionKey);
            var uri = "http://www.tuling123.com/openapi/api?key=" + APIkey + "&info=" + queryString + "&userid=" + conversationID;
            var response = await client.GetAsync(uri);
            string JSON = await response.Content.ReadAsStringAsync();
            TULING tuling= JsonHelper.Deserialize<TULING>(JSON);
            return new TULINGResult()
            {
                code = tuling.code,
                // LUISEntity = entity
                text = tuling.text,
                json = JSON
            };
            //LUIS luis = JsonHelper.Deserialize<LUIS>(JSON);

            //Intent intent = null;
            ////Entity entity = null;
            //List<Entity> entitys = null;
            //if (luis.intents != null && luis.intents.Count != 0)
            //    intent = luis.intents.OrderByDescending(sn => sn.score).FirstOrDefault();
            ////if (luis.entities != null && luis.entities.Count != 0)
            ////    entity = luis.entities.OrderByDescending(sn => sn.score).FirstOrDefault();
            //if (luis.entities != null && luis.entities.Count != 0)
            //{
            //    entitys = new List<LUISLibrary.Entity>();

            //    entitys = luis.entities.OrderByDescending(sn => sn.score).ToList();
            //}
            //return new LUISResult()
            //{
            //    LUISIntent = intent,
            //    // LUISEntity = entity
            //    LUISEntitys = entitys
            //};
        }


    }


    public class TULINGResult
    {
        public string code { get; set; }
        public string text { get; set; }
        public string json { get; set; }
    }


    [DataContract]
    public class TULING
    {
        [DataMember]
        public string code { get; set; }
        [DataMember]
        public string text { get; set; }
    }
    public class LUISResult
    {
        public Intent LUISIntent { get; set; }
        //public Entity LUISEntity { get; set; }
        public List<Entity> LUISEntitys { get; set; }
    }

    [DataContract]
    public class Intent
    {
        [DataMember]
        public string intent { get; set; }
        [DataMember]
        public double score { get; set; }
    }

    [DataContract]
    public class Resolution
    {
        [DataMember]
        public string date { get; set; }
    }

    [DataContract]
    public class Entity
    {
        [DataMember]
        public string entity { get; set; }
        [DataMember]
        public string type { get; set; }
        [DataMember]
        public int startIndex { get; set; }
        [DataMember]
        public int endIndex { get; set; }
        [DataMember]
        public double score { get; set; }
        [DataMember]
        public Resolution resolution { get; set; }
    }

    [DataContract]
    public class LUIS
    {
        [DataMember]
        public string query { get; set; }
        [DataMember]
        public List<Intent> intents { get; set; }
        [DataMember]
        public List<Entity> entities { get; set; }
    }


    public class JsonHelper
    {
        /// <summary>
        /// 将JSON字符串反序列化成数据对象
        /// </summary>
        /// <typeparam name="T">数据对象类型</typeparam>
        /// <param name="json">JSON字符串</param>
        /// <returns>返回数据对象</returns>
        public static T Deserialize<T>(string json)
        {
            var _Bytes = Encoding.Unicode.GetBytes(json);
            using (MemoryStream _Stream = new MemoryStream(_Bytes))
            {
                var _Serializer = new DataContractJsonSerializer(typeof(T));
                return (T)_Serializer.ReadObject(_Stream);
            }
        }

        /// <summary>
        /// 将object序列化成JSON字符串 
        /// </summary>
        /// <param name="instance">被序列化对象</param>
        /// <returns>返回json字符串</returns>
        public static string Serialize(object instance)
        {
            using (MemoryStream _Stream = new MemoryStream())
            {
                var _Serializer = new DataContractJsonSerializer(instance.GetType());
                _Serializer.WriteObject(_Stream, instance);
                _Stream.Position = 0;
                using (StreamReader _Reader = new StreamReader(_Stream))
                { return _Reader.ReadToEnd(); }
            }
        }
    }

    public class VendingLuisHelper { }
}