namespace Domain.Entities
{
    public class MeetingGroup : BaseEntity
    {
    
        public int UserId { get; set; }
        public virtual AppUser User { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? SpecialDescription { get; set; }
        public string? Image { get; set; }

        public virtual ICollection<MeetingMeetingGroup> MeetingMeetingGroups { get; set; }
        public virtual ICollection<MeetingGroupUserList> MeetingGroupUserLists { get; set; }
        public virtual ICollection<TaskAssignment> TaskAssignments { get; set; }
       // public virtual ICollection<ManagerParticipant> ManagerParticipants { get; set; }

    }
}
