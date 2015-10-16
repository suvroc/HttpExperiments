using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;

namespace HttpExperiments.Filters
{
    // http://www.tomdupont.net/2014/11/web-api-return-correct-status-codes-for.html
    public class StatusCodeExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private static readonly Dictionary<Type, HttpStatusCode> TypeToCodeMap;

        static StatusCodeExceptionFilterAttribute()
        {
            TypeToCodeMap = new Dictionary<Type, HttpStatusCode>
        {
            {typeof (ArgumentException), HttpStatusCode.BadRequest},
            {typeof (SecurityException), HttpStatusCode.Forbidden},
            {typeof (WebException), HttpStatusCode.BadGateway},
            //{typeof (FaultException), HttpStatusCode.BadGateway},
            {typeof (KeyNotFoundException), HttpStatusCode.NotFound},
            {typeof (DBConcurrencyException), HttpStatusCode.Conflict},
            {typeof (NotImplementedException), HttpStatusCode.NotImplemented}
        };
        }

        public override void OnException(HttpActionExecutedContext context)
        {
            var exception = GetUnderlyingException(context);
            var responseCode = GetStatusCode(exception);
            var httpError = CreateHttpError(context, exception);

            var response = context.Request.CreateErrorResponse(
                responseCode,
                httpError);

            response.ReasonPhrase = exception.Message
                .Replace(Environment.NewLine, " ")
                .Replace("\r", " ")
                .Replace("\n", " ");

            context.Response = response;
        }

        private static HttpError CreateHttpError(
            HttpActionExecutedContext context,
            Exception exception)
        {
            var includeErrorDetail = context.Request.ShouldIncludeErrorDetail();
            return new HttpError(exception, includeErrorDetail)
            {
                Message = exception.Message
            };
        }

        private static Exception GetUnderlyingException(
            HttpActionExecutedContext context)
        {
            if (context == null || context.Exception == null)
                return null;

            var aggregateException = context.Exception as AggregateException;
            var exception = aggregateException != null
                ? aggregateException.GetBaseException()
                : context.Exception;

            return exception;
        }

        private static HttpStatusCode GetStatusCode(Exception exception)
        {
            if (exception == null)
                return HttpStatusCode.OK;

            var exceptionType = exception.GetType();

            if (TypeToCodeMap.ContainsKey(exceptionType))
                return TypeToCodeMap[exceptionType];

            foreach (var pair in TypeToCodeMap)
                if (pair.Key.IsAssignableFrom(exceptionType))
                    return pair.Value;

            return HttpStatusCode.InternalServerError;
        }
    }
}