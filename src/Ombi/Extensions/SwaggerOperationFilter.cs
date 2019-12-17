using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Ombi
{
    public class SwaggerOperationFilter : IOperationFilter
    {
        public string Name { get; private set; }

        public SwaggerOperationFilter()
        {
            Name = "Authorization";
        }

        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<IParameter>();
            var tokenAuthDict = new Dictionary<string, IEnumerable<string>> {{Name, new List<string>()}};
            operation.Security = new IDictionary<string, IEnumerable<string>>[] { tokenAuthDict };
        }
    }
}
