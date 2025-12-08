using Params_OUMIT_;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Audio900.Services
{
    internal class PostMes
    {
        public PostMes()
        {
            _httpClient = new HttpClient();
        }

        private static PostMes mesInstance;
        private static object locker_mesInstance = new object();
        private HttpClient _httpClient;
        private string _receiveMessage = "";
        private double _postTimeOut = 3000;
        private object locker_post = new object();
        private object lock1 = new object();
        private static object locker1 = new object();
        private static PostMes instace1;
        string url = "http://10.32.16.50:8080/api/File/UploadFile";

        /// <summary>
        /// 接收到消息时发生
        /// </summary>
        public event Action<string> ReceiveMessageEvent;
        /// <summary>
        /// 上传错误时发生
        /// </summary>
        public event Action<string> PostErrorEvent;
        public static PostMes CreateInstance()
        {
            lock (locker1)
            {
                if (instace1 == null)
                {
                    instace1 = new PostMes();
                }
                return instace1;
            }
        }
        public static PostMes GetMesInstance
        {
            get
            {
                lock (locker_mesInstance)
                {
                    if (mesInstance == null)
                    {
                        mesInstance = new PostMes();
                    }
                }
                return mesInstance;
            }
        }

        /// <summary>
        /// MesCheckSN
        /// </summary>
        /// <param name="sn"></param>
        /// <returns></returns>
        public string PostCheckSN(string sn)
        {
            MesPostTestData _mesCheckSN = new MesPostTestData();
            _mesCheckSN.serial_Number = sn;
            string json = JsonHelper.ConvertJson(_mesCheckSN);
            string meslog = DateTime.Now.ToString("HH:mm:ss") + $"向Mes发送CheckSN {Params.Instance.getMESDataURL}:  " + "\r\n" + json + "\r\n";
            Post(Params.Instance.getMESDataURL, json);
            meslog = DateTime.Now.ToString("HH:mm:ss") + "接收到Mes:  " + "\r\n" + _receiveMessage + "\r\n";
            return _receiveMessage;
        }

        /// <summary>
        /// MES上传数据
        /// </summary>
        /// <param name="sn"></param>
        /// <returns></returns>
        public string PostTestData(string sn, DateTime startTime, DateTime endTime, bool pass, List<string> dysNumber)
        {
            MesPostTestData _passStation = new MesPostTestData();
            _passStation.serial_Number = sn;
            _passStation.toolingNo = "";
            string testData = "test_result=" + (pass ? "PASS" : "FAIL") +
                              "$unit_sn=" + sn +
                              "$uut_start=" + startTime.ToString("yyyy-MM-dd HH:mm:ss") +
                              "$uut_stop=" + endTime.ToString("yyyy-MM-dd HH:mm:ss") +
                              "$limits_version=" +
                              "$software_name=" +
                              "$software_version=" + "MachineVer" +
                              "$station_id=" + "Params.MachineStation" +
                              "$fixture_id=" + "UC";
            _passStation.testData = testData;

            string json = JsonHelper.ConvertJson(_passStation);
            string meslog = DateTime.Now.ToString("HH:mm:ss") + $"向Mes发送过站 {"getMESTestDataURL"}:  " + "\r\n" + json + "\r\n";
            Post(url, json);
            meslog = DateTime.Now.ToString("HH:mm:ss") + "接收到Mes:  " + "\r\n" + _receiveMessage + "\r\n";
            return _receiveMessage;
        }
        /// <summary>
        /// MES上传图片
        /// </summary>
        /// <returns></returns>
        public string PostMesPic(string sn, string SS)
        {
            {
                System.IO.FileStream fs;
                fs = new System.IO.FileStream(SS, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite, FileShare.ReadWrite);
                byte[] temp = new byte[fs.Length];
                fs.Seek(0, SeekOrigin.Begin);
                fs.Read(temp, 0, temp.Length);
                string h = Convert.ToBase64String(temp);
                var obj = new
                {
                    terminal_Name = "MachineStation",
                    serial_Number = sn,
                    fileName = SS.Substring(SS.LastIndexOf("\\") + 1),
                    equipmentName = "station",
                    bytes = h
                };

                fs.Close();
                string json = JsonHelper.ConvertJson(obj);
                Post("upPicURL", json);
                return _receiveMessage;
            }
        }

        /// <summary>
        /// post方式上传
        /// </summary>
        /// <param name="url">链接</param>
        /// <param name="data">json格式的内容</param>
        public void Post(string url, string data)
        {
            try
            {
                var content = new StringContent(data, Encoding.UTF8, "application/json");
                HttpResponseMessage res = _httpClient.PostAsync(url, content).Result;
                _receiveMessage = res.Content.ReadAsStringAsync().Result;
                ReceiveMessageEvent?.Invoke(_receiveMessage);
            }
            catch (Exception postErr)
            {
                MesResult mesResult = new MesResult();
                mesResult.Result = false;
                mesResult.RetMsg = postErr.Message;
                _receiveMessage = JsonHelper.ConvertJson(mesResult);
                ReceiveMessageEvent?.Invoke(_receiveMessage);
            }
        }

        public class MesPostTestData
        {
            public string empNo = "";
            public string terminalName = "";
            public string serial_Number = "";
            public string machine = "";
            public string toolingNo = "";
            public string lotNo = "";
            public string kpsn = "";
            public string workOrder = "";
            public string cavity = "";
            public string testData = "";
            public string results = "";
            public string collectType = "REC";
        }
        public class MesResult
        {
            public bool Result;
            public string RetMsg;
        }
    }
}
