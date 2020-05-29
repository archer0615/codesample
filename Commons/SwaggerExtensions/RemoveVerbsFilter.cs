using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ROG.Commons.SwaggerExtensions
{
    public class RemoveVerbsFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            //不給人看得Controller
            List<string> ignroeList = new List<string>() { "Country", "RogService", "Article", "Home" };

            //排除寫法1
            //swaggerDoc.Paths.Values.ToList()
            //    .Where(x => x.Get != null && ignroeList.Contains(x.Get?.Tags[0]) ||
            //                x.Post != null && ignroeList.Contains(x.Post?.Tags[0]))
            //                .ToList<PathItem>().ForEach(x => { x.Get = null; x.Post = null; });

            //排除寫法2
            foreach (PathItem path in swaggerDoc.Paths.Values)
            {
                if (path.Get != null && ignroeList.Contains(path.Get?.Tags[0]))
                {
                    path.Get = null;
                    continue;
                }
                else if (path.Post != null && ignroeList.Contains(path.Post?.Tags[0]))
                {
                    path.Post = null;
                    continue;
                }
            }
            //隱藏Swagger Models
            //context.SchemaRegistry.Definitions.Clear();
        }
    }
}
