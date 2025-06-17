


using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Persistence.EntityFramework;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application.Shared.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection InjectApplicationServices(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
            //var validators = AssemblyScanner.FindValidatorsInAssemblyContaining<RegisterValidator>();
            //validators.ForEach(validator => services.AddSingleton(validator.InterfaceType, validator.ValidatorType));
            return services;
        }

        public static IServiceCollection AddExternalApplication(this IServiceCollection services)
        {
            services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            });
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddScoped<UserManager<AppUser>>();
            services.AddIdentity<AppUser, AppRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 4;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.User.AllowedUserNameCharacters = "-._@+'#!/^%{}*";
            })
            .AddEntityFrameworkStores<MeetingDbContext>()
            .AddDefaultTokenProviders()
            .AddTokenProvider("meeting.api", typeof(DataProtectorTokenProvider<AppUser>));

            return services;
        }

        public static IServiceCollection AddMyContext(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<DbContext>(provider => provider.GetService<MeetingDbContext>())
                    .AddDbContextPool<MeetingDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("Infrastructure")));

            return services;
        }
    }
}
