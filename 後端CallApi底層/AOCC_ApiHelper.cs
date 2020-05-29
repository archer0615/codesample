using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;

namespace AOCC_ApiHelper
{
    public static class AOCC_ApiHelper
    {
        public enum RequestMethod
        {
            GET = 0,
            POST
        }
        public enum ServerMode
        {
            WebAPI,
            Soap
        }
        public static string SendRequest(string apiUrl, RequestMethod method)//x-www-form-urlencoded
        {
            string strJsonData = "";
            try
            {
                HttpWebRequest request = WebRequest.Create(apiUrl) as HttpWebRequest;
                request.Method = method.ToString().ToUpper();
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                strJsonData = new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return strJsonData;
        }

        public static string SendRequestWithHeader(string apiUrl, RequestMethod method,
            Dictionary<string, string> headerData)//x-www-form-urlencoded
        {
            string strJsonData = "";
            try
            {
                HttpWebRequest request = WebRequest.Create(apiUrl) as HttpWebRequest;
                request.Method = method.ToString().ToUpper();

                foreach (var item in headerData)
                {
                    request.Headers.Add(item.Key, item.Value);
                }

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                strJsonData = new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return strJsonData;
        }

        public static string SendPostRequestWithBody(string postURL,
            Dictionary<string, object> postParameters)//multipart/form-data
        {
            HttpWebResponse response = FormUpload.MultipartFormDataPost(postURL, "", postParameters);

            string strJsonData = new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd();

            return strJsonData;
        }

        public static string SendPostRequestWithHeaderAndBody(string apiUrl,
            Dictionary<string, string> headerData, Dictionary<string, object> postData)//multipart/form-data
        {
            string strJsonData = "";
            try
            {
                HttpWebRequest request = WebRequest.Create(apiUrl) as HttpWebRequest;

                foreach (var item in headerData)
                {
                    request.Headers.Add(item.Key, item.Value);
                }

                request = FormUpload.SetPostWebRequestWithBody(request, postData);        

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                
                strJsonData = new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return strJsonData;
        }

        public static string ToPostAPI_withArray(string post_url, string postData)//x-www-form-urlencoded
        {
            var request = WebRequest.Create(post_url) as HttpWebRequest;

            var data = Encoding.UTF8.GetBytes(postData);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }
            var response = request.GetResponse() as HttpWebResponse;

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            return responseString;
        }

        public static string ToPostSoapRequest(string hostURL, string postURL, string postData)
        {
            var webRequest = CreateWebRequest(postURL, ServerMode.Soap);

            byte[] data = Encoding.UTF8.GetBytes(postData);
            using (Stream stream = webRequest.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            string strXMLResponse = new StreamReader(webRequest.GetResponse().GetResponseStream(), Encoding.UTF8).ReadToEnd();

            return strXMLResponse;
        }

        public static string ToPostSoapRequestWithXML(string hostURL, string strHeader, string strBody)
        {
            XmlDocument soapEnvelopeXml = CreateSoapEnvelope(strHeader, strBody);
            var webRequest = CreateWebRequest(hostURL, ServerMode.Soap);
            using (Stream stream = webRequest.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }
            string strXMLResponse = new StreamReader(webRequest.GetResponse().GetResponseStream(), Encoding.UTF8).ReadToEnd();

            return strXMLResponse;
        }

        private static HttpWebRequest CreateWebRequest(string requestUrl, ServerMode serverMode = ServerMode.WebAPI)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(requestUrl);
            if (serverMode == ServerMode.Soap)
            {
                webRequest.ContentType = "text/xml; charset =\"utf-8\"";
                webRequest.Accept = "text/xml";
            }
            else
            {
                webRequest.ContentType = "application/x-www-form-urlencoded";
            }
            webRequest.Method = "POST";
            return webRequest;
        }

        private static XmlDocument CreateSoapEnvelope(string strHeader, string strBody)
        {
            XmlDocument soapEnvelop = new XmlDocument();
            string mainXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
                <soap:Envelope 
                        xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" 
                        xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" 
                        xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" >  ";
            if (!string.IsNullOrWhiteSpace(strHeader))
            {
                mainXML +=
                    @"<soap:Header>
                        <Header xmlns=""http://tempuri.org/"">
                        " + strHeader + @"
                        </Header>
                    </soap:Header>
                ";
            }
            mainXML += @"
                    <soap:Body>
                    " + strBody + @"
                    </soap:Body>
                </soap:Envelope>
            ";
            soapEnvelop.LoadXml(mainXML);
            //soapEnvelop.LoadXml(
            //    @"<?xml version=""1.0"" encoding=""utf-8""?>
            //    <soap:Envelope 
            //            xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" 
            //            xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" 
            //            xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" >                
            //        <soap:Header>
            //            <Header xmlns=""http://tempuri.org/"">
            //                <apiId>52421</apiId>
            //                <owner_key>qqqqq</owner_key>
            //            </Header>
            //        </soap:Header>
            //        <soap:Body>
            //            <Get_WTB xmlns=""http://tempuri.org/"">
            //                <Data>" + strHeader + @"</Data>
            //            </Get_WTB>
            //        </soap:Body>
            //    </soap:Envelope>");
            return soapEnvelop;
        }

        public static string SetXMLString(object data, bool isBody = false, string actionName = "")
        {
            string result = "";
            var tt = data.GetType().GetProperties();
            string xmlnsStartBody = @"<" + actionName + @" xmlns=""http://tempuri.org/"" >";
            string xmlnsEndBody = @"</" + actionName + @">";

            if (isBody) result += xmlnsStartBody;
            foreach (var item in tt)
            {
                result += @"<" + item.Name + ">";
                result += item.GetValue(data);
                result += @"</" + item.Name + ">";
            }
            if (isBody) result += xmlnsEndBody;

            return result;
        }

        public static string GetXmlStringData(string xmlResponseStr, string tagName = "string")
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlResponseStr);
            return doc.GetElementsByTagName(tagName)[0].InnerText;
        }
    }
    public static class FormUpload
    {
        private static readonly Encoding encoding = Encoding.UTF8;

        public static HttpWebRequest SetPostWebRequestWithBody(HttpWebRequest request, Dictionary<string, object> postParameters)
        {
            string formDataBoundary = String.Format("----------{0:N}", Guid.NewGuid());
            string contentType = "multipart/form-data; boundary=" + formDataBoundary;

            byte[] formData = FormUpload.GetMultipartFormData(postParameters, formDataBoundary);
            if (request == null) throw new NullReferenceException("request is not a http request");

            request.Method = "POST";
            request.ContentType = contentType;
            request.CookieContainer = new CookieContainer();
            request.ContentLength = formData.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(formData, 0, formData.Length);
                requestStream.Close();
            }
            return request;
        }

        public static HttpWebResponse MultipartFormDataPost(string postUrl, string userAgent, Dictionary<string, object> postParameters)
        {
            string formDataBoundary = String.Format("----------{0:N}", Guid.NewGuid());
            string contentType = "multipart/form-data; boundary=" + formDataBoundary;

            byte[] formData = GetMultipartFormData(postParameters, formDataBoundary);

            return PostForm(postUrl, userAgent, contentType, formData);
        }

        public static string MultipartFormDataPostReturnResponseString(string postUrl, string userAgent, Dictionary<string, object> postParameters)
        {
            return GetResponseStreamToString(MultipartFormDataPost(postUrl, userAgent, postParameters));
        }

        public static HttpWebResponse PostForm(string postUrl, string userAgent, string contentType, byte[] formData)
        {
            HttpWebRequest request = WebRequest.Create(postUrl) as HttpWebRequest;
            if (request == null) throw new NullReferenceException("request is not a http request");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            //request.Credentials = CredentialCache.DefaultCredentials;
            request.Method = "POST";
            request.ContentType = contentType;
            request.UserAgent = userAgent;
            request.CookieContainer = new CookieContainer();
            request.ContentLength = formData.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(formData, 0, formData.Length);
                requestStream.Close();
            }

            return request.GetResponse() as HttpWebResponse;
        }

        private static string GetResponseStreamToString(HttpWebResponse req)
        {
            Stream st = req.GetResponseStream();
            StreamReader responseStream = new StreamReader(st, Encoding.UTF8);
            return responseStream.ReadToEnd();
        }

        private static byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary)
        {
            Stream formDataStream = new System.IO.MemoryStream();
            bool needsCLRF = false;

            foreach (var param in postParameters)
            {
                if (needsCLRF) formDataStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));

                needsCLRF = true;

                if (param.Value is FileParameter)
                {
                    FileParameter fileToUpload = (FileParameter)param.Value;

                    string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\";\r\nContent-Type: {3}\r\n\r\n",
                        boundary,
                        param.Key,
                        fileToUpload.FileName ?? param.Key,
                        fileToUpload.ContentType ?? "application/octet-stream");

                    formDataStream.Write(encoding.GetBytes(header), 0, encoding.GetByteCount(header));

                    formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
                }
                else
                {
                    string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
                        boundary,
                        param.Key,
                        param.Value);
                    formDataStream.Write(encoding.GetBytes(postData), 0, encoding.GetByteCount(postData));
                }
            }

            string footer = "\r\n--" + boundary + "--\r\n";
            formDataStream.Write(encoding.GetBytes(footer), 0, encoding.GetByteCount(footer));

            formDataStream.Position = 0;
            byte[] formData = new byte[formDataStream.Length];
            formDataStream.Read(formData, 0, formData.Length);
            formDataStream.Close();

            return formData;
        }

        public class FileParameter
        {
            public byte[] File { get; set; }
            public string FileName { get; set; }
            public string ContentType { get; set; }
            public FileParameter(byte[] file) : this(file, null) { }
            public FileParameter(byte[] file, string filename) : this(file, filename, null) { }
            public FileParameter(byte[] file, string filename, string contenttype)
            {
                File = file;
                FileName = filename;
                ContentType = contenttype;
            }
        }
    }
}