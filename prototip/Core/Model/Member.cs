using System;

namespace Core.Model
{
    public class Member(DateTime joinDate, DateTime? leaveDate, string groupId, string memberId)
    {
        private DateTime joinDate = joinDate;
        private DateTime? leaveDate = leaveDate;
        private string groupId = groupId;
        private string memberId = memberId;

        public DateTime JoinDate { get => joinDate; set => joinDate = value; }
        public DateTime? LeaveDate { get => leaveDate; set => leaveDate = value; }
        public string GroupId { get => groupId; set => groupId = value; }
        public string MemberId { get => memberId; set => memberId = value; }
    }
}
