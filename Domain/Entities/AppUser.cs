using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
    public class AppUser : IdentityUser<int>
    {
        public Int16 UserType { get; set; } // 0 = Student 1 = Teacher
        public virtual ICollection<UserRecovery> UserRecoveries { get; set; }
        public virtual ICollection<MeetingGroupUserList> MeetingGroupUserLists { get; set; }
        public virtual ICollection<MeetingModeratorList> MeetingModeratorLists { get; set; }
        public virtual ICollection<ManagerParticipant> ManagerParticipants { get; set; }
        public virtual UserDetailSetting UserDetailSetting { get; set; }
        public bool ChangePasswordTF { get; set; } = false;
        public int? BBBServerId { get; set; }
        public DateTime? PackageEndDate { get; set; }
        public DateTime? PackageBuyDate { get; set; }
        public string? CheckMessage { get; set; }

        public virtual ICollection<Purchase> Purchases { get; set; }

        public int? PackageId { get; set; }
        public string? TimeZoneId { get; set; }
        public string? FullName { get; set; }


        public virtual Package Package { get; set; }
    }

}
