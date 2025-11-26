using MeetingMinutes.Models;

namespace MeetingMinutes.ViewModels
{
    public class CreateMeetingViewModel
    {
       
        public int MeetingTypeId { get; set; }
        public DateTime MeetingDate { get; set; }

        
        public string MeetingCode { get; set; }

        
        public List<MeetingType> MeetingTypes { get; set; }

        
        public List<MeetingItemSelectionViewModel> PreviousItems { get; set; }
        public DateTime? PreviousMeetingDate { get; set; }


        public CreateMeetingViewModel()
        {
            MeetingTypes = new List<MeetingType>();
            PreviousItems = new List<MeetingItemSelectionViewModel>();
        }
    }
}
