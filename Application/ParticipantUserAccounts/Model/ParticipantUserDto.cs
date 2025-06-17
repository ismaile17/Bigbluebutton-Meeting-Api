using Application.Groups.Model;
using Application.MeetingGroups.Model;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;

namespace Application.ParticipantAccounts.Model
{
    public class ParticipantUserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public Int16 UserType { get; set; }

        // ManagerParticipant'ları ManagerParticipantDto olarak tutacak
        public List<ManagerParticipantDto> ManagerParticipants { get; set; }

        public List<SelectedGroupDto> MeetingGroups { get; set; }

    }

    public class ManagerParticipantDto
    {
        public int ManagerId { get; set; }
        public int ParticipantId { get; set; }
        public string SpecialDescription { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public bool ExpiryDateIsActive { get; set; } = false;
        public string PhoneNumber { get; set; }
        public string NameSurname { get; set; }

        // Ekstra alanlar gerekirse buraya eklenebilir
    }

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<AppUser, ParticipantUserDto>()
            .ForMember(dest => dest.ManagerParticipants, opt => opt.MapFrom(src => src.ManagerParticipants.Select(mp => new ManagerParticipantDto
            {
                ManagerId = mp.ManagerId,
                ParticipantId = mp.ParticipantId,
                SpecialDescription = mp.SpecialDescription,
                ExpiryDate = mp.ExpiryDate ?? default(DateTime),
                ExpiryDateIsActive = mp.ExpiryDateIsActive ?? false,
                IsActive = mp.IsActive ?? default(bool),
                PhoneNumber = mp.PhoneNumber,
                NameSurname = mp.NameSurname

            })))
            .ForMember(dest => dest.MeetingGroups, opt => opt.MapFrom(src => src.MeetingGroupUserLists.Select(mgul => new SelectedGroupDto
            {
                Id = mgul.MeetingGroupId,
                Name = mgul.MeetingGroup.Name

            })));

        }
    }
}
