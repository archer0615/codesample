using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ROG.Commons.Helpers
{
    public enum RequestMethod
    {
        GET = 0,
        POST,
        PUT,
        DELETE,
    }
    public static class WebAPIHelper
    {
        public static async Task<string> SendRequestAsync(string apiUrl, RequestMethod method)
        {
            string jsonStr = "";
            try
            {
                HttpWebRequest request = WebRequest.Create(apiUrl) as HttpWebRequest;
                request.Method = method.ToString().ToUpper();
                HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse;

                using (Stream receiveStream = response.GetResponseStream())
                {
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        jsonStr = await readStream.ReadToEndAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return jsonStr;
        }

        public static string SendRequest(string apiUrl, RequestMethod method)
        {
            string jsonStr = "";
            try
            {
                HttpWebRequest request = WebRequest.Create(apiUrl) as HttpWebRequest;
                request.Method = method.ToString().ToUpper();
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                using (Stream receiveStream = response.GetResponseStream())
                {
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        jsonStr = readStream.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return jsonStr;
        }

        public static string SendPostRequest(string postURL, Dictionary<string, object> postParameters)
        {
            var req = FormUpload.MultipartFormDataPost(postURL, "", postParameters);

            Stream st = req.GetResponseStream();
            StreamReader responseStream = new StreamReader(st, Encoding.UTF8);
            string strJsonData = responseStream.ReadToEnd();
            return strJsonData;
        }

        public static string SendRequestWithHeader(string apiUrl, RequestMethod method, Dictionary<string, string> headerData)
        {
            string jsonStr = "";
            try
            {
                HttpWebRequest request = WebRequest.Create(apiUrl) as HttpWebRequest;
                request.Method = method.ToString().ToUpper();

                foreach (var item in headerData)
                {
                    request.Headers.Add(item.Key, item.Value);
                }

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                using (Stream receiveStream = response.GetResponseStream())
                {
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        jsonStr = readStream.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return jsonStr;
        }

        private static string ReadResponseStr(HttpWebResponse response)
        {
            string jsonStr;
            using (Stream receiveStream = response.GetResponseStream())
            {
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    jsonStr = readStream.ReadToEnd();
                }
            }

            return jsonStr;
        }

        public static string SendPostRequestWithBody(string postURL,
            Dictionary<string, object> postParameters)//multipart/form-data
        {
            try
            {
                string formDataBoundary = String.Format("----------{0:N}", Guid.NewGuid());
                string contentType = "multipart/form-data; boundary=" + formDataBoundary;

                byte[] formData = FormUpload.GetMultipartFormData(postParameters, formDataBoundary);

                HttpWebResponse response = FormUpload.MultipartFormDataPost(postURL, "", postParameters);

                string strJsonData = new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd();

                return strJsonData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }    
}
