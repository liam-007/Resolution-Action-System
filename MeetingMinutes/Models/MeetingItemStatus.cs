namespace MeetingMinutes.Models
{
    public class MeetingItemStatus
    {
        public int StatusId { get; set; }
        public int MeetingId { get; set; }
        public int MeetingItemId { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
        public DateTime UpdatedOn { get; set; }

        public Meeting Meeting { get; set; }
        public MeetingItem MeetingItem { get; set; }
    }
}
