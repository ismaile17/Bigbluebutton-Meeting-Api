namespace Domain.Entities
{
    public class MeetingModeratorList
    {
        public int MeetingId { get; set; }
        public Meeting Meeting { get; set; }
        public int AppUserId { get; set; }
        public AppUser AppUser { get; set; }
    }
}
