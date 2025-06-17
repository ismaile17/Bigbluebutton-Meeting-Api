using Application.Accounts.Model;
using Application.Shared.Mappings;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Accounts.Commands.Login
{
    public class RoleDto : IMapFrom<AppRole>
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<AppRole, RoleDto>()
                .ForMember(a => a.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(a => a.Name, opt => opt.MapFrom(s => s.Name));
        }
    }
}
