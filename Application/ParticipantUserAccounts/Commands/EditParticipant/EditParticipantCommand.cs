using Application.ParticipantAccounts.Model;
using Application.Shared.Results;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Participants.Commands.EditParticipant
{
    public class EditParticipantCommand : IRequest<ResultSingle<ParticipantUserDto>>
    {
        public int Id { get; set; }
        public int AppUserId { get; set; }
        public string Email { get; set; } // Email adresi sadece tanımlama için kullanılacak, değiştirilemeyecek.
        public string NameSurname { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? SpecialDescription { get; set; }
        public bool? IsActive { get; set; }
        public bool? ExpiryDateIsActive { get; set; }
        public string? PasswordEdit { get; set; }
        public List<int?> ParticipantMeetingGroups { get; set; }

    }

    public class EditParticipantCommandHandler : IRequestHandler<EditParticipantCommand, ResultSingle<ParticipantUserDto>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IRepository<MeetingGroup> _meetingGroupRepository;
        private readonly IMapper _mapper;

        public EditParticipantCommandHandler(UserManager<AppUser> userManager, IRepository<MeetingGroup> meetingGroupRepository, IMapper mapper)
        {
            _userManager = userManager;
            _meetingGroupRepository = meetingGroupRepository;
            _mapper = mapper;
        }

        public async Task<ResultSingle<ParticipantUserDto>> Handle(EditParticipantCommand request, CancellationToken cancellationToken)
        {

            request.Email = $"{request.Email.ToLower().Trim()}_PARTICIPANT";
            var participant = await _userManager.Users
                .Include(u => u.ManagerParticipants)
                .Include(u => u.MeetingGroupUserLists)
                .ThenInclude(mgul => mgul.MeetingGroup)
                .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());

        

            if (participant == null)
            {
                return Result.Fail<ParticipantUserDto>(null, "Kullanıcı bulunamadı.");
            }


            var participantManagerData = participant.ManagerParticipants.FirstOrDefault(a => a.ParticipantId == participant.Id && a.ManagerId == request.AppUserId);

            if(participantManagerData == null)
            {
                return Result.Fail<ParticipantUserDto>(null, "Yönetici bulunamadı.");
            }


            // Şifre güncelleme işlemi
            if (!string.IsNullOrEmpty(request.PasswordEdit))
            {
                // Şifre uzunluğu kontrolü
                if (request.PasswordEdit.Length < 6)
                {
                    return Result.Fail<ParticipantUserDto>(null, "Şifre en az 6 karakter olmalıdır.");
                }

                // Şifrede boşluk karakteri kontrolü
                if (request.PasswordEdit.Contains(" "))
                {
                    return Result.Fail<ParticipantUserDto>(null, "Şifre boşluk içeremez.");
                }

                // Şifreyi güncelle
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(participant);
                var resetPasswordResult = await _userManager.ResetPasswordAsync(participant, resetToken, request.PasswordEdit);

                if (!resetPasswordResult.Succeeded)
                {
                    return Result.Fail<ParticipantUserDto>(null, "Şifre güncellenirken bir hata oluştu.");
                }
            }


            participantManagerData.ExpiryDate = request.ExpiryDate;
            participantManagerData.SpecialDescription = request.SpecialDescription;
            participantManagerData.IsActive = request.IsActive ?? true;
            participantManagerData.ExpiryDateIsActive = request.ExpiryDateIsActive ?? false;
            participantManagerData.PhoneNumber = request.PhoneNumber;
            participantManagerData.NameSurname = request.NameSurname;

            if(request.ParticipantMeetingGroups.Count> 0) 
            { 
                var groupLists = participant.MeetingGroupUserLists;
                var deletedMatchData = groupLists.Select(a=>(int?)a.MeetingGroupId).Except(request.ParticipantMeetingGroups).ToList();

                foreach (var item in deletedMatchData)
                {
                    var participantGroups = groupLists.FirstOrDefault(a => a.MeetingGroupId == item);
                    participant.MeetingGroupUserLists.Remove(participantGroups);
                }

                var newMatchData = request.ParticipantMeetingGroups.Where(x => !groupLists.Any(z => z.MeetingGroupId == x)).ToList();
                foreach (var groupId in newMatchData)
                {
                    // Öncelikle, mevcut MeetingGroup nesnesini bulun.
                    var meetingGroup = _meetingGroupRepository.GetMany(mg => mg.Id == groupId.Value).FirstOrDefault();
                    if (meetingGroup != null)
                    {
                        var meetingGroupUserList = new MeetingGroupUserList
                        {
                            MeetingGroup = meetingGroup, // Doğru MeetingGroup nesnesi burada atanıyor.
                            AppUserId = participant.Id
                        };
                        participant.MeetingGroupUserLists.Add(meetingGroupUserList);
                    }
                }
            }

            var result = await _userManager.UpdateAsync(participant);

            if (!result.Succeeded)
            {
                return Result.Fail<ParticipantUserDto>(null,"Kullanıcı güncellenirken bir hata oluştu.");
            }


            var userDto = _mapper.Map<ParticipantUserDto>(participant);
            return Result.Ok<ParticipantUserDto>(userDto, $"Kullanıcı başarıyla güncellendi.");
        }
    }
}