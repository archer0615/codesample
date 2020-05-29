using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ROG.Commons.ExternalAPI;
using ROG.DataDefine.Commons;
using ROG.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using static ROG.DataDefine.Enums.EnumClass;

namespace ROG.Commons.Helpers
{
    public class MiddleSystemHelper : SystemHelperBase
    {
        LogService oLogService;
        public MiddleSystemHelper(ExternalAPI_URLService externalAPI_URLService, LogService _LogService)
            : base(externalAPI_URLService)
        {
            oLogService = _LogService;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        public ModulesGetPageDTO Modules_GetPage(string pageid, string dataRowId = "")
        {
            ModulesGetPageDTO dto = new ModulesGetPageDTO();
            postData.Clear();
            postData.Add("apiid", ModulesSystem_apiid);
            postData.Add("apikey", ModulesSystem_apikey);
            try
            {
                postData.Add("pageid", pageid);

                if (!string.IsNullOrWhiteSpace(dataRowId)) postData.Add("dataRowId", dataRowId);

                string strResponseJsonData = AOCC_ApiHelper.SendPostRequest(Modules_GetPageURL, postData);

                dto = JsonConvert.DeserializeObject<ModulesGetPageDTO>(strResponseJsonData);

            }
            catch (Exception ex)
            {
                oLogService.InsertExternalApiErrorLog(MethodBase.GetCurrentMethod().Name, Modules_GetPageURL, postData, dto, ex.Message);
            }
            return dto;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public JObject Product_gettagInfo(string siteid, List<string> product_id_list)
        {
            JObject dto = new JObject();
            var startTime = DateTime.Now;
            var endTime = DateTime.Now;
            string strJsonData = "";

            strPostData = "";
            strPostData += "apiid=" + this.ProductSystem_apiid;
            strPostData += "&apikey=" + this.ProductSystem_apikey;

            try
            {
                strPostData += "&siteid=" + siteid;

                foreach (var product_id in product_id_list)
                {
                    strPostData += "&product_id=" + product_id;
                }

                startTime = DateTime.Now;

                strJsonData = AOCC_ApiHelper.ToPostAPI_withArray(this.Product_GettagInfo_Url, strPostData);

                dto = JsonConvert.DeserializeObject<JObject>(strJsonData);

                if (dto["status"].ToString() == "1") return null;
                if (dto["data"].ToList().Count == 0) return null;

                oLogService.InsertExternalAPILog(MethodBase.GetCurrentMethod().Name, startTime, DateTime.Now, Product_GettagInfo_Url, "", strPostData, strJsonData);
            }
            catch (Exception ex)
            {
                oLogService.InsertExternalApiErrorLog(MethodBase.GetCurrentMethod().Name, Product_GettagInfo_Url, strPostData, strJsonData, ex.Message);
            }
            return dto;
        }

        public List<GetSpecResultResponseDTO> Spec_GetSpec(string midSiteId, List<string> partNoList, string cutting = "")
        {
            DateTime requestStartTime = DateTime.Now;
            List<GetSpecResultResponseDTO> dto = new List<GetSpecResultResponseDTO>();
            string postData = "";
            string responseJson = "";
            try
            {
                postData = PrivateSpecMethod.SetDefaultPostData(new GetSpecInput(partNoList, midSiteId, cutting), SpecSystem_apiid, SpecSystem_apikey);
                responseJson = AOCC_ApiHelper.ToPostAPI_withArray(GetSpecURL_URL, postData);

                oLogService.InsertExternalAPILog(MethodBase.GetCurrentMethod().Name, requestStartTime, DateTime.Now, GetSpecURL_URL, "", postData, responseJson);

                JObject jObj = JObject.Parse(responseJson);

                if (jObj == null) throw new Exception("Spec System Error : " + jObj["message"].Value<string>());
                if (jObj["result"].Value<int>() == 1) throw new Exception("Spec System Error : " + jObj["message"].Value<string>());

                foreach (var partNo in partNoList)
                {
                    JToken tmpSku = jObj["skus"][partNo];
                    if (jObj["skus"][partNo]["status"].Value<int>() == 0) continue;//測試用先關閉判斷

                    var skuData = JsonConvert.DeserializeObject<SpecPartResponseDTO>(JsonConvert.SerializeObject(tmpSku));

                    dto.Add(new GetSpecResultResponseDTO()
                    {
                        Part_no = partNo,
                        Obj = skuData
                    });
                }
            }
            catch (Exception ex)
            {
                oLogService.InsertExternalApiErrorLog(MethodBase.GetCurrentMethod().Name, GetSpecURL_URL, postData, responseJson, ex.Message);
            }
            return dto;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public RetrunImageUrlDTO Media_GetImageURL(string media_id, string siteId, int width = 0, int hight = 0, int q = 0, string tHex = "")
        {
            RetrunImageUrlDTO dto = new RetrunImageUrlDTO();

            try
            {
                string returnURL = GetSha256Code(media_id + siteId);
                string preURL = IMAGE_URL + media_id;

                if (width > 0) preURL += "/w" + ((width > 2000) ? "2000" : width.ToString());
                if (hight > 0) preURL += "/h" + ((hight > 2000) ? "2000" : hight.ToString());
                if (q > 0) preURL += "/q" + ((q > 100) ? "2000" : q.ToString());
                if (!string.IsNullOrWhiteSpace(tHex)) preURL += "/t" + tHex.ToString();

                dto.ImageUrl = preURL;
                dto.NoSyncImageUrl = preURL + "/?" + returnURL; ;
            }
            catch (Exception)
            {
                dto.Status = "FAIL";
                dto.Message = "FAIL";
            }
            return dto;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public List<Get_WtbResponseObj> WTB_Get_WTB(string website, string type, List<string> partNo)
        {
            DateTime requestStartTime = DateTime.Now;
            Get_WtbRootResponseResult result = new Get_WtbRootResponseResult();
            var requestData = new List<Get_WtbObjRequest>();
            requestData.Add(new Get_WtbObjRequest() { website = website, type = type, sku_member = partNo });
            string responseJsonStr = "";
            string responseXML = "";
            try
            {
                string strPostData = JsonConvert.SerializeObject(requestData);
                var header = new WtbHeader() { apiId = this.WhereToBuySystem_apiid, apikey = WhereToBuySystem_apikey };
                var body = new WtbBody_Get_WTB() { Data = strPostData };
                string strHeader = AOCC_ApiHelper.SetXMLString(header);
                string strBody = AOCC_ApiHelper.SetXMLString(body, true, "Get_WTB");

                responseXML = AOCC_ApiHelper.ToPostSoapRequestWithXML(WTB_Host, strHeader, strBody);
                responseJsonStr = AOCC_ApiHelper.GetXmlStringData(responseXML, "Get_WTBResult");

                result = JsonConvert.DeserializeObject<List<Get_WtbRootResponseResult>>(responseJsonStr)?.FirstOrDefault();
                if (result != null)
                {
                    if (result.Status == "1") throw new Exception("WTB System Error : " + result.Message);

                    oLogService.InsertExternalAPILog(MethodBase.GetCurrentMethod().Name, requestStartTime, DateTime.Now, Get_WTB_Url, "",
                        $"strHeader : {strHeader}; strBody : {strBody}", responseJsonStr);
                }
            }
            catch (Exception ex)
            {
                oLogService.InsertExternalApiErrorLog(MethodBase.GetCurrentMethod().Name, Get_WTB_Url, responseXML, responseJsonStr, ex.Message);
            }
            return result.lists;
        }

        public Get_SimpleSmResult WTB_Get_SimpleSmInfoEC(string ISOwebsite, List<string> partNo)
        {
            Get_SimpleSmResult result = new Get_SimpleSmResult();

            var requestData = new List<Get_SimpleSmObjRequest>();
            requestData.Add(new Get_SimpleSmObjRequest() { sku_member = partNo, ISOwebsite = ISOwebsite });
            string responseJsonStr = "";
            string responseXML = "";
            try
            {
                string strPostData = JsonConvert.SerializeObject(requestData);
                var header = new WtbHeader() { apiId = this.WhereToBuySystem_apiid, apikey = WhereToBuySystem_apikey };
                var body = new WtbBody_Get_WTB() { Data = strPostData };
                string strHeader = AOCC_ApiHelper.SetXMLString(header);
                string strBody = AOCC_ApiHelper.SetXMLString(body, true, "Get_SimpleSmInfoEC");

                DateTime requestStartTime = DateTime.Now;

                responseXML = AOCC_ApiHelper.ToPostSoapRequestWithXML(WTB_Host, strHeader, strBody);
                responseJsonStr = AOCC_ApiHelper.GetXmlStringData(responseXML, "Get_SimpleSmInfoECResult");

                var tmpResult = JsonConvert.DeserializeObject<Get_SimpleSmResponseResult>(responseJsonStr);

                oLogService.InsertExternalAPILog(MethodBase.GetCurrentMethod().Name, requestStartTime, DateTime.Now, Get_WTB_Url, "",
                        $"strHeader : {strHeader}; strBody : {strBody}", responseJsonStr);

                if (tmpResult != null && tmpResult.Status == "1") throw new Exception("WTB System Error : " + tmpResult.Message);
                
                if(tmpResult != null && tmpResult.Status == "0") result = tmpResult.lists;

            }
            catch (Exception ex)
            {
                oLogService.InsertExternalApiErrorLog(MethodBase.GetCurrentMethod().Name, Get_WTB_Url, responseXML, responseJsonStr, ex.Message);
            }
            return result;
        }

        public Get_SimpleSmResult WTB_Get_PIDSimpleSmInfoEC(string midSiteId, string ISOwebsite, List<string> productId)
        {
            Get_SimpleSmResult result = new Get_SimpleSmResult();

            var requestData = new List<Get_PIDSimpleSmObjRequest>();
            requestData.Add(new Get_PIDSimpleSmObjRequest() { websiteid = midSiteId, ISOwebsite = ISOwebsite, pid = productId });
            string responseJsonStr = "";
            string responseXML = "";
            try
            {
                string strPostData = JsonConvert.SerializeObject(requestData);
                var header = new WtbHeader() { apiId = this.WhereToBuySystem_apiid, apikey = WhereToBuySystem_apikey };
                var body = new WtbBody_Get_WTB() { Data = strPostData };
                string strHeader = AOCC_ApiHelper.SetXMLString(header);
                string strBody = AOCC_ApiHelper.SetXMLString(body, true, "Get_PIDSimpleSmInfoEC");

                DateTime requestStartTime = DateTime.Now;

                responseXML = AOCC_ApiHelper.ToPostSoapRequestWithXML(WTB_Host, strHeader, strBody);
                responseJsonStr = AOCC_ApiHelper.GetXmlStringData(responseXML, "Get_PIDSimpleSmInfoECResult");

                var tmpResult = JsonConvert.DeserializeObject<Get_SimpleSmResponseResult>(responseJsonStr);

                oLogService.InsertExternalAPILog(MethodBase.GetCurrentMethod().Name, requestStartTime, DateTime.Now, Get_WTB_Url, "",
                            $"strHeader : {strHeader}; strBody : {strBody}", responseJsonStr);

                if (tmpResult != null && tmpResult.Status == "1") throw new Exception("WTB System Error : " + tmpResult.Message);

                if (tmpResult != null && tmpResult.Status == "0") result = tmpResult.lists;
            }
            catch (Exception ex)
            {
                oLogService.InsertExternalApiErrorLog(MethodBase.GetCurrentMethod().Name, Get_WTB_Url, responseXML, responseJsonStr, ex.Message);
            }
            return result;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public GetAwardListResponseRoot Award_GetAwardList(string productId, string area = "", string productLineId = "", string awardDateS = "", string awardDateE = "")
        {
            DateTime requestStartTime = DateTime.Now;
            GetAwardListResponseRoot result = new GetAwardListResponseRoot();
            string xmlResponseString = "";
            string responseString = "";
            try
            {
                string strPostData = "";
                strPostData += "apiid=" + AwardSystem_apiid;
                strPostData += "&apikey=" + AwardSystem_apikey;
                strPostData += "&productId=" + productId;
                if (!string.IsNullOrWhiteSpace(productLineId)) strPostData += "&productLineId=" + productLineId;
                if (!string.IsNullOrWhiteSpace(awardDateS)) strPostData += "&awardDateS=" + HttpUtility.UrlEncode(awardDateS);
                if (!string.IsNullOrWhiteSpace(awardDateE)) strPostData += "&awardDateE=" + HttpUtility.UrlEncode(awardDateE);
                if (!string.IsNullOrWhiteSpace(area)) strPostData += "&area=" + area;

                xmlResponseString = AOCC_ApiHelper.SendRequest($"{GetAwardList_Url}?{strPostData}");

                responseString = AOCC_ApiHelper.GetXmlStringData(xmlResponseString);

                result = JsonConvert.DeserializeObject<GetAwardListResponseRoot>(responseString);

                if (result.Status == "1") throw new Exception("Award System Error : " + result.Message);

                oLogService.InsertExternalAPILog(MethodBase.GetCurrentMethod().Name, requestStartTime, DateTime.Now, GetAwardList_Url, "", strPostData, responseString);
            }
            catch (Exception ex)
            {
                oLogService.InsertExternalApiErrorLog(MethodBase.GetCurrentMethod().Name, GetAwardList_Url, strPostData, responseString, ex.Message);
            }
            return result;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //tagtype 搜尋模式 ( 0 = TAG搜尋 , 1 = 含該TAG的同義字 , 2 = 含該TAG的翻譯字 , 3 = 含該TAG的同義字+翻譯字 )
        public RogSpecFiltersApiRootObj RogSpecFiltersAPI(int website, string tagid, string productline, string tagtype = "3")
        {
            RogSpecFiltersApiRootObj dto = new RogSpecFiltersApiRootObj();
            postData.Clear();
            postData.Add("apiid", RogSpecFiltersApi_apiid);
            postData.Add("apikey", RogSpecFiltersApi_apikey);
            try
            {
                postData.Add("website", website);
                postData.Add("tagid", tagid);
                postData.Add("productline", productline);
                postData.Add("tagtype", tagtype);

                string strJsonData = AOCC_ApiHelper.SendPostRequest(RogSpecFiltersApi_Url, postData);
                dto = JsonConvert.DeserializeObject<RogSpecFiltersApiRootObj>(strJsonData);
            }
            catch (Exception ex)
            {
                oLogService.InsertExternalApiErrorLog(MethodBase.GetCurrentMethod().Name, RogSpecFiltersApi_Url, postData, dto, ex.Message);
            }
            return dto;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    }

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
        public static string SendRequest(string apiUrl, RequestMethod method = RequestMethod.GET)
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

        public static string ToPostAPI_withArray(string post_url, string postData)
        {
            var request = WebRequest.Create(post_url) as HttpWebRequest;

            var data = Encoding.UTF8.GetBytes(postData);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
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
        public static string ToPostSoapRequest(string postURL, string postData)
        {
            var webRequest = CreateWebRequest(postURL);

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
            string result = "";
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlResponseStr);
            result = doc.GetElementsByTagName(tagName)[0].InnerText;
            return result;
        }
    }

    public class SystemHelperBase
    {
        protected Dictionary<string, object> postData = new Dictionary<string, object>();
        protected string strPostData = "";

        private ModuleSystemConfig moduleSystemConfig;
        private SpecSystemConfig specSystemConfig;
        private MediaSystemConfig mediaSystemConfig;
        private ProductSystemConfig productSystemConfig;
        private WhereToBuySystemConfig whereToBuySystemConfig;
        private AwardSystemConfig awardSystemConfig;
        private RogSpecFiltersAPIConfig rogSpecFiltersAPIConfig;
        public SystemHelperBase(ExternalAPI_URLService externalAPI_URLService)
        {
            this.moduleSystemConfig = externalAPI_URLService.SetModuleSystemConfig();
            this.specSystemConfig = externalAPI_URLService.SetSpecSystemConfig();
            this.mediaSystemConfig = externalAPI_URLService.SetMediaSystemConfig();
            this.productSystemConfig = externalAPI_URLService.SetProductSystemConfig();
            this.whereToBuySystemConfig = externalAPI_URLService.SetWhereToBuySystemConfig();
            this.awardSystemConfig = externalAPI_URLService.SetAwardSystemConfig();
            this.rogSpecFiltersAPIConfig = externalAPI_URLService.SetRogSpecFiltersAPIConfig();

            ModulesSystem_apiid = moduleSystemConfig.apiid;
            ModulesSystem_apikey = moduleSystemConfig.apikey;
            Modules_GetPageURL = moduleSystemConfig.GetPageURL;

            SpecSystem_apiid = specSystemConfig.apiid;
            SpecSystem_apikey = specSystemConfig.apikey;
            GetSpecURL_URL = specSystemConfig.GetSpecURL;

            MediaSystem_apiid = mediaSystemConfig.Media_ApiId;
            MediaSystem_apikey = mediaSystemConfig.Media_ApiKey;
            IMAGE_URL = mediaSystemConfig.MediaDNS;

            ProductSystem_apiid = productSystemConfig.apiid;
            ProductSystem_apikey = productSystemConfig.apikey;
            Product_GettagInfo_Url = productSystemConfig.GettagInfo_Url;

            WhereToBuySystem_apiid = whereToBuySystemConfig.apiid;
            WhereToBuySystem_apikey = whereToBuySystemConfig.apikey;
            WTB_Host = whereToBuySystemConfig.WTB_Host;
            Get_WTB_Url = whereToBuySystemConfig.Get_WTB_Url;

            AwardSystem_apiid = awardSystemConfig.apiid;
            AwardSystem_apikey = awardSystemConfig.apikey;
            Award_Host = awardSystemConfig.Award_Host;
            GetAwardList_Url = awardSystemConfig.GetAwardList_Url;

            RogSpecFiltersApi_apiid = rogSpecFiltersAPIConfig.apiid;
            RogSpecFiltersApi_apikey = rogSpecFiltersAPIConfig.apikey;
            RogSpecFiltersApi_Url = rogSpecFiltersAPIConfig.SpecFilterAPI_Url;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        protected string ModulesSystem_apiid;
        protected string ModulesSystem_apikey;
        protected string Modules_GetPageURL;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        protected string SpecSystem_apiid;
        protected string SpecSystem_apikey;
        protected string GetSpecURL_URL;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        protected string IMAGE_URL;
        protected string MediaSystem_apiid;
        protected string MediaSystem_apikey;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        protected string ProductSystem_apiid;
        protected string ProductSystem_apikey;
        protected string Product_GettagInfo_Url;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        protected string WhereToBuySystem_apiid;
        protected string WhereToBuySystem_apikey;
        protected string WTB_Host;
        protected string Get_WTB_Url;
        protected string Get_SimpleSmInfoEC_Url;
        protected string Get_PIDSimpleSmInfoEC_Url;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        protected string AwardSystem_apiid;
        protected string AwardSystem_apikey;
        protected string Award_Host;
        protected string GetAwardList_Url;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        protected string RogSpecFiltersApi_apiid;
        protected string RogSpecFiltersApi_apikey;
        protected string RogSpecFiltersApi_Url;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        protected string GetSha256Code(string orignData)
        {
            SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
            string sha256Code = string.Empty;

            byte[] textWithSaltBytes = Encoding.ASCII.GetBytes(orignData);
            byte[] hashedBytes = sha256.ComputeHash(textWithSaltBytes);

            foreach (byte bt in hashedBytes)
            {
                sha256Code += bt.ToString("x2");
            }
            return sha256Code;
        }
    }
    public static class PrivateSpecMethod
    {
        public static string SetDefaultPostData(GetSpecInput getSpecInput, string specSystem_apiid, string specSystem_apikey)
        {
            string postData = string.Empty;

            postData += $"apiid={specSystem_apiid}";
            postData += $"&apikey={specSystem_apikey}";
            postData += foreachPart_no(getSpecInput.Part_no);
            if (!string.IsNullOrEmpty(getSpecInput.Siteid)) postData += $"&Siteid={getSpecInput.Siteid}";
            if (!string.IsNullOrEmpty(getSpecInput.Template_id)) postData += $"&Template_id={getSpecInput.Template_id}";
            if (!string.IsNullOrEmpty(getSpecInput.Field)) postData += $"&Field={getSpecInput.Field}";
            if (!string.IsNullOrEmpty(getSpecInput.Flag)) postData += $"&Flag={getSpecInput.Flag}";
            if (!string.IsNullOrEmpty(getSpecInput.Cutting)) postData += $"&Cutting={getSpecInput.Cutting}";
            if (getSpecInput.Field_no != null && getSpecInput.Field_no.Count > 0) postData += foreachField_no(getSpecInput.Part_no);

            return postData;
        }
        private static string foreachPart_no(List<string> Part_no)
        {
            string result = string.Empty;
            foreach (var item in Part_no)
                result += "&Part_no=" + item;

            return result;
        }
        private static string foreachField_no(List<string> Field_no)
        {
            string result = string.Empty;
            foreach (var item in Field_no)
                result += "&Field_no=" + item;

            return result;
        }
    }

    public class BaseReturnDTO
    {
        public string Status;
        public string Message;
        public BaseReturnDTO()
        {
            Status = EnumApiStatus.Success.ToString();
            Message = EnumApiStatus.Success.ToString();
        }
    }

    #region ModulesResponseDTO
    public class ModulesReturnDTO
    {
        public string result { get; set; }
        public string message { get; set; }
    }
    public class ModulesNewPageDTO : ModulesReturnDTO
    {
        public string pageid { get; set; }
        public string dataRowId { get; set; }
    }
    public class ModulesGetPageDTO : ModulesReturnDTO
    {
        public ModulesGetPageObjDTO pagedata { get; set; }
    }
    public class ModulesGetPageObjDTO
    {
        public List<string> tags { get; set; }
        public string createdate { get; set; }
        public string lastdate { get; set; }
        public string date { get; set; }
        public string status { get; set; }//	0 = off , 1 = on
        public string siteID { get; set; }
        public string description { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public string dataRowId { get; set; }
    }
    public class ModulesPageHistoryDTO : ModulesReturnDTO
    {
        public List<ModulesPageHistoryObjDTO> data { get; set; } = new List<ModulesPageHistoryObjDTO>();
    }
    public class ModulesPageHistoryObjDTO
    {
        public string dataRowID { get; set; }
        public string createDate { get; set; }
        public string updateDate { get; set; }
        public string status { get; set; }
        public string cdtuser { get; set; }
    }
    public class GetAll_PageURL_DTO
    {
        public string EditPageURL { get; set; }
        public string EditPageURL_PageId { get; set; }
        public string EditPageURL_PageId_dataRowId { get; set; }
        public string EditPageURL_PageId_Token { get; set; }
        public string EditPageURL_PageId_Token_backUrl { get; set; }
        public string EditPageURL_PageId_dataRowId_Token { get; set; }
        public string EditPageURL_PageId_dataRowId_Token_backUrl { get; set; }
        public string PreviewPageURL_PageId_dataRowId { get; set; }
    }
    #endregion

    #region SpecInputDTO
    public class GetSpecInput
    {
        public List<string> Part_no { get; set; } = new List<string>();
        public string Siteid { get; set; }
        public string Template_id { get; set; }
        public string Field { get; set; }
        public List<string> Field_no { get; set; } = new List<string>();
        public string Flag { get; set; }
        public string Cutting { get; set; }
        public GetSpecInput(List<string> part_no, string siteid, string cutting = "",
            string template_id = "", string field = "", List<string> field_no = null, string flag = "")
        {
            Part_no = part_no;
            Siteid = siteid;
            Template_id = template_id;
            Field = field;
            Field_no = field_no;
            Flag = flag;
            Cutting = cutting;
        }
    }
    #endregion

    #region SpecResponseDTO
    public enum SpecMiddleStatus
    {
        Offline = 0,
        Online,
        Translating
    }
    public class GetSpecResultResponseDTO
    {
        public string Part_no { get; set; }
        public SpecPartResponseDTO Obj { get; set; } = new SpecPartResponseDTO();
    }
    public class SpecPartResponseDTO
    {
        public string sku_part_no { get; set; }
        public string sku_name { get; set; }
        public int Status { get; set; }
        public List<SpecKeySpecObjResponseDTO> keyspec { get; set; } = new List<SpecKeySpecObjResponseDTO>();
        public List<SpecContentResponseDTO> spec_content { get; set; } = new List<SpecContentResponseDTO>();
    }
    public class SpecKeySpecObjResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<SpecKeySpecItemResponseDTO> Items { get; set; } = new List<SpecKeySpecItemResponseDTO>();
    }
    public class SpecKeySpecItemResponseDTO
    {
        public string Name { get; set; }
        public string Key { get; set; }
    }
    public class SpecContentResponseDTO
    {
        public string display_field { get; set; }
        public string display_field_id { get; set; }
        public List<SpecDescriptionResponseDTO> descriptions { get; set; }
        public string description_text { get; set; }
    }
    public class SpecDescriptionResponseDTO
    {
        public string display_description { get; set; }
        public string description_no { get; set; }
        public string field_no { get; set; }
        public string option_no { get; set; }
        public string flag { get; set; }
    }
    #endregion

    #region MediaResponseDTO
    public class ReturnQueryImageDTO : BaseReturnDTO
    {
        public string result { get; set; }
        public string total { get; set; }
        public List<ReturnQueryImageItemDTO> datas { get; set; }
    }
    public class ReturnQueryImageItemDTO
    {
        public string site { get; set; }
        public string fileName { get; set; }
        public string fileSize { get; set; }
        public string id { get; set; }
        public string contentType { get; set; }
        public string media_name { get; set; }
        public string cdt { get; set; }
        public string status { get; set; }
    }
    public class MediaUpdateReturnDTO : BaseReturnDTO
    {
        public string result { get; set; }
        public string media_id { get; set; }
    }
    public class RetrunImageUrlDTO : BaseReturnDTO
    {
        public string ImageUrl { get; set; }
        public string NoSyncImageUrl { get; set; }
    }
    #endregion

    #region ProductResponseDTO
    public class BaseReturnProductDTO
    {
        public string status { get; set; }
        public string msg { get; set; }
    }
    public class Product_getebsmodelInfo : BaseReturnProductDTO
    {
        public List<Product_getebsmodelInfo_Data> data { get; set; }
    }
    public class Product_getebsmodelInfo_Data
    {
        public string product_line { get; set; }
        public string series_name { get; set; }
        public string option_id { get; set; }
        public string second_model_name { get; set; }
    }
    public class Product_createProduct : BaseReturnProductDTO
    {
        public int productinfo_id { get; set; }
    }
    public class Product_gettagInfo : BaseReturnProductDTO
    {
        public List<Product_gettagInfo_Obj> data { get; set; }
    }
    public class Product_gettagInfo_Obj
    {
        public string product_id { get; set; }
        public string siteid { get; set; }
        public List<Product_gettagInfo_Tags> tags { get; set; }
    }
    public class Product_gettagInfo_Tags
    {
        public string tag_id { get; set; }
        public string tag_name { get; set; }
    }
    public class Product_getProduct90 : BaseReturnProductDTO
    {
        public List<Product_getProduct90_Data> data { get; set; } = new List<Product_getProduct90_Data>();
    }
    public class Product_getProduct90_Data
    {
        public string sku { get; set; }
        public string EBSsales_model_name { get; set; }
        public string biscountry { get; set; }
        public string siteid { get; set; }
    }

    #endregion

    #region WtbRequestDTO
    public class Get_WtbRootRequest
    {
        public List<Get_WtbObjRequest> Data { get; set; } = new List<Get_WtbObjRequest>();
    }
    public class Get_WtbObjRequest
    {
        public List<string> sku_member { get; set; } = new List<string>();
        public string website { get; set; }
        public string type { get; set; }
    }

    public class Get_SimpleSmInfoECRequest
    {
        public List<Get_SimpleSmObjRequest> Data { get; set; } = new List<Get_SimpleSmObjRequest>();
    }

    public class Get_SimpleSmObjRequest
    {
        public List<string> sku_member { get; set; } = new List<string>();
        public string ISOwebsite { get; set; }
    }
    public class Get_PIDSimpleSmInfoECRequest
    {
        public List<Get_PIDSimpleSmObjRequest> Data { get; set; } = new List<Get_PIDSimpleSmObjRequest>();
    }

    public class Get_PIDSimpleSmObjRequest
    {
        public List<string> pid { get; set; } = new List<string>();
        public string ISOwebsite { get; set; }
        public string websiteid { get; set; }
    }

    #endregion

    #region WtbResponse Class
    public class Get_WtbRootResponseResult
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public List<Get_WtbResponseObj> lists { get; set; } = new List<Get_WtbResponseObj>();
    }

    public class Get_WtbResponseObj
    {
        public string sku_member { get; set; }
        public List<Get_WtbResponseItem> Hatch { get; set; } = new List<Get_WtbResponseItem>();
        public List<Get_WtbResponseItem> EC { get; set; } = new List<Get_WtbResponseItem>();
    }
    public class Get_WtbResponseItem
    {
        public string name { get; set; }
        public string logo { get; set; }
        public string stock_qty { get; set; }
        public string currency { get; set; }
        public string currencySymbol { get; set; }
        public double price { get; set; }
        public string purchase_link { get; set; }
    }
    public class Get_SimpleSmResponseResult
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public Get_SimpleSmResult lists { get; set; } = new Get_SimpleSmResult();
    }
    public class Get_SimpleSmResult
    {
        public string Currency { get; set; }
        public string CurrencySign { get; set; }
        public double CheapestPrice { get; set; }
    }

    #endregion

    #region Wtb HeaderData
    public class WtbHeader
    {
        public string apiId { get; set; }
        public string apikey { get; set; }
    }
    public class WtbBody_Get_WTB
    {
        public string Data { get; set; }
    }
    #endregion

    #region Award Class
    public class GetAwardListRequsetDTO
    {
        public string productId { get; set; }
        public string productLineId { get; set; }
        public string awardDateS { get; set; }
        public string awardDateE { get; set; }
        public string websiteId { get; set; }
        public string area { get; set; }
    }
    public class GetAwardListResponseRoot
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public List<GetAwardListResponseObj> Lists { get; set; }
    }

    public class GetAwardListResponseObj
    {
        public string AwardId { get; set; }
        public string ImageURL { get; set; }
        public int ProductLineID { get; set; }
        public int ProductID { get; set; }
        public string AwardDate { get; set; }
        public string Subject { get; set; }
        public string Comments { get; set; }
        public string Medianame { get; set; }
        public string Area { get; set; }
        public string ExternalURL { get; set; }
    }
    #endregion

    #region RogSpecFiltersApi

    public class RogSpecFiltersApiRootObj
    {
        public List<RogSpecFiltersApi_Product> products { get; set; } = new List<RogSpecFiltersApi_Product>();
        public List<RogSpecFiltersApi_Sku> skus { get; set; } = new List<RogSpecFiltersApi_Sku>();
        public int status { get; set; }
    }

    public class RogSpecFiltersApi_Product
    {
        public int id { get; set; }
        public List<string> skus { get; set; } = new List<string>();
        public List<RogSpecFiltersApi_Tag> tags { get; set; } = new List<RogSpecFiltersApi_Tag>();
    }

    public class RogSpecFiltersApi_Sku
    {
        public string sku { get; set; }
        public int productid { get; set; }
        public int Priority { get; set; }
        public List<RogSpecFiltersApi_Tag> tags { get; set; } = new List<RogSpecFiltersApi_Tag>();
    }

    public class RogSpecFiltersApi_Tag
    {
        public int tagid { get; set; }
        public string tagname { get; set; }
    }

    #endregion
}