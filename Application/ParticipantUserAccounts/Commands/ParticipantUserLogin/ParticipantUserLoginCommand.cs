using Application.Accounts.Commands.Login;
using Application.Accounts.Model;
using Application.Shared.Results;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Shared.Jwt;

namespace Application.ParticipantUserAccounts.Commands.ParticipantUserLogin
{
    public class ParticipantUserLoginCommand:IRequest<ResultSingle<UserDto>>
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class ParticipantUserLoginCommandHandler : IRequestHandler<ParticipantUserLoginCommand, ResultSingle<UserDto>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IOptions<JwtSettings> _jwtSettings;
        private readonly IMapper _mapper;
        private readonly MeetingDbContext _dbContext;

        public ParticipantUserLoginCommandHandler(UserManager<AppUser> userManager, IOptions<JwtSettings> jwtSettings, IMapper mapper, MeetingDbContext dbContext)
        {
            _userManager = userManager;
            _jwtSettings = jwtSettings;
            _mapper = mapper;
            _dbContext = dbContext;
        }

        public async Task<ResultSingle<UserDto>> Handle(ParticipantUserLoginCommand request, CancellationToken cancellationToken)
        {
            request.Email = $"{request.Email.ToLower().Trim()}_PARTICIPANT";

            var appUser = await _userManager.FindByEmailAsync(request.Email);

            if (appUser == null)
            {
                return Result.Fail<UserDto>(null, $"Bu mail adresi adresi kayıtlı değil");
            }

            if (!await _userManager.IsInRoleAsync(appUser, "Participant"))
            {
                return Result.Fail<UserDto>(null, $"Bu mail adresi katılımcı olarak kayıtlı değil. Lütfen Manager olarak giriş yapınız.");
            }

            if (appUser != null && await _userManager.CheckPasswordAsync(appUser, request.Password))
            {

                var data = await Task.Run(() => _mapper.Map<AppUser, UserDto>(appUser));
                data.Roles = new List<RoleDto>();
                var query = (from ur in _dbContext.UserRoles
                             join rle in _dbContext.Roles on ur.RoleId equals rle.Id into roles
                             from role in roles.DefaultIfEmpty()
                             where ur.UserId == appUser.Id
                             select new RoleDto { Name = role.Name, Id = role.Id }).ToList();


                data.Roles.AddRange(query);

                if (appUser.ChangePasswordTF)
                {
                    //Şifre değiştirme sayfasına yönlendirilecek mi yönlendirilmeyecek mi?
                    return Result.Fail<UserDto>(null, $"Mail adresine sahip kullanıcı şifresini değiştirmelidir.");
                }

                data.AccessToken = await GenerateToken(appUser, data.Roles);
                data.RefreshToken = await _userManager.GenerateUserTokenAsync(appUser, "meeting.api", "RefreshToken");

                await _userManager.SetAuthenticationTokenAsync(appUser, "meeting.api", "RefreshToken", data.RefreshToken);


                return Result.Ok(data, $"Giriş başarılı");
            }
            return Result.Fail<UserDto>(null, $"Mail adresi veya şifre yanlış.");

        }

        private async Task<string> GenerateToken(AppUser user, List<RoleDto> roles)
        {
            var token = new JwtTokenBuilder();
            token.AddSecurityKey(JwtSecurityKey.Create(_jwtSettings.Value.SecretKey));

            if (_jwtSettings.Value.ValidateIssuer)
            {
                token.AddIssuer(_jwtSettings.Value.ValidIssuer);
            }

            if (_jwtSettings.Value.ValidateAudience)
            {
                token.AddAudience(_jwtSettings.Value.ValidAudience);
            }
            var claims = new Dictionary<string, string>
                           {
                     { "name", user.UserName },
                     { "email", user.Email },
                    // { "profileName", GenerateProfileName(employee.FullName) },
                    // { "title", employee.Title == null ? "" : employee.Title.Name },
                     { "UserId", user.Id.ToString() },
                     { "role",string.Join(",",roles.Select(f => f.Name)) }
                            };


            token.AddClaims(claims);
            token.AddExpiry(_jwtSettings.Value.ExpiryInMinutes);
            token.Build();

            return await Task.FromResult(token.Build().Value);
        }

        private static string GenerateProfileName(string Name)
        {
            if (Name == null)
            {
                return null;
            }

            var tempName = Name.TrimStart().TrimEnd().Split(" ");
            return tempName.First().ToUpper()[0].ToString() + tempName.Last().ToUpper()[0].ToString();
        }
    }
    
}
