namespace MeetingMinutes.ViewModels
{
    public class MeetingItemSelectionViewModel
    {
        public int MeetingItemId { get; set; }
        public string Title { get; set; }
        public string ResponsiblePerson { get; set; }
        public string LastStatus { get; set; }

        // checkbox
        public bool IsSelected { get; set; }
    }
}
