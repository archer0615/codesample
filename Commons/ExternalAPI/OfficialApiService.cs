using Newtonsoft.Json;
using ROG.DataDefine.ApiRequests.Official;
using ROG.DataDefine.Commons;
using ROG.DataDefine.ViewModels;
using ROG.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ROG.Commons.ExternalAPI
{
    public class OfficialApiService : BaseService
    {
        OfficialApiConfig urlConfig;
        SupportApiConfig supportUrlConfig;

        public OfficialApiService(ExternalAPI_URLService externalAPI_URLService, LogService oLogService) : base(oLogService)
        {
            this.urlConfig = externalAPI_URLService.SetOfficialApi_Url();
            this.supportUrlConfig = externalAPI_URLService.SetSupportApiConfig();
        }

        public ApiResponse<GetPrivacyPolicyCategoryResultDTO> GetPrivacyPolicyCategory(string website)
        {
            string requestURL = urlConfig.GetPrivacyPolicyCategory.Replace("{Website}", website);

            string responseStr = this.SendRequestToString("GetPrivacyPolicyCategory", requestURL);

            var result = this.ResponseStrMapping<ApiResponse<GetPrivacyPolicyCategoryResultDTO>>(responseStr);

            return result;
        }

        public string GetPrivacyPolicy(string website, string policyID)
        {
            string requestURL = urlConfig.GetPrivacyPolicy.Replace("{Website}", website)
                                                                                .Replace("{PolicyID}", policyID);

            string responseStr = this.SendRequestToString("GetPrivacyPolicy", requestURL);

            return responseStr;
        }

        public ApiResponse<GetPDSupportTabDTO> GetPDSupportTab(string Website, int ProductId)
        {
            var result = new ApiResponse<GetPDSupportTabDTO>();
            string requestURL = supportUrlConfig.GetPDSupportTab.Replace("{Website}", Website).Replace("{ProductId}", ProductId.ToString());
            string responseStr = "";
            try
            {                
                responseStr = this.SendRequestToString("GetPDSupportTab", requestURL);
                result = this.ResponseStrMapping<ApiResponse<GetPDSupportTabDTO>>(responseStr);
            }
            catch(Exception ex)
            {
                oLogService.InsertExternalApiErrorLog(MethodBase.GetCurrentMethod().Name, requestURL, "", responseStr, ex.Message);
            }
            return result;
        }
    }
}
