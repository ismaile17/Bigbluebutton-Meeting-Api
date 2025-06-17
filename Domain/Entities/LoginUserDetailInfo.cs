namespace Domain.Entities
{
    public class LoginUserDetailInfo:BaseEntity
    {
        public string MeetingGuid { get; set; }
        public int MeetingId { get; set; }
        public string? IpAddress { get; set; }
        public int? Port { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
    }
}
