using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace HttpExperiments.Handlers
{
    // http://www.jefclaes.be/2012/09/supporting-options-verb-in-aspnet-web.html
    public class OptionsHttpMessageHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Options)
            {
                var apiExplorer = GlobalConfiguration.Configuration.Services.GetApiExplorer();

                var controllerRequested = request.GetRouteData().Values["controller"] as string;
                var supportedMethods = apiExplorer.ApiDescriptions
                    .Where(d =>
                    {
                        var controller = d.ActionDescriptor.ControllerDescriptor.ControllerName;
                        return string.Equals(
                      controller, controllerRequested, StringComparison.OrdinalIgnoreCase);
                    })
                    .Select(d => d.HttpMethod.Method)
                    .Distinct();

                if (!supportedMethods.Any())
                    return Task.Factory.StartNew(
                        () => request.CreateResponse(HttpStatusCode.NotFound), cancellationToken);

                return Task.Factory.StartNew(() =>
                {
                    var resp = new HttpResponseMessage(HttpStatusCode.OK);
                    resp.Headers.Add("Access-Control-Allow-Origin", "*");
                    resp.Headers.Add(
                        "Access-Control-Allow-Methods", string.Join(",", supportedMethods));

                    return resp;
                }, cancellationToken);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}