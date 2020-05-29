using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ROG.DataDefine.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ROG.Commons.ExternalAPI
{
    public class ExternalAPI_URLService
    {
        private ExternalAPIOptions options;

        public ExternalAPI_URLService(IOptions<ExternalAPIOptions> options)
        {
            this.options = JsonConvert.DeserializeObject<ExternalAPIOptions>(JsonConvert.SerializeObject(options.Value));
        }
        public ProductSystemConfig SetProductSystemConfig() => this.options.ProductSystemConfig;
        public TokenSystemConfig SetTokenSystemConfig() => this.options.TokenSystemConfig;
        public SpecSystemConfig SetSpecSystemConfig() => this.options.SpecSystemConfig;
        public OfficialApiConfig SetOfficialApi_Url() => this.options.OfficialApiConfig;
        public ModuleSystemConfig SetModuleSystemConfig() => this.options.ModuleSystemConfig;
        public MediaSystemConfig SetMediaSystemConfig() => this.options.MediaSystemConfig;
        public AwardSystemConfig SetAwardSystemConfig() => this.options.AwardSystemConfig;
        public WhereToBuySystemConfig SetWhereToBuySystemConfig() => this.options.WhereToBuySystemConfig;
        public RogSpecFiltersAPIConfig SetRogSpecFiltersAPIConfig() => this.options.RogSpecFiltersAPIConfig;
        public SupportApiConfig SetSupportApiConfig() => this.options.SupportApiConfig;
    }
}
