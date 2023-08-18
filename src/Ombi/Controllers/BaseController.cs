
using System.Linq;
using Microsoft.AspNetCore.Mvc;

public class BaseController : Controller
{
    protected string GetRequestIP()
    {
        string ip = null;

        if (Request.HttpContext?.Request?.Headers != null && Request.HttpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
        {
            var forwardedip = Request.HttpContext.Request.Headers["X-Forwarded-For"].ToString();
            ip = forwardedip.TrimEnd(',').Split(",").Select(s => s.Trim()).FirstOrDefault();
        }

        if (string.IsNullOrWhiteSpace(ip) && Request.HttpContext?.Connection?.RemoteIpAddress != null)
            ip = Request.HttpContext.Connection.RemoteIpAddress.ToString();

        if (string.IsNullOrWhiteSpace(ip) && Request.HttpContext?.Request?.Headers != null && Request.HttpContext.Request.Headers.ContainsKey("REMOTE_ADDR"))
        {
            var remoteip = Request.HttpContext.Request.Headers["REMOTE_ADDR"].ToString();
            ip = remoteip.TrimEnd(',').Split(",").Select(s => s.Trim()).FirstOrDefault();
        }

        return ip;
    }
}