using Application.Accounts.Services;
using Application.FileSystem.Models;
using Application.Shared.Results;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Enum;

namespace Application.FileSystem.Queries
{
    public class GetFileListQuery : IRequest<Result<FileListDto>>
    {
        public int AppUserId { get; set; }
        public int PageId { get; set; }
        public int? ParticipantIdForTeacher { get; set; }
        public FileSystemPageType PageType { get; set; }
    }

    public class GetFileListQueryHandler : IRequestHandler<GetFileListQuery, Result<FileListDto>>
    {
        private readonly IRepository<Domain.Entities.FileSystem> _fileSystemRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly IRepository<MeetingGroup> _meetingGroupRepository;
        private readonly IRepository<ManagerParticipant> _managerParticipantRepository;
        private readonly IMapper _mapper;
        private readonly ITimeZoneService _timeZoneService;


        public GetFileListQueryHandler(IRepository<Domain.Entities.FileSystem> fileSystemRepository, UserManager<AppUser> userManager, IRepository<MeetingGroup> meetingGroupRepository, IRepository<ManagerParticipant> managerParticipantRepository, IMapper mapper, ITimeZoneService timeZoneService)
        {
            _fileSystemRepository = fileSystemRepository;
            _userManager = userManager;
            _meetingGroupRepository = meetingGroupRepository;
            _managerParticipantRepository = managerParticipantRepository;
            _mapper = mapper;
            _timeZoneService = timeZoneService;
        }

        public async Task<Result<FileListDto>> Handle(GetFileListQuery request, CancellationToken cancellationToken)
        {
            // Kullanıcıyı AppUser tablosundan getiriyoruz
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == request.AppUserId);

            if (user == null)
            {
                return Result.Fail<FileListDto>("Kullanıcı bulunamadı.");
            }

            // Kullanıcı rollerini alıyoruz
            var roles = await _userManager.GetRolesAsync(user);
            IQueryable<Domain.Entities.FileSystem> fileQuery;

            if (request.PageType == FileSystemPageType.Logo)
            {
                // Teacher rolündeyse CreatedBy o mu diye kontrol ediyoruz
                fileQuery = _fileSystemRepository.GetMany(f =>
                    f.PageId == request.PageId &&
                    f.PageType == FileSystemPageType.Logo &&
                    f.CreatedBy == request.AppUserId &&
                    !f.IsDeleted);
            }
            else if (roles.Contains("Admin"))
            {
                // Burasını sonradan teacher rolü olarak düzenleyeceğiz.
                fileQuery = _fileSystemRepository.GetMany(f =>
                    f.PageId == request.PageId &&
                    f.PageType == request.PageType &&
                    !f.IsDeleted && f.CreatedBy == request.AppUserId);
            }
            else if (request.PageType == FileSystemPageType.GroupTeacher)
            {
                // Kullanıcının yöneticisi gruba bağlı mı kontrol ediyoruz
                var managerParticipants = await _managerParticipantRepository
                    .GetMany(mp => mp.ParticipantId == request.AppUserId)
                    .ToListAsync(cancellationToken);

                if (!managerParticipants.Any())
                {
                    return Result.Fail<FileListDto>("Dosya bulunamadı.");
                }

                var managerIds = managerParticipants.Select(mp => mp.ManagerId).ToList();

                // MeetingGroupUserLists üzerinden grubu alıyoruz
                var meetingGroup = await _meetingGroupRepository.GetAll
                    .Include(g => g.MeetingGroupUserLists)
                    .FirstOrDefaultAsync(g => g.Id == request.PageId &&
                                              managerIds.Contains(g.UserId) && // Birden fazla manager ID'ye göre kontrol
                                              g.MeetingGroupUserLists.Any(ul => ul.AppUserId == request.AppUserId),
                                          cancellationToken);

                if (meetingGroup == null)
                {
                    return Result.Fail<FileListDto>("Dosya bulunamadı.");
                }

                // Gruba bağlı dosyaları getiriyoruz
                fileQuery = _fileSystemRepository.GetMany(f =>
                    f.PageId == request.PageId &&
                    f.PageType == FileSystemPageType.GroupTeacher &&
                    !f.IsDeleted);
            }
            else
            {
                return Result.Fail<FileListDto>("Dosya bulunamadı.");
            }

            var fileList = await fileQuery.ToListAsync(cancellationToken);

            if (!fileList.Any())
            {
                return Result.Fail<FileListDto>("Dosya bulunamadı.");
            }

            var fileListDto = _mapper.Map<List<FileListDto>>(fileList);

            var userTimeZoneId = await _timeZoneService.GetUserTimeZoneAsync(request.AppUserId);

            foreach (var file in fileListDto)
            {
                file.CreatedTime = _timeZoneService.ConvertToUserTimeZone(file.CreatedTime, userTimeZoneId);
            }

            return Result.Ok(fileListDto, "Başarılı");
        }

    }

}
