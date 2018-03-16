using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Text.RegularExpressions;

namespace rpkp
{
    public class MyRoutes
    {
        public MyRoutes(IApplicationBuilder app)
        {
            var routeBuilder = new RouteBuilder(app);        
           
            routeBuilder.MapGet("", context => {
                return context.Response.WriteAsync("You're drunk.");
            });

            var routes = routeBuilder.Build();

            app.UseRouter(routes);
        }        
    }    
}
