using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MeetingMinutes.Data;
using MeetingMinutes.Models;
using MeetingMinutes.ViewModels;

namespace MeetingMinutes.Controllers
{
    public class MeetingController : Controller
    {
        // DB context for working with the MeetingMinutes database
        private readonly MeetingMinutesContext _context;



        // Logger so I can log any errors that happen in the controller
        private readonly ILogger<MeetingController> _logger;

        public MeetingController(MeetingMinutesContext context,
                                 ILogger<MeetingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        

        // Use Case 1: Capture New Meeting (GET)
       
        [HttpGet]
        public IActionResult Create()
        {
            try
            {
                // When the user first opens the Create screen it will show today's date and the list of meeting types
                var vm = new CreateMeetingViewModel
                {
                    MeetingDate = DateTime.Today,
                    MeetingTypes = _context.MeetingTypes.ToList()
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                // If something goes wrong loading the page, I log it and send the user back to the Home page with a friendly message
                _logger.LogError(ex, "Error loading Create meeting screen.");
                TempData["ErrorMessage"] = "Could not load the create meeting screen. Please try again.";
                return RedirectToAction("Index", "Home");
            }
        }



        
        // Use Case 1: Load previous items for selected meeting type (POST)

        [HttpPost]
        public IActionResult LoadItems(CreateMeetingViewModel model)
        {
            try
            {
                // After a POST, the dropdown loses its values, so it always reloads the list of meeting types for the view
                model.MeetingTypes = _context.MeetingTypes.ToList();

                /* 
                 As soon as the user picks a meeting type, it generates the next
                 meeting code for that type it then removes the model error on MeetingCode because I'm setting it myself here
                */

                if (model.MeetingTypeId > 0)
                {
                    model.MeetingCode = GenerateMeetingCode(model.MeetingTypeId);
                    ModelState.Remove(nameof(model.MeetingCode));
                }
                /*
                Here I load ALL items from ALL previous meetings of this type
                I start from MeetingItemStatuses so that meetings with no items
                are automatically ignored.
                */
                var allPreviousItems = _context.MeetingItemStatuses
                    .Include(s => s.MeetingItem)
                    .Include(s => s.Meeting)
                    .Where(s => s.Meeting.MeetingTypeId == model.MeetingTypeId)
                    .OrderByDescending(s => s.Meeting.MeetingDate)
                    .ToList();

                if (allPreviousItems.Any())
                {
                    // Map the status rows into the selection view model so the user can tick which items to carry forward into the new meeting
                    model.PreviousItems = allPreviousItems
                        .Select(s => new MeetingItemSelectionViewModel
                        {
                            MeetingItemId = s.MeetingItemId,
                            Title = s.MeetingItem.Title,
                            ResponsiblePerson = s.MeetingItem.ResponsiblePerson,
                            LastStatus = s.Status,
                            IsSelected = false
                        })
                        .ToList();

                    // I show the date of the most recent meeting just as extra info
                    model.PreviousMeetingDate = allPreviousItems.First().Meeting.MeetingDate;
                }
                else
                {
                    /* 
                     If there were no previous items for this type, it still allows
                     the user to create a meeting, but It lets them know why there
                     are no carry-forward items
                    */
                    ViewBag.Info = "No previous meeting items found for this type.";
                    model.PreviousItems = new List<MeetingItemSelectionViewModel>();
                }

                // It return the same Create view, now with PreviousItems loaded.
                return View("Create", model);
            }
            catch (Exception ex)
            {
                // If something fails here (e.g. DB issue), I log it and show an error message on the Create screen
                _logger.LogError(ex, "Error loading previous items for MeetingTypeId {MeetingTypeId}.", model.MeetingTypeId);
                ModelState.AddModelError(string.Empty,
                    "Something went wrong while loading previous items. Please try again.");

                // Makes sure the dropdown is still populated even on an error.
                model.MeetingTypes = _context.MeetingTypes.ToList();
                return View("Create", model);
            }
        }

        


        // Use Case 1: Actually create the meeting (POST)
       
       [HttpPost]
        public IActionResult CreateMeeting(CreateMeetingViewModel model)
        {
            // First I check if the basic model is valid If not, it reloads the meeting types and shows the Create view again
            if (!ModelState.IsValid)
            {
                model.MeetingTypes = _context.MeetingTypes.ToList();
                return View("Create", model);
            }

            try
            {
                // It always generates the meeting code here based on the type, so the code stays consistent and auto-increments properly
                var code = GenerateMeetingCode(model.MeetingTypeId);

                var meeting = new Meeting
                {
                    MeetingTypeId = model.MeetingTypeId,
                    MeetingCode = code,
                    MeetingDate = model.MeetingDate
                };

                _context.Meetings.Add(meeting);
                _context.SaveChanges();

                /* 
                 If the user selected any previous items to carry forward,
                 it creates a new MeetingItemStatus row for each one pointing to the new meeting
                */
                if (model.PreviousItems != null)
                {
                    foreach (var item in model.PreviousItems.Where(i => i.IsSelected))
                    {
                        var status = new MeetingItemStatus
                        {
                            MeetingId = meeting.MeetingId,
                            MeetingItemId = item.MeetingItemId,
                            Status = item.LastStatus ?? "Open",
                            Comment = "Carried forward from previous meeting",
                            UpdatedOn = DateTime.Now
                        };

                        _context.MeetingItemStatuses.Add(status);
                    }

                    _context.SaveChanges();
                }

                // After creating the meeting, it sends the user to the Details page so they can see the meeting and its items.
                return RedirectToAction("Details", new { id = meeting.MeetingId });
            }
            catch (Exception ex)
            {
                // If something goes wrong while saving, it logs the error and shows a friendly message on the Create screen.
                _logger.LogError(ex, "Error creating meeting for MeetingTypeId {MeetingTypeId}.", model.MeetingTypeId);

                ModelState.AddModelError(string.Empty,
                    "Something went wrong while creating the meeting. Please try again.");

                model.MeetingTypes = _context.MeetingTypes.ToList();
                return View("Create", model);
            }
        }



        
        // Meeting Details (GET)

        [HttpGet]
        public IActionResult Details(int id)
        {
            try
            {
                // Here I loaded the meeting and included the meeting type and the item statuses with their linked meeting items
                var meeting = _context.Meetings
                    .Include(m => m.MeetingType)
                    .Include(m => m.ItemStatuses)
                        .ThenInclude(s => s.MeetingItem)
                    .FirstOrDefault(m => m.MeetingId == id);

                if (meeting == null)
                {
                    // If the meeting can't be found, it just returns NotFound.
                    return NotFound();
                }

                return View(meeting);
            }
            catch (Exception ex)
            {
                // Logs any unexpected error and send the user back to the list with a general error message.
                _logger.LogError(ex, "Error loading Details for MeetingId {MeetingId}.", id);
                TempData["ErrorMessage"] = "Could not load the meeting details. Please try again.";
                return RedirectToAction("Index");
            }
        }



        
        // Add a new item to an existing meeting (POST)

        [HttpPost]
        public IActionResult AddItem(int meetingId, string title, string responsiblePerson, string description)
        {
            // Basic validation for new items, I kept it simple and just checked title and responsible person
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(responsiblePerson))
            {
                TempData["ErrorMessage"] = "Title and Responsible Person are required.";
                return RedirectToAction("Details", new { id = meetingId });
            }

            try
            {
                // 1) First it creates the MeetingItem record that describes the item
                var item = new MeetingItem
                {
                    Title = title,
                    ResponsiblePerson = responsiblePerson,
                    Description = description
                };

                _context.MeetingItems.Add(item);
                _context.SaveChanges();

                // 2) Then it links that item to the current meeting with a status row
                var status = new MeetingItemStatus
                {
                    MeetingId = meetingId,
                    MeetingItemId = item.MeetingItemId,
                    Status = "Open", // new items start as Open by default
                    Comment = "Added in this meeting",
                    UpdatedOn = DateTime.Now
                };

                _context.MeetingItemStatuses.Add(status);
                _context.SaveChanges();

                // 3) Finally it redirects back to the Details page so the user can immediately see the new item in the list
                return RedirectToAction("Details", new { id = meetingId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to MeetingId {MeetingId}.", meetingId);
                TempData["ErrorMessage"] = "Could not add the item. Please try again.";
                return RedirectToAction("Details", new { id = meetingId });
            }
        }

        

        // Use Case 5: List and filter existing meetings (GET)

        [HttpGet]
        public IActionResult Index(int? meetingTypeId)
        {
            try
            {
                // It loads all meeting types for the filter dropdown
                var types = _context.MeetingTypes.ToList();
                ViewBag.MeetingTypes = types;

                // Base query for meetings, ordered from newest to oldest
                var query = _context.Meetings
                    .Include(m => m.MeetingType)
                    .OrderByDescending(m => m.MeetingDate)
                    .AsQueryable();

                // Optional filter by meeting type if the user selected one
                if (meetingTypeId.HasValue && meetingTypeId.Value > 0)
                {
                    query = query.Where(m => m.MeetingTypeId == meetingTypeId.Value);
                }

                var meetings = query.ToList();
                return View(meetings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading meetings list.");
                TempData["ErrorMessage"] = "Could not load the meetings list. Please try again.";

                // On error it just returns an empty list so the view can still render.
                return View(new List<Meeting>());
            }
        }



        
        // Use Case 5: Load the Update Status screen (GET)

        [HttpGet]
        public IActionResult UpdateStatus(int statusId)
        {
            try
            {
                // Here I loaded the specific status row and include the linked item so I can show the item title/responsible in the view
                var status = _context.MeetingItemStatuses
                    .Include(s => s.MeetingItem)
                    .FirstOrDefault(s => s.StatusId == statusId);

                if (status == null)
                {
                    return NotFound();
                }

                return View(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading UpdateStatus for StatusId {StatusId}.", statusId);
                TempData["ErrorMessage"] = "Could not load the status screen. Please try again.";
                return RedirectToAction("Index");
            }
        }



        
        // Use Case 5: Save updated status (POST)

        [HttpPost]
        public IActionResult UpdateStatus(MeetingItemStatus model)
        {
            try
            {
                // Instead of only updating one row, it updates aLL statuses for this MeetingItem so the status stays consistent wherever it appears
                var statuses = _context.MeetingItemStatuses
                    .Where(s => s.MeetingItemId == model.MeetingItemId)
                    .ToList();

                if (!statuses.Any())
                {
                    return NotFound();
                }

                var now = DateTime.Now;

                foreach (var s in statuses)
                {
                    s.Status = model.Status;
                    s.Comment = model.Comment;
                    s.UpdatedOn = now;
                }

                _context.SaveChanges();

                // After updating, it sends the user back to the meeting where they started the update
                return RedirectToAction("Details", new { id = model.MeetingId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for MeetingItemId {MeetingItemId}.", model.MeetingItemId);
                TempData["ErrorMessage"] = "Could not update the status. Please try again.";
                return RedirectToAction("Details", new { id = model.MeetingId });
            }
        }




        
        // Helper: Generate Meeting Code based on type

        private string GenerateMeetingCode(int meetingTypeId)
        {
            // I used a simple prefix per meeting type and then counted how many meetings of that type already exist to generate the next number
            string prefix = meetingTypeId switch
            {
                1 => "M", // MANCO
                2 => "F", // Finance
                3 => "P", // PTL
                _ => "X"  // Fallback in case of unknown type
            };

            var count = _context.Meetings
                .Count(m => m.MeetingTypeId == meetingTypeId);

            int nextNumber = count + 1;

            // Format as two digits, e.g. F01, F02, F10 etc.
            return prefix + nextNumber.ToString("D2");
        }




        
        // Helper: API endpoint for JS to get next code (GET)
        [HttpGet]
        public IActionResult GetNextCode(int meetingTypeId)
        {
            try
            {
                // If the type is invalid, it just returns an empty code
                if (meetingTypeId <= 0)
                {
                    return Json(new { code = "" });
                }

                // Otherwise it generates the code and returns it as JSON
                var code = GenerateMeetingCode(meetingTypeId);
                return Json(new { code });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating next code for MeetingTypeId {MeetingTypeId}.", meetingTypeId);
                return Json(new { code = "" });
            }
        }
    }
}
