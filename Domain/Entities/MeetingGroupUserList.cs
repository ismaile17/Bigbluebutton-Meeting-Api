namespace Domain.Entities
{
    public class MeetingGroupUserList
    {
        public int AppUserId { get; set; }
        public AppUser AppUser { get; set; }
        public int MeetingGroupId { get; set; }
        public MeetingGroup MeetingGroup { get; set; }
    }
}
