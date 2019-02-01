using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace ZQ.BaseLibrary.Web
{
    public class WebPostInputStream
    {
        /// <summary>
        /// 主要针对Web中POST请求中数据流读取。
        /// 当然，我认为可读数据流可能都能试用。
        /// </summary>
        /// <param name="postRequestInputStream">数据流</param>
        /// <returns></returns>
        public static string ReadPostInputStream(System.IO.Stream postRequestInputStream)
        {
            int read = postRequestInputStream.ReadByte();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            while (read >= 0)
            {
                sb.Append((char)read);
                read = postRequestInputStream.ReadByte();
            }
            return sb.ToString();
        }
        /// <summary>
        /// 主要针对Web中POST请求中数据流读取。
        /// 当然，我认为可读数据流可能都能试用。
        /// </summary>
        /// <param name="postRequestInputStream">数据流</param>
        /// <returns></returns>
        public static string SaveFileInPostStream(System.IO.Stream postRequestInputStream, string fileName)
        {
            byte[] b = new byte[postRequestInputStream.Length];
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (b.LongLength <= 2147483647)
            {
                postRequestInputStream.Read(b, 0, b.Length);
                string[] subpath = fileName.Split('\\');
                StringBuilder derection = new StringBuilder();
                for (var i = 0; i < subpath.Length - 1; i++)
                {
                    derection.AppendFormat("{0}\\", subpath[i]);
                }
                if (!System.IO.Directory.Exists(derection.ToString()))
                {
                    System.IO.Directory.CreateDirectory(derection.ToString());
                }
                if (System.IO.File.Exists(fileName))
                {
                    System.IO.File.Delete(fileName);
                }
                //System.IO.File.Create(pFileName,b.Length,System.IO.FileOptions.
                System.IO.File.WriteAllBytes(fileName, b);
                //{
                //    bw.w
                //    bw.Flush();
                //}
            }
            else
            {
                throw new Exception("数据太大");
            }
            return sb.ToString();
        }
        /// <summary>
        /// post操作
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        public static string WebPost(string url, string postData)
        {
            return WebPost(url, postData, Encoding.UTF8);
        }
        /// <summary>
        /// post操作
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        public static string WebPost(string url, string postData, Encoding dataEncoding)
        {
            HttpWebRequest webrequest = null;
            HttpWebResponse response = null;
            System.IO.StreamReader reader = null;
            System.IO.Stream stream = null;
            try
            {
                webrequest = HttpWebRequest.Create(url) as HttpWebRequest;
                if (webrequest == null)
                {
                    return string.Empty;
                }
                webrequest.Method = "POST";
                string encodingName = dataEncoding.WebName;
                webrequest.ContentType = string.Format("text/html;charset={0}", encodingName);
                byte[] postdatabyte = dataEncoding.GetBytes(postData);
                webrequest.ContentLength = postdatabyte.Length;
                stream = webrequest.GetRequestStream();
                stream.Write(postdatabyte, 0, postdatabyte.Length);
                //得到返回信息
                response = (HttpWebResponse)webrequest.GetResponse();
                string encoding = response.ContentEncoding;
                if (encoding == null || encoding.Length < 1)
                {
                    encoding = encodingName;
                }
                reader = new System.IO.StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding));
                string retString = reader.ReadToEnd();
                reader.Close();
                stream.Close();
                response.Close();
                webrequest.Abort();
                return retString;
            }
            catch
            {
                CloseRequest(webrequest, response, stream, reader);
                throw;
            }
        }
        /// <summary>
        /// post操作
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        public static string WebPost<T>(string url, T postData, Encoding dataEncoding)
        {
            HttpWebRequest webrequest = null;
            HttpWebResponse response = null;
            System.IO.StreamReader reader = null;
            System.IO.Stream stream = null;
            try
            {
                webrequest = HttpWebRequest.Create(url) as HttpWebRequest;
                if (webrequest == null)
                {
                    return string.Empty;
                }
                webrequest.Method = "POST";
                string encodingName = dataEncoding.WebName;
                webrequest.ContentType = string.Format("text/html;charset={0}", encodingName);
                byte[] postdatabyte;
                ToJsonByte<T>(out postdatabyte, postData);
                webrequest.ContentLength = postdatabyte.Length;
                stream = webrequest.GetRequestStream();
                stream.Write(postdatabyte, 0, postdatabyte.Length);
                //得到返回信息
                response = (HttpWebResponse)webrequest.GetResponse();
                string encoding = response.ContentEncoding;
                if (encoding == null || encoding.Length < 1)
                {
                    encoding = encodingName;
                }
                reader = new System.IO.StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding));
                string retString = reader.ReadToEnd();
                reader.Close();
                stream.Close();
                response.Close();
                webrequest.Abort();
                return retString;
            }
            catch
            {
                CloseRequest(webrequest, response, stream, reader);
                throw;
            }
        }
        /// <summary>
        /// 去掉反斜杠
        /// 原本认为json转字符串时，服务代码（C#）双引号显示为'\"',所以认为转成byte[]时多了个‘\’字符。不过现在发现，这个想法是错的
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static byte[] RemoveOblique(byte[] data)
        {
            IList<InputData> input = new List<InputData>();
            foreach (byte d in data)
            {
                InputData one = new InputData();
                one.Data = d;
                one.ToRemove = d.Equals(System.Convert.ToByte('\"'));
                input.Add(one);
            }
            var returnData = from aInput in input where !aInput.ToRemove select aInput;
            IList<byte> rtnData = new List<byte>();
            foreach (InputData o in returnData)
            {
                rtnData.Add(o.Data);
            }
            return rtnData.ToArray();
        }
        /// <summary>
        /// get操作
        /// </summary>
        /// <param name="url"></param>
        /// <param name="getData"></param>
        public static string WebGet(string url, string getData)
        {
            url = string.Format("{0}?{1}", url, (getData ?? string.Empty));
            url = url.Trim().TrimEnd('?');
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            System.IO.Stream myResponseStream = null;
            System.IO.StreamReader myStreamReader = null;
            try
            {
                request = WebRequest.Create(url) as HttpWebRequest;
                if (request == null)
                {
                    return string.Empty;
                }
                request.Method = "GET";
                request.ContentType = "text/html;charset=UTF-8";
                response = (HttpWebResponse)request.GetResponse();
                //得到返回信息
                myResponseStream = response.GetResponseStream();
                myStreamReader = new System.IO.StreamReader(myResponseStream, Encoding.UTF8);
                string retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                response.Close();
                request.Abort();
                return retString;
            }
            catch
            {
                CloseRequest(request, response, myResponseStream, myStreamReader);
                throw;
            }
        }
        private static void CloseRequest(HttpWebRequest request, HttpWebResponse response
            , System.IO.Stream myResponseStream, System.IO.StreamReader myStreamReader)
        {
            if (myStreamReader != null)
            {
                myStreamReader.Close();
            }
            if (myResponseStream != null)
            {
                myResponseStream.Close();
            }
            if (response != null)
            {
                response.Close();
            }
            if (request != null)
            {
                request.Abort();
            }
        }
        /// <summary>
        /// get操作
        /// 增加一个headers参数，不知道是不是这样处理
        /// </summary>
        /// <param name="url"></param>
        /// <param name="getData"></param>
        /// <param name="headers"></param>
        public static string WebGet(string url, string getData, string headers)
        {
            url = string.Format("{0}?{1}", url, (getData ?? string.Empty));
            url = url.Trim().TrimEnd('?');
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            System.IO.Stream myResponseStream = null;
            System.IO.StreamReader myStreamReader = null;
            try
            {
                request = WebRequest.Create(url) as HttpWebRequest;
                if (request == null)
                {
                    return string.Empty;
                }
                request.Method = "GET";
                request.ContentType = "text/html;charset=UTF-8";
                request.Headers.Add("headers", (headers ?? string.Empty));
                response = (HttpWebResponse)request.GetResponse();
                //得到返回信息
                myResponseStream = response.GetResponseStream();
                myStreamReader = new System.IO.StreamReader(myResponseStream, Encoding.UTF8);
                string retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                response.Close();
                request.Abort();
                return retString;
            }
            catch
            {
                CloseRequest(request, response, myResponseStream, myStreamReader);
                throw;
            }
        }
        /// <summary>
        /// 把对象转入json byte
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toJsonByte"></param>
        /// <param name="obj"></param>
        private static void ToJsonByte<T>(out byte[] toJsonByte, T obj)
        {
            //实例化DataContractJsonSerializer对象，需要待序列化的对象类型
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
            //实例化一个内存流，用于存放序列化后的数据
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                //使用WriteObject序列化对象
                serializer.WriteObject(stream, obj);
                //写入内存流中
                toJsonByte = new byte[stream.Length];
                stream.Position = 0;
                stream.Read(toJsonByte, 0, (int)stream.Length);
            }
        }

        #region https
        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受     
        }
        //下面这个定义不知何用
        private static string DefaultUserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
        /// <summary>
        /// post操作
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        public static string HttpsPost<T>(string url, T postData, Encoding dataEncoding)
        {
            HttpWebRequest webrequest = null;
            HttpWebResponse response = null;
            System.IO.StreamReader reader = null;
            System.IO.Stream stream = null;
            try
            {
                if (url.StartsWith("https://"))
                {
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                    ServicePointManager.CheckCertificateRevocationList = true;
                    ServicePointManager.Expect100Continue = false;
                    ServicePointManager.DefaultConnectionLimit = 100;
                }
                string formDataBoundary = String.Format("----------{0:N}", Guid.NewGuid());
                webrequest = HttpWebRequest.Create(url) as HttpWebRequest;
                if (webrequest == null)
                {
                    return string.Empty;
                }
                webrequest.Method = "POST";
                string encodingName = dataEncoding.WebName;
                webrequest.ProtocolVersion = HttpVersion.Version11;
                webrequest.AllowAutoRedirect = true;
                webrequest.UserAgent = DefaultUserAgent;
                webrequest.ContentType = "multipart/form-data; boundary=" + formDataBoundary;//而不用string.Format("text/html;charset={0}", encodingName);
                webrequest.KeepAlive = false;
                webrequest.Accept = "*/*";
                webrequest.Referer = null;
                byte[] postdatabyte = GetPostDataInHttps<T>(postData, formDataBoundary, dataEncoding);
                webrequest.ContentLength = postdatabyte.Length;
                stream = webrequest.GetRequestStream();
                stream.Write(postdatabyte, 0, postdatabyte.Length);
                //得到返回信息
                response = (HttpWebResponse)webrequest.GetResponse();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    string message = String.Format("Request failed. Received HTTP {0}", response.StatusCode);
                    throw new ApplicationException(message);
                }
                string encoding = response.ContentEncoding;
                if (encoding == null || encoding.Length < 1)
                {
                    encoding = encodingName;
                }
                reader = new System.IO.StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding));
                string retString = reader.ReadToEnd();
                reader.Close();
                stream.Close();
                response.Close();
                webrequest.Abort();
                return retString;
            }
            catch
            {
                CloseRequest(webrequest, response, stream, reader);
                throw;
            }
        }
        /// <summary>
        /// 得到传递的信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="formDataBoundary"></param>
        /// <returns></returns>
        private static byte[] GetPostDataInHttps<T>(T t, string formDataBoundary, Encoding encoding)
        {
            Type ty = t.GetType();
            StringBuilder postData = new StringBuilder();
            foreach (System.Reflection.PropertyInfo one in ty.GetProperties())
            {
                postData.AppendLine(string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
                    formDataBoundary,
                    one.Name,
                    one.GetValue(t, null))
                   );
            }
            postData.AppendLine(string.Format("--{0}--", formDataBoundary));
            return encoding.GetBytes(postData.ToString());
        }
        /// <summary>
        /// get操作
        /// 增加一个headers参数,主要原因是调用的接口要用到这个参数
        /// </summary>
        /// <param name="url"></param>
        /// <param name="getData"></param>
        /// <param name="headers"></param>
        public static string HttpsGet(string url, string getData, string headers)
        {
            url = string.Format("{0}?{1}", url, (getData ?? string.Empty));
            url = url.Trim().TrimEnd('?');
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            System.IO.Stream myResponseStream = null;
            System.IO.StreamReader myStreamReader = null;
            try
            {
                if (url.StartsWith("https://"))
                {
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                    ServicePointManager.CheckCertificateRevocationList = true;
                    ServicePointManager.Expect100Continue = false;
                    ServicePointManager.DefaultConnectionLimit = 100;
                }
                request = WebRequest.Create(url) as HttpWebRequest;
                if (request == null)
                {
                    return string.Empty;
                }
                request.Method = "GET";
                //request.ContentType = "application/json";//而不用string.Format("text/html;charset={0}", encodingName)
                request.Accept = "*/*";//"application/json";
                request.Referer = null;
                request.UserAgent = DefaultUserAgent;
                request.AllowAutoRedirect = true;
                //-----这个是相应接口的处理，因该接口要在这里传参数------ start
                request.Headers.Add("Access-Token", (headers ?? string.Empty));
                //-----这个是相应接口的处理，因该接口要在这里传参数------ end
                response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    string message = String.Format("Request failed. Received HTTP {0}", response.StatusCode);
                    throw new ApplicationException(message);
                }
                //得到返回信息
                myResponseStream = response.GetResponseStream();
                myStreamReader = new System.IO.StreamReader(myResponseStream, Encoding.UTF8);
                string retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                response.Close();
                request.Abort();
                return retString;
            }
            catch
            {
                CloseRequest(request, response, myResponseStream, myStreamReader);
                throw;
            }
        }
        /// <summary>
        /// get操作
        /// </summary>
        /// <param name="url"></param>
        /// <param name="getData"></param>
        /// <param name="headers"></param>
        public static string HttpsGet(string url, string getData)
        {
            url = string.Format("{0}?{1}", url, (getData ?? string.Empty));
            url = url.Trim().TrimEnd('?');
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            System.IO.Stream myResponseStream = null;
            System.IO.StreamReader myStreamReader = null;
            try
            {
                if (url.StartsWith("https://"))
                {
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                    ServicePointManager.CheckCertificateRevocationList = true;
                    ServicePointManager.Expect100Continue = false;
                    ServicePointManager.DefaultConnectionLimit = 100;
                }
                request = WebRequest.Create(url) as HttpWebRequest;
                if (request == null)
                {
                    return string.Empty;
                }
                request.Method = "GET";
                //request.ContentType = "application/json";//而不用string.Format("text/html;charset={0}", encodingName)
                request.Accept = "*/*";//"application/json";
                request.Referer = null;
                request.UserAgent = DefaultUserAgent;
                request.AllowAutoRedirect = true;
                response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    string message = String.Format("Request failed. Received HTTP {0}", response.StatusCode);
                    throw new ApplicationException(message);
                }
                //得到返回信息
                myResponseStream = response.GetResponseStream();
                myStreamReader = new System.IO.StreamReader(myResponseStream, Encoding.UTF8);
                string retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                response.Close();
                request.Abort();
                return retString;
            }
            catch
            {
                CloseRequest(request, response, myResponseStream, myStreamReader);
                throw;
            }
        }
        #endregion https
    }
    /// <summary>
    /// 传递的byte数组。
    /// 原本认为json转字符串时，服务代码（C#）双引号显示为'\"',就认为转成byte[]时多了个‘\’字符。不过现在发现，这个想法是错的
    /// </summary>
    internal struct InputData
    {
        /// <summary>
        /// 数据
        /// </summary>
        internal byte Data { get; set; }
        /// <summary>
        /// 删除标志
        /// </summary>
        internal bool ToRemove { get; set; }
    }

}
