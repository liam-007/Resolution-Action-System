namespace MeetingMinutes.Models
{
    public class MeetingItem
    {
        public int MeetingItemId { get; set; }
        public string Title { get; set; }
        public string ResponsiblePerson { get; set; }
        public string Description { get; set; }

        public ICollection<MeetingItemStatus> StatusHistory { get; set; }
    }
}
