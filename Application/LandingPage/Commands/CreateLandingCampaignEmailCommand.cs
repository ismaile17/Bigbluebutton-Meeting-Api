using Application.LandingPage.Models;
using Application.Services.ClientInfoService;
using Application.Shared.Results;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;

namespace Application.LandingPage.Commands
{
    public class CreateLandingCampaignEmailCommand : IRequest<ResultSingle<LandingCampaignEmailDto>>
    {
        public string Email { get; set; }
    }

    public class CreateLandingCampaignEmailCommandHandler : IRequestHandler<CreateLandingCampaignEmailCommand, ResultSingle<LandingCampaignEmailDto>>
    {
        private readonly IRepository<LandingCampaignEmail> _landingCampaignEmailRepository;
        private readonly IMapper _mapper;
        private readonly TelegramService _telegramService;
        private readonly IClientInfoService _clientInfoService;


        // Rate limiting parametreleri
        private const int RateLimitSeconds = 60; // 1 dakika

        public CreateLandingCampaignEmailCommandHandler(IRepository<LandingCampaignEmail> landingCampaignEmailRepository, IMapper mapper, TelegramService telegramService, IClientInfoService clientInfoService)
        {
            _landingCampaignEmailRepository = landingCampaignEmailRepository;
            _mapper = mapper;
            _telegramService = telegramService;
            _clientInfoService = clientInfoService;
        }

        public async Task<ResultSingle<LandingCampaignEmailDto>> Handle(CreateLandingCampaignEmailCommand request, CancellationToken cancellationToken)
        {

            if(request.Email == null)
            {
                return Result.Fail<LandingCampaignEmailDto>(null, "Email Boş");
            }

            // Email adresinin daha önce kayıtlı olup olmadığını kontrol et
            var existingEmail = _landingCampaignEmailRepository.GetMany(x => x.Email == request.Email).FirstOrDefault();
            if (existingEmail != null)
            {
                return ResultSingle<LandingCampaignEmailDto>.Ok((LandingCampaignEmailDto)null, "Kampanya emaili başarıyla oluşturuldu.");
            }


            // 1. İstemcinin IP adresini al
            var (ipAddress, port) = _clientInfoService.GetClientIpAndPort();

            if (string.IsNullOrEmpty(ipAddress))
            {
                return Result.Fail<LandingCampaignEmailDto>(null, "IP adresi belirlenemedi.");

            }

            var oneMinuteAgo = DateTime.UtcNow.AddSeconds(-RateLimitSeconds);
            var recentEntries = await Task.FromResult(_landingCampaignEmailRepository.GetMany(
            x => x.IPAddress == ipAddress && x.CreatedTime >= oneMinuteAgo));

            if (recentEntries.Any())
            {
                return Result.Fail<LandingCampaignEmailDto>(null, "Çok fazla istek gönderildi. Lütfen biraz bekleyin.");
            }

            // 3. LandingCampaignEmail entity'sini oluştur
            var landingCampaignEmail = new LandingCampaignEmail
            {
                Email = request.Email,
                IPAddress = ipAddress +"/"+ port,               
            };

            // 4. Veritabanına ekle (commit yapmadan)
            _landingCampaignEmailRepository.InsertWithoutCommit(landingCampaignEmail);

            // 5. Tüm değişiklikleri veritabanına kaydetme (Commit işlemi)
            var commitResult = await _landingCampaignEmailRepository.CommitAsync(cancellationToken);
            if (commitResult == -1)
            {
                return Result.Fail<LandingCampaignEmailDto>(null, "Kayıt edilemedi.");
            }

            // 6. DTO'ya map et
            var landingCampaignEmailDto = _mapper.Map<LandingCampaignEmailDto>(landingCampaignEmail);

            // 7. Telegram mesajını hazırla
            _ = _telegramService.SendMessageAsync($"NEW CAMPAIGN EMAIL:\r\n Email: {request.Email}");


            // 9. Başarılı sonucu döndür
            return ResultSingle<LandingCampaignEmailDto>.Ok(landingCampaignEmailDto, "Kampanya emaili başarıyla oluşturuldu.");
        }
    }
}
