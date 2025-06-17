using Application.CompletedMeetings.Model;
using Application.Services.ClientInfoService;
using Application.Shared.Results;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Shared.Enum;

namespace Application.CompletedMeeting.Queries.GetCompletedMeetingByUserId
{
    public class GetLoginCompletedMeetingQuery : IRequest<ResultSingle<LoginCompletedMeetingDto>>
    {
        public int? AppUserId { get; set; }
        public Guid CompletedMeetingGuid { get; set; }
    }

    public class GetLoginCompletedMeetingQueryHandler : IRequestHandler<GetLoginCompletedMeetingQuery, ResultSingle<LoginCompletedMeetingDto>>
    {
        private readonly IRepository<Domain.Entities.CompletedMeeting> _completedMeetingRepository;
        private readonly IRepository<Meeting> _meetingRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private readonly IClientInfoService _clientInfoService;

        public GetLoginCompletedMeetingQueryHandler(IRepository<Domain.Entities.CompletedMeeting> completedMeetingRepository, IRepository<Meeting> meetingRepository, UserManager<AppUser> userManager, IMapper mapper, IMemoryCache cache, IClientInfoService clientInfoService)
        {
            _completedMeetingRepository = completedMeetingRepository;
            _meetingRepository = meetingRepository;
            _userManager = userManager;
            _mapper = mapper;
            _cache = cache;
            _clientInfoService = clientInfoService;
        }

        public async Task<ResultSingle<LoginCompletedMeetingDto>> Handle(GetLoginCompletedMeetingQuery request, CancellationToken cancellationToken)
        {
            string identifier;

            var (ipAddress, port) = _clientInfoService.GetClientIpAndPort();


            // Kullanıcıyı tanımlayın (Authenticated kullanıcı için User ID, değilse IP adresi kullanabilirsiniz)
            var user = await _userManager.Users
                                         .FirstOrDefaultAsync(u => u.Id == request.AppUserId);

            if (user != null)
            {
                // Authenticated kullanıcılar için User ID alın
                identifier = user?.Id.ToString() ?? "unknown_user";
            }
            else
            {
                // Authenticated olmayan kullanıcılar için IP adresi alın
                identifier = ipAddress?.ToString() ?? "anonymous";
            }

            // Önbellek anahtarını oluşturun (kullanıcı kimliği ve toplantı GUID'si baz alınarak)
            string cacheKey = $"GetLoginCompletedMeeting_{identifier}_{request.CompletedMeetingGuid}";

            // Aynı kullanıcıdan gelen kısa süreli tekrar isteklerini engellemek için önbelleği kontrol edin
            if (_cache.TryGetValue(cacheKey, out _))
            {
                return Result.Fail<LoginCompletedMeetingDto>(null, $"Lütfen bekleyiniz.");
            }

            // Önbelleğe bu isteği ekleyin (örneğin, 2 saniye)
            _cache.Set(cacheKey, true, TimeSpan.FromSeconds(8));

            var completedMeeting = await _completedMeetingRepository
                .GetMany(a => a.CompletedGuid == request.CompletedMeetingGuid && a.IsActive == 1)
                .FirstOrDefaultAsync(cancellationToken);

            if (completedMeeting == null)
            {
                return Result.Fail<LoginCompletedMeetingDto>(null, $"Meeting yok.");
            }


            if (completedMeeting.GuestPolicy == GuestPolicy.ALWAYS_DENY && user == null )
            {
                var deny = new LoginCompletedMeetingDto
                {
                    CompletedGuid = request.CompletedMeetingGuid,
                    CreateStartOrFinish = 0,
                    GuestPolicy = GuestPolicy.ALWAYS_DENY.ToString()
                };

               return ResultSingle<LoginCompletedMeetingDto>.Ok(deny, "Başarılı");

            }




            // Entity'yi DTO'ya dönüştür
            var data = _mapper.Map<LoginCompletedMeetingDto>(completedMeeting);

            // Başarılı sonucu döndür
            return ResultSingle<LoginCompletedMeetingDto>.Ok(data, "Başarılı");
        }
    }
}
