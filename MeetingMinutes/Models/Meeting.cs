namespace MeetingMinutes.Models
{
    public class Meeting
    {
        public int MeetingId { get; set; }
        public int MeetingTypeId { get; set; }
        public string MeetingCode { get; set; }
        public DateTime MeetingDate { get; set; }

        public MeetingType MeetingType { get; set; }
        public ICollection<MeetingItemStatus> ItemStatuses { get; set; }
    }
}
