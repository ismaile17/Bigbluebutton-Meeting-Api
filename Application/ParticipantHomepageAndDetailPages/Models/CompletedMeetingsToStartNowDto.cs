using Shared.Enum;

namespace Application.ParticipantHomepageAndDetailPages.Models
{
    public class CompletedMeetingsToStartNowDto //burayı direkt sadece completed i döndürmek için kullanacağız.
    {
        public int MeetingId { get; set; }
        public string MeetingName { get; set; }
        public string MeetingDescription { get; set; }
        public string? CompletedMeetingGuid { get; set; }
        public GuestPolicy? GuestPolicy { get; set; }
        public CompletedMeetingCreateStartOrFinish? CreateStartOrFinish { get; set; }
    }
}