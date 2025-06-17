using Application.Shared.Results;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Accounts.Services
{

    public interface ITimeZoneService
    {
        Task<string> GetUserTimeZoneAsync(int userId);
        DateTime ConvertToUserTimeZone(DateTime utcDateTime, string userTimeZoneId);
        Task<TimeSpan> ConvertUserTimeToUtcAsync(TimeSpan userTime, int userId);
        Task<TimeSpan> ConvertUtcTimeToUserTimeAsync(TimeSpan utcTime, int userId);
        Task<DateTime> ConvertUserDateTimeToUtcAsync(DateTime userDateTime, int userId);
    }
    public class TimeZoneService:ITimeZoneService
    {
        private readonly UserManager<AppUser> _userManager;

        public TimeZoneService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Kullanıcının zaman dilimini getirir. Bulunamazsa UTC döner.
        /// </summary>
        /// <param name="userId">Kullanıcının ID'si</param>
        /// <returns>Zaman dilimi kimliği (örneğin: Europe/Istanbul)</returns>
        public async Task<string> GetUserTimeZoneAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            return user?.TimeZoneId ?? "Turkey Standard Time"; // Kullanıcı bulunamazsa UTC döner.
        }

        /// <summary>
        /// UTC zamanını, kullanıcının zaman dilimine dönüştürür.
        /// </summary>
        /// <param name="utcDateTime">UTC tarih/saat</param>
        /// <param name="userTimeZoneId">Kullanıcının zaman dilimi kimliği</param>
        /// <returns>Kullanıcının yerel saatine dönüştürülmüş tarih/saat</returns>
        public DateTime ConvertToUserTimeZone(DateTime utcDateTime, string userTimeZoneId)
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(userTimeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZoneInfo);
        }

        /// <summary>
        /// Kullanıcının saat bilgisini UTC'ye dönüştürür.
        /// </summary>
        /// <param name="userTime">Kullanıcının saat bilgisi (TimeSpan)</param>
        /// <param name="userId">Kullanıcının ID'si</param>
        /// <returns>UTC zamanına dönüştürülmüş saat bilgisi (TimeSpan)</returns>
        public async Task<TimeSpan> ConvertUserTimeToUtcAsync(TimeSpan userTime, int userId)
        {
            var userTimeZoneId = await GetUserTimeZoneAsync(userId);
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(userTimeZoneId);

            var userDateTime = DateTime.UtcNow.Date.Add(userTime);
            var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(userDateTime, timeZoneInfo);

            return utcDateTime.TimeOfDay;
        }

        /// <summary>
        /// UTC saat bilgisini, kullanıcının zaman dilimine dönüştürür.
        /// </summary>
        /// <param name="utcTime">UTC saat bilgisi (TimeSpan)</param>
        /// <param name="userId">Kullanıcının ID'si</param>
        /// <returns>Kullanıcının zaman dilimine göre dönüştürülmüş saat bilgisi (TimeSpan)</returns>
        public async Task<TimeSpan> ConvertUtcTimeToUserTimeAsync(TimeSpan utcTime, int userId)
        {
            var userTimeZoneId = await GetUserTimeZoneAsync(userId);
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(userTimeZoneId);

            var utcDateTime = DateTime.UtcNow.Date.Add(utcTime);
            var userDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZoneInfo);

            return userDateTime.TimeOfDay;
        }

        /// <summary>
        /// Kullanıcının tarih ve saat bilgisini UTC'ye dönüştürür.
        /// </summary>
        /// <param name="userDateTime">Kullanıcının tarih ve saat bilgisi</param>
        /// <param name="userId">Kullanıcının ID'si</param>
        /// <returns>UTC tarih ve saat bilgisi</returns>
        public async Task<DateTime> ConvertUserDateTimeToUtcAsync(DateTime userDateTime, int userId)
        {
            var userTimeZoneId = await GetUserTimeZoneAsync(userId);
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(userTimeZoneId);

            return TimeZoneInfo.ConvertTimeToUtc(userDateTime, timeZoneInfo);
        }
    }
}
