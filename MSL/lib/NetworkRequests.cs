using System;
using System.IO;
using System.Net;
using System.Text;

namespace MSL.lib
{
    internal class NetworkRequests
    {
        public static string Get(string url = "")
        {
            WebClient webClient = new WebClient
            {
                Credentials = CredentialCache.DefaultCredentials
            };
            byte[] pageData = webClient.DownloadData(url);
            return Encoding.UTF8.GetString(pageData);
        }

        public static string Post(string url = "", string data = "", bool json = true)
        {
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
            byte[] buf = Encoding.GetEncoding("UTF-8").GetBytes(data);
            myRequest.ContentLength = buf.Length;
            myRequest.MaximumAutomaticRedirections = 1;
            myRequest.AllowAutoRedirect = true;
            myRequest.Method = "POST";
            if (json)
            {
                myRequest.Accept = "application/json";
                myRequest.ContentType = "application/json; charset=UTF-8";
                
            }
            else
            {
                myRequest.Accept = "text/plain";
                myRequest.ContentType = "text/plain; charset=UTF-8";
            }
            using (Stream stream = myRequest.GetRequestStream()) { stream.Write(buf, 0, buf.Length); }

            // 获取响应
            using (HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse())
            using (StreamReader reader = new StreamReader(myResponse.GetResponseStream() ?? throw new InvalidOperationException(), Encoding.UTF8))
            {
                //string returnData = Regex.Unescape(reader.ReadToEnd());
                string returnData = reader.ReadToEnd();
                return returnData;
            }
        }
    }
}
