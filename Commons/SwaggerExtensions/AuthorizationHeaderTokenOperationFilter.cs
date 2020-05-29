using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ROG.Commons.SwaggerExtensions
{
    public class AuthorizationHeaderTokenOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var tmpList = context.MethodInfo.CustomAttributes.ToList().Select(x => x.AttributeType.Name);
            //判斷POST 才給Token的欄位
            if (tmpList.Contains("ServiceFilterAttribute"))
            {
                if (operation.Parameters == null)
                    operation.Parameters = new List<IParameter>();

                ApiTokenHeaderParameter api_keyHeader = new ApiTokenHeaderParameter
                {
                    Name = "api_key",
                    In = "header",
                    Type = "string",
                    Required = true,
                };
                ApiTokenHeaderParameter tokenHeader = new ApiTokenHeaderParameter
                {
                    Name = "Token",
                    In = "header",
                    Type = "string",
                    Required = true,
                };

                operation.Parameters.Add(api_keyHeader);
                operation.Parameters.Add(tokenHeader);
            }
        }

        public class ApiTokenHeaderParameter : NonBodyParameter
        {
        }
    }
}
