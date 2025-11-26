namespace MeetingMinutes.Models
{
    public class MeetingType
    {
        public int MeetingTypeId { get; set; }
        public string Name { get; set; }

        public ICollection<Meeting> Meetings { get; set; }
    }
}