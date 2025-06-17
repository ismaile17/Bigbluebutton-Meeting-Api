namespace Domain.Entities
{
    public class MeetingMeetingGroup
    {
        public int MeetingId { get; set; }
        public Meeting Meeting { get; set; }
        public int MeetingGroupId { get; set; }
        public MeetingGroup MeetingGroup { get; set; }
    }
}