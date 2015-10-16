using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
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
            // TODO: only for authorized

            if (request.Method == HttpMethod.Options)
            {
                var apiExplorer = GlobalConfiguration.Configuration.Services.GetApiExplorer();

                var controllerRequested = request.GetRouteData().Values["controller"] as string;

                var aaa = apiExplorer.ApiDescriptions
                    .Where(d =>
                    {
                        var controller = d.ActionDescriptor.ControllerDescriptor.ControllerName;

                        if (!string.Equals(
                            controller, controllerRequested, StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }

                        var authorizeAttribute =
                            d.ActionDescriptor.GetCustomAttributes<AuthorizeAttribute>().FirstOrDefault();
                        if (authorizeAttribute == null) return true;
                        var rc = request.GetRequestContext();
                        var principal = rc.Principal;

                        var usersSplit = SplitString(authorizeAttribute.Roles);
                        var rolesSplit = SplitString(authorizeAttribute.Users);

                        if (principal?.Identity == null || !principal.Identity.IsAuthenticated)
                        {
                            return false;
                        }
                        if ((int) usersSplit.Length > 0 &&
                            !usersSplit.Contains<string>(principal.Identity.Name, StringComparer.OrdinalIgnoreCase))
                        {
                            return false;
                        }
                        if ((int) rolesSplit.Length <= 0)
                        {
                            return false;
                        }
                        IPrincipal principal1 = principal;
                        if (!rolesSplit.Any<string>(principal1.IsInRole))
                        {
                            return false;
                        }
                        return false;

                    }).ToList();
                    var supportedMethods = 
                    aaa.Select(d => d.HttpMethod.Method)
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

        static string[] SplitString(string original)
        {
            if (string.IsNullOrEmpty(original))
            {
                return new string[] { };
            }
            char[] chrArray = new char[] { ',' };
            IEnumerable<string> strs =
                from piece in original.Split(chrArray)
                let trimmed = piece.Trim()
                where !string.IsNullOrEmpty(trimmed)
                select trimmed;
            return strs.ToArray<string>();
        }
    }
}