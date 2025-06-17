using Microsoft.AspNetCore.Http;
using System.Net;

namespace Application.Services.ClientInfoService
{
    public interface IClientInfoService
    {
        (string IPAddress, int Port) GetClientIpAndPort();
    }
    public class ClientInfoService : IClientInfoService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TelegramService _telegramService;

        public ClientInfoService(IHttpContextAccessor httpContextAccessor, TelegramService telegramService)
        {
            _httpContextAccessor = httpContextAccessor;
            _telegramService = telegramService;
        }

        public (string IPAddress, int Port) GetClientIpAndPort()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                    return (string.Empty, 0);

                string ipAddress = string.Empty;
                int port = 0;

                if (httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedForValues))
                {
                    var forwardedFor = forwardedForValues.FirstOrDefault();
                    if (!string.IsNullOrEmpty(forwardedFor))
                    {
                        // Birden fazla IP adresi olabilir, ilkini alıyoruz
                        var ip = forwardedFor.Split(',').First().Trim();
                        if (IPAddress.TryParse(ip, out var parsedIp))
                        {
                            ipAddress = parsedIp.ToString();
                        }
                    }
                }

                if (string.IsNullOrEmpty(ipAddress))
                {
                    var remoteIp = httpContext.Connection.RemoteIpAddress;
                    if (remoteIp != null)
                    {
                        ipAddress = remoteIp.ToString();
                    }
                }

                port = httpContext.Connection.RemotePort;

                return (ipAddress, port);
            }
            catch (Exception ex)
            {
                var errorMessage = $"[ClientInfoService] GetClientIpAndPort sırasında bir hata oluştu: {ex.Message}";
                _telegramService.SendMessageAsync(errorMessage).GetAwaiter().GetResult();

                return (string.Empty, 0);
            }
        }
    }
}
