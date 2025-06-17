using Application.LandingPage.Models;
using Application.Shared.Results;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.LandingPage.Commands
{
    public class CreateLandingContactFormCommand : IRequest<ResultSingle<LandingContactFormDto>>
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string? AboutYou { get; set; }
        public string Message { get; set; }
    }

    public class CreateLandingContactFormCommandHandler : IRequestHandler<CreateLandingContactFormCommand, ResultSingle<LandingContactFormDto>>
    {
        private readonly IRepository<LandingContactForm> _landingContactFormRepository;
        private readonly IMapper _mapper;
        private readonly TelegramService _telegramService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Rate limiting parametreleri
        private const int RateLimitSeconds = 60; // 1 dakika

        public CreateLandingContactFormCommandHandler(IRepository<LandingContactForm> landingContactFormRepository, IMapper mapper, TelegramService telegramService, IHttpContextAccessor httpContextAccessor)
        {
            _landingContactFormRepository = landingContactFormRepository;
            _mapper = mapper;
            _telegramService = telegramService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ResultSingle<LandingContactFormDto>> Handle(CreateLandingContactFormCommand request, CancellationToken cancellationToken)
        {
            // 1. İstemcinin IP adresini al
            var ipAddress = GetClientIpAddress();
            if (string.IsNullOrEmpty(ipAddress))
            {
                return Result.Fail<LandingContactFormDto>(null, "IP adresi belirlenemedi.");
            }

            // 2. Rate limiting kontrolü
            var oneMinuteAgo = DateTime.UtcNow.AddSeconds(-RateLimitSeconds);
            var recentEntries = await Task.FromResult(_landingContactFormRepository.GetMany(
            x => x.IPAddress == ipAddress && x.CreatedTime >= oneMinuteAgo));


            if (recentEntries.Any())
            {
                return Result.Fail<LandingContactFormDto>(null, "Çok fazla istek gönderildi. Lütfen biraz bekleyin.");
            }

            // 3. LandingContactForm entity'sini oluştur
            var landingContactForm = new LandingContactForm
            {
                Name = request.Name,
                Surname = request.Surname,
                Email = request.Email,
                AboutYou = request.AboutYou,
                Message = request.Message,
                CreatedTime = DateTime.UtcNow,
                IPAddress = ipAddress
            };

            // 4. Veritabanına ekle (commit yapmadan)
            _landingContactFormRepository.InsertWithoutCommit(landingContactForm);

            // 5. Tüm değişiklikleri veritabanına kaydetme (Commit işlemi)
            var commitResult = await _landingContactFormRepository.CommitAsync(cancellationToken);
            if (commitResult == -1)
            {
                return Result.Fail<LandingContactFormDto>(null, "Kayıt edilemedi.");
            }

            // 6. DTO'ya map et
            var landingContactFormDto = _mapper.Map<LandingContactFormDto>(landingContactForm);

            // 7. Telegram mesajını hazırla
            var message = $"*NEW CONTACT FORM SUBMISSION:*\n" +
                          $"*Name:* {landingContactForm.Name}\n" +
                          $"*Surname:* {landingContactForm.Surname}\n" +
                          $"*Email:* {landingContactForm.Email}\n" +
                          $"*About You:* {landingContactForm.AboutYou}\n" +
                          $"*Message:* {landingContactForm.Message}";

            // 8. Telegram mesajını gönder
            try
            {
                await _telegramService.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                // Opsiyonel: Hata loglama yapılabilir
                // Örneğin: _logger.LogError(ex, "Telegram mesajı gönderilirken hata oluştu.");
            }

            // 9. Başarılı sonucu döndür
            return ResultSingle<LandingContactFormDto>.Ok(landingContactFormDto, "İletişim formu başarıyla oluşturuldu.");
        }

        /// <summary>
        /// İstemcinin IP adresini alır.
        /// </summary>
        /// <returns>IP adresi string olarak döner.</returns>
        private string GetClientIpAddress()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return string.Empty;

            // Proxy arkasında çalışıyorsa X-Forwarded-For başlığını kontrol edin
            if (httpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    // Birden fazla IP adresi olabilir, ilkini alıyoruz
                    return forwardedFor.Split(',').First().Trim();
                }
            }

            return httpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        }
    }
}
