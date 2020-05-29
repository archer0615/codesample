## 後端串後端api共同使用Class

---

#### SendRequest
* API URL 用網址串參數的
* RequestMethod 有Get, Post
* ContentType : ***x-www-form-urlencoded***
``` csharp
public void GetPrivacyPolicyCategory()
{
    string requestURL = "http://test.asus.com/OfficialSiteAPI.asmx/GetPrivacyPolicyCategory?Website=zh-tw";
    string responseJsonStr = AOCC_ApiHelper.SendRequest(requestURL, RequestMethod.GET);
}
```
---
#### SendRequestWithHeader
* API URL 用網址串參數的
* RequestMethod 有Get, Post
* ContentType : ***x-www-form-urlencoded***
* Header用 Dictionary<string, string>
``` csharp
private void VerifyTokenByTokenSystem(string api_key, string token)
{
    Dictionary<string, string> headerData = new Dictionary<string, string>();
    headerData.Add("api_key", api_key);
    headerData.Add("Token", token);
    string responseString = AOCC_ApiHelper.SendRequestWithHeader(apiURL, RequestMethod.POST, headerData);
}
```
---
#### SendPostRequestWithBody
* API 參數使用 body傳遞
* RequestMethod 只有 Post
* ContentType : ***multipart/form-data***
* body參數 Dictionary<string, object>
``` csharp
private void TrySendRequestWithHeader()
{
    Dictionary<string, object> postData = new Dictionary<string, object>();
    postData.Add("apiid", test);
    postData.Add("apikey", test);
    string responseString = AOCC_ApiHelper.SendPostRequestWithBody(apiURL, postData);
}
```
---
#### SendPostRequestWithHeaderAndBody
* API 參數使用 body傳遞
* RequestMethod 只有 Post
* ContentType : ***multipart/form-data***
* Header用 Dictionary<string, string>
* body參數 Dictionary<string, object>
``` csharp
private void TrySendRequestWithHeader()
{
    Dictionary<string, string> headerData = new Dictionary<string, string>();
    headerData.Add("api_key", api_key);
    headerData.Add("Token", token);
    Dictionary<string, object> postData = new Dictionary<string, object>();
    postData.Add("apiid", test);
    postData.Add("apikey", test);
    string responseString = AOCC_ApiHelper.SendPostRequestWithHeaderAndBody(apiURL,headerData,  postData);
}
```
---
#### ToPostAPI_withArray
* API 參數使用 URL串起來 進行傳遞
* RequestMethod 只有 Post
* ContentType : ***application/x-www-form-urlencoded***
* 通常使用於有相同post key 並進行傳遞的api 因無法使用 SendPostRequestWithBody 衍伸出來的方法
``` csharp
public void Spec_GetSpec(List<string> partNoList)
{
    string postData = string.Empty;
    
    postData += $"apiid=test";
    postData += $"&apikey=test";
    
    foreach (var item in partNoList)
        postData += "&Part_no=" + item;
    
    string responseJson = AOCC_ApiHelper.ToPostAPI_withArray(GetSpecURL_URL, postData);
}
```
---
#### ToPostSoapRequest
* 待更新範例
``` csharp
```
---
#### ToPostSoapRequestWithXML
* API 端點為Soap
* 根據對方WebMathod webconfig設定 開放Get或者Post
* ***參數 1. 為SoapApi Host URL.*** 
* ***參數 2. header字串.***
* ***參數 3. body字串.***
* 先使用 AOCC_ApiHelper.SetXMLString 組裝header, 和body資訊
* SetXMLString 取得body 需傳入 Body內的主要tag 的名稱
* 最後使用 GetXmlStringData 取得 result中的Json資料 傳入參數根據 api結構而定
* 範例結構如下
``` csharp

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
//                <Data>" + strBody + @"</Data>
//            </Get_WTB>
//        </soap:Body>
//    </soap:Envelope>");
public class WtbHeader
{
    public string apiId { get; set; }
    public string owner_key { get; set; }
}
public class WtbBody_Get_WTB
{
    public string Data { get; set; }
}
public Get_WtbRootResponseResult Get_WTB(List<Get_WtbObjRequest> strResquest)
{
    //[{"sku_member":['90IG0440-MA0R00'],"website":"US","type":"0"}]
    //[{"sku_member":['90NB0I95-M00110'],"website":"US","type":"1"}]
    //[{'sku_member':['90IG02V1-BX1010','90IG0300-BX1333'],'website':'US','type':'2'}]
    string strPostData = JsonConvert.SerializeObject(strResquest);
    WtbHeader header = new WtbHeader() { apiId = WTBSystem_apiid, owner_key = WTBSystem_apikey };
    WtbBody_Get_WTB body = new WtbBody_Get_WTB() { Data = strPostData };
    string strHeader = AOCC_ApiHelper.SetXMLString(header);
    string strBody = AOCC_ApiHelper.SetXMLString(body, true, "Get_WTB");
    
    var responseXML = AOCC_ApiHelper.ToPostSoapRequestWithXML(WTB_Host, strHeader, strBody);
    string responseJsonStr = AOCC_ApiHelper.GetXmlStringData(responseXML, "Get_WTBResult"); 
}

```
---