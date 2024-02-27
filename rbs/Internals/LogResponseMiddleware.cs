using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

//http://www.sulhome.com/blog/10/log-asp-net-core-request-and-response-using-middleware
public class LogResponseMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LogResponseMiddleware> _logger;
    private Func<string, Exception, string> _defaultFormatter = (state, exception) => state;

    public LogResponseMiddleware(RequestDelegate next, ILogger<LogResponseMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var bodyStream = context.Response.Body;
        var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        await _next(context);

        responseBodyStream.Seek(0, SeekOrigin.Begin);
        var responseBody = new StreamReader(responseBodyStream).ReadToEnd();
        if (context.Response.ContentType != "application/x-msdownload" 
            && context.Response.ContentType != "application/x-apple-aspen-config; iOSinstallApp=true"
            && context.Response.ContentType != "application/pdf"
            && context.Response.ContentType != "application/jpg"
            && context.Response.ContentType != "application/png"
            && context.Response.ContentType != "application/image")
        {
            _logger.Log(LogLevel.Information, 1, $"RESPONSE LOG: {responseBody}", null, _defaultFormatter);
            Logger.Log("RESPONSE_MIDDLEWARE", $"RESPONSE LOG: {responseBody}");
        }
        responseBodyStream.Seek(0, SeekOrigin.Begin);
        await responseBodyStream.CopyToAsync(bodyStream);
    }
}
