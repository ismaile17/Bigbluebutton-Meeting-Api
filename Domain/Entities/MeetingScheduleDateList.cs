namespace Domain.Entities
{
    public class MeetingScheduleDateList
    {
        public int MeetingId { get; set; }
        public Meeting Meeting { get; set; }
        public DateOnly Date { get; set; }
        public bool DidHappen { get; set; }
        public int CompletedMeetingId { get; set; }
        public DateTime StartDateTimeUtc { get; set; }

    }
}
