namespace cmis.Model
{
    public class ProfileModel
    {
        public string UserID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string RoleId { get; set; }
        public string Contact { get; set; }
        public string ProfilePicUrl { get; set; }
        public string MembershipId { get; set; }
        public int ClubId { get; set; } // New field for club ID
        public string ClubName { get; set; } // New field for club name
    }

    public class AnnouncementModel
    {
        public int AnnouncementId { get; set; }
        public string AnnouncementById { get; set; }
        public int ClubId { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AnnouncementText { get; set; }
        public string AnnouncementTitle { get; set; }
    }
}
