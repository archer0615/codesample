### API不讓AakamiCache的方法


```csharp
        [HttpGet]
        [NoAkamaiCacheAttribute]
        [Route("api/v1/Translation")]
        public ApiResponse<GetTranslationResultViewModel> Translation(int WebsiteId = 1)
        {
            ApiResponse<GetTranslationResultViewModel> apiResponse = new ApiResponse<GetTranslationResultViewModel>();
            try
            {
                apiResponse.Result.Obj = oCommonService.GetTranslation(WebsiteId);
            }
            catch (Exception ex)
            {
                apiResponse.Backend_ErrorWithLog(oLogService, HttpContext, ex);
            }
            return apiResponse;
        }
```