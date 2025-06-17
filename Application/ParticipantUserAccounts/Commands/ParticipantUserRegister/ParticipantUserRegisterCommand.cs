using Application.ParticipantAccounts.Model;
using Application.Shared.Results;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.ParticipantAccounts.Commands.ParticipantRegister
{
    public class ParticipantUserRegisterCommand : IRequest<Result<ParticipantUserDto>>
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string? Phone { get; set; }
        public int ManagerId { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? SpecialDescription { get; set; }
        public string Password { get; set; }
        public bool? IsActive { get; set; }
        public bool? ExpiryDateIsActive { get; set; }
        public bool ChangePasswordTF { get; set; } = false;
        public List<int?> MeetingGroupLists { get; set; }

        //bu sayfa yeni participant user oluşturma sayfası, hata alınabilecek olan nokta ise Grubu eklerken o gruba daha önce kayıt edilmiş mi edilmemiş mi detayı.
        //Ancak kişi ilk defa oluşturulacağında buraya gireceğimiz için onu ekleyen ManagerId nin hiçbir grubuna zaten kayıt edilmemiş olacaktır.
        //O yüzden şimdilik bu detayı es geçiyorum. Edit participant user page de bunu daha detaylı yapacağız. 

        public class ParticipantUserRegisterCommanddHandler : IRequestHandler<ParticipantUserRegisterCommand, Result<ParticipantUserDto>>
        {
            private readonly UserManager<AppUser> _userManager;
            private readonly IRepository<MeetingGroup> _meetingGroupRepository;
            private readonly IMapper _mapper;
            private readonly IMediator mediator;

            public ParticipantUserRegisterCommanddHandler(UserManager<AppUser> userManager, IRepository<MeetingGroup> meetingGroupRepository, IMapper mapper, IMediator mediator)
            {
                _userManager = userManager;
                _meetingGroupRepository = meetingGroupRepository;
                _mapper = mapper;
                this.mediator = mediator;
            }

            public async Task<Result<ParticipantUserDto>> Handle(ParticipantUserRegisterCommand request, CancellationToken cancellationToken)
            {
                // E-posta kontrolü, kullanıcı oluşturma ve rol atama işlemleri...
                request.Email = $"{request.Email.ToLower().Trim()}_PARTICIPANT";

                //var appUser = await _userManager.FindByEmailAsync(request.Email.ToLower());
                var appUser = await _userManager.Users
                .Include(u => u.ManagerParticipants)
                .Include(u=>u.MeetingGroupUserLists)
                .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());

                if (appUser != null) //üyelik varsa
                {
                    //appUser.ManagerParticipants = new List<ManagerParticipant>();

                    var existingRecord = appUser.ManagerParticipants.FirstOrDefault(mp => mp.ParticipantId == appUser.Id && mp.ManagerId == request.ManagerId);


                    if (request.MeetingGroupLists.Count > 0)
                    {
                        var newMatchData = request.MeetingGroupLists.ToList();
                        foreach (var groupId in newMatchData)
                        {
                            // Öncelikle, mevcut MeetingGroup nesnesini bulun.
                            var meetingGroup = _meetingGroupRepository.GetById(groupId.Value);
                            if (meetingGroup != null && meetingGroup.UserId == request.ManagerId)
                            {
                                var meetingGroupUserList = new MeetingGroupUserList
                                {
                                    MeetingGroup = meetingGroup, // Doğru MeetingGroup nesnesi burada atanıyor.
                                    AppUserId = appUser.Id
                                };
                                appUser.MeetingGroupUserLists.Add(meetingGroupUserList);
                            }

                        }
                        await _userManager.UpdateAsync(appUser);
                    }


                    if (existingRecord == null)//üyelik var ve ara tabloda o managere ekli değilse.
                    {
                        var participantInfo = new ManagerParticipant
                        {
                            ManagerId = request.ManagerId,
                            AppUserId = request.ManagerId,
                            ParticipantId = appUser.Id,
                            ExpiryDate = request.ExpiryDate,
                            SpecialDescription = request.SpecialDescription,
                            IsActive = request.IsActive ?? true,
                            ExpiryDateIsActive = request.ExpiryDateIsActive ?? false,
                            NameSurname = request.FullName,
                            PhoneNumber = request.Phone,
                        };

                        appUser.ManagerParticipants.Add(participantInfo);
                        await _userManager.UpdateAsync(appUser);

                        return Result.Ok<ParticipantUserDto>($"Kişi sizin listenize kayıt edilmiştir.");
                    }

                    return Result.Ok<ParticipantUserDto>($"Kişi sizin listenize kayıt edilmiştir.");

                }
                if (appUser == null)//hiç üyelik yoksa.
                {

                    string Password = request.Password;
                    var createAppUser = await _userManager.CreateAsync(new AppUser { UserName = request.FullName, Email = request.Email.ToLower(), ChangePasswordTF = request.ChangePasswordTF, UserType = 0 }, Password);

                   
                    if (!createAppUser.Succeeded)
                    {
                        return Result.Fail<ParticipantUserDto>($"Kayıt olurken bir hatayla karşılaşıldı. Formu inceleyiniz");
                    }

                    var getAppUser = await _userManager.FindByEmailAsync(request.Email.ToLower());
                    if (getAppUser == null)
                    {
                        return Result.Fail<ParticipantUserDto>($"Kayıt olurken bir hatayla karşılaşıldı. Formu inceleyiniz");
                    }
                    var addRole = await _userManager.AddToRoleAsync(getAppUser, "Participant");
                    if (!addRole.Succeeded)
                    {
                        await _userManager.DeleteAsync(getAppUser);
                        return Result.Fail<ParticipantUserDto>($"Kayıt olurken bir hatayla karşılaşıldı. Lütfen iletişime geçiniz");
                    }

                    getAppUser.ManagerParticipants = new List<ManagerParticipant>(); 

                    var participantInfo2 = new ManagerParticipant//üyelik oluşturduktan sonra ara tabloya ekle.
                    {
                        ManagerId = request.ManagerId,
                        AppUserId = request.ManagerId,
                        ParticipantId = getAppUser.Id,
                        ExpiryDate = request.ExpiryDate,
                        SpecialDescription = request.SpecialDescription,
                        IsActive = request.IsActive ?? true,
                        ExpiryDateIsActive = request.ExpiryDateIsActive ?? false,
                        NameSurname = request.FullName,
                        PhoneNumber = request.Phone,
                    };

                    getAppUser.ManagerParticipants = new List<ManagerParticipant>();

                    if (getAppUser != null)
                    {
                        getAppUser.ManagerParticipants.Add(participantInfo2);

                        if (request.MeetingGroupLists.Count > 0)
                        {
                            foreach (var groupId in request.MeetingGroupLists)
                            {
                                if (groupId.HasValue)
                                {
                                    // Grup ID'sini kullanarak ilgili MeetingGroup nesnesini alın
                                    var meetingGroup = _meetingGroupRepository.GetById(groupId.Value);

                                    // MeetingGroup nesnesinin başarıyla alındığını kontrol edin
                                    if (meetingGroup != null && meetingGroup.UserId == request.ManagerId)
                                    {
                                        // MeetingGroupUserLists özelliğini başlatın, eğer başlatılmamışsa
                                        if (meetingGroup.MeetingGroupUserLists == null)
                                        {
                                            meetingGroup.MeetingGroupUserLists = new List<MeetingGroupUserList>();
                                        }

                                        // Yeni MeetingGroupUserList oluşturun ve MeetingGroup'a ekleyin
                                        var meetingGroupUserList = new MeetingGroupUserList
                                        {
                                            MeetingGroupId = meetingGroup.Id,
                                            AppUserId = getAppUser.Id,
                                        };

                                        // Oluşturulan MeetingGroupUserList'i MeetingGroup'a ekleyin
                                        meetingGroup.MeetingGroupUserLists.Add(meetingGroupUserList);
                                    }
                                }
                            }
                        }

                        await _userManager.UpdateAsync(getAppUser);
                    }

                    if (getAppUser == null)
                    {
                        await _userManager.DeleteAsync(getAppUser);
                        return Result.Fail<ParticipantUserDto>($"Kayıt başarısız.");
                    }

                    
                }
                return Result.Ok<ParticipantUserDto>($"Kayıt başarılı. Giriş yapabilirsiniz.");
            }
        }
        }
    }

