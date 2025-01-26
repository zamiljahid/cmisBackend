using cmis.Manager;
using cmis.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace cmis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClubController : ControllerBase
    {
        private readonly IClubRepository _clubRepository;

        public ClubController(IClubRepository clubRepository)
        {
            _clubRepository = clubRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllClubs()
        {
            try
            {
                var clubs = await _clubRepository.GetAllClubsAsync();

                if (clubs != null && clubs.Any())
                {
                    return StatusCode(StatusCodes.Status200OK, clubs);
                }
                else
                {
                    return StatusCode(StatusCodes.Status404NotFound, "No clubs found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("events/upcoming-ongoing")]
        public async Task<IActionResult> GetUpcomingAndOngoingEvents()
        {
            try
            {
                var events = await _clubRepository.GetUpcomingAndOngoingEventsAsync();

                if (events != null && events.Any())
                {
                    return StatusCode(StatusCodes.Status200OK, events);
                }
                else
                {
                    return StatusCode(StatusCodes.Status404NotFound, "No upcoming or ongoing events found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("events/pending")]
        public async Task<IActionResult> GetPendingEvents()
        {
            try
            {
                var events = await _clubRepository.GetPendingEventsAsync();

                if (events != null && events.Any())
                {
                    return StatusCode(StatusCodes.Status200OK, events);
                }
                else
                {
                    return StatusCode(StatusCodes.Status404NotFound, "No pending events found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("members")]
        public async Task<IActionResult> GetMembersByClubId(int clubId)
        {
            try
            {
                var members = await _clubRepository.GetMembersByClubIdAsync(clubId);

                if (members != null && members.Any())
                {
                    return StatusCode(StatusCodes.Status200OK, members);
                }
                else
                {
                    return StatusCode(StatusCodes.Status404NotFound, "No members found for the specified club.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("top-presidents")]
        public async Task<IActionResult> GetTopPresidents()
        {
            try
            {
                var presidents = await _clubRepository.GetTopPresidentsAsync();

                if (presidents != null && presidents.Any())
                {
                    return StatusCode(StatusCodes.Status200OK, presidents);
                }
                else
                {
                    return StatusCode(StatusCodes.Status404NotFound, "No presidents found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("top-clubs")]
        public async Task<IActionResult> GetTopClubs()
        {
            try
            {
                var topClubs = await _clubRepository.GetTopClubsAsync();

                if (topClubs != null && topClubs.Any())
                {
                    return StatusCode(StatusCodes.Status200OK, topClubs);
                }
                else
                {
                    return StatusCode(StatusCodes.Status404NotFound, "No top clubs found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        
        [HttpGet("elected-presidents")]
        public async Task<IActionResult> GetElectedPresidents()
        {
            try
            {
                var presidents = await _clubRepository.GetElectedPresidentsAsync();

                if (presidents != null && presidents.Any())
                {
                    return StatusCode(StatusCodes.Status200OK, presidents);
                }
                else
                {
                    return StatusCode(StatusCodes.Status404NotFound, "No elected presidents found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("election-results")]
        public async Task<IActionResult> GetElectionResults([FromQuery] int clubId)
        {
            try
            {
                var results = await _clubRepository.GetElectionResultsAsync(clubId);

                if (!results.Any())
                {
                    return NotFound(new { Message = "No election results found for the specified club." });
                }

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ex.Message });
            }
        }

        [HttpGet("GetCandidatesForVoting")]
        public async Task<IActionResult> GetCandidatesForVoting([FromQuery] int clubId)
        {
            try
            {
                if (clubId <= 0)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Invalid Club ID.");
                }

                var candidates = await _clubRepository.GetCandidatesForVotingAsync(clubId);

                if (candidates != null && candidates.Any())
                {
                    return StatusCode(StatusCodes.Status200OK, candidates);
                }
                else
                {
                    return StatusCode(StatusCodes.Status404NotFound, "No candidates found for this club.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpPost("SaveVote")]
        public IActionResult SaveVote([FromBody] VoteSaveModel voteModel)
        {
            if (voteModel == null)
            {
                return BadRequest("Invalid vote data.");
            }

            try
            {
                string result = _clubRepository.SaveVote(voteModel);
                return StatusCode(StatusCodes.Status200OK, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        [HttpPost("SaveAnnouncement")]
        public IActionResult SaveAnnouncement([FromBody] AnnouncementSaveModel announcementModel)
        {
            if (announcementModel == null)
            {
                return BadRequest("Invalid announcement data.");
            }

            try
            {
                string result = _clubRepository.SaveAnnouncement(announcementModel);
                return StatusCode(StatusCodes.Status200OK, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("SaveEventRegistration")]
        public IActionResult SaveEventRegistration([FromBody] EventRegistrationModel eventRegistrationModel)
        {
            if (eventRegistrationModel == null)
            {
                return BadRequest("Invalid user data.");
            }

            try
            {
                string result = _clubRepository.SaveEventRegistration(eventRegistrationModel);
                return StatusCode(StatusCodes.Status200OK, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("CreateEvent")]
        public IActionResult CreateEvent([FromBody] CreateEventModel createEventModel)
        {
            if (createEventModel == null)
            {
                return BadRequest("Invalid event data.");
            }

            try
            {
                string result = _clubRepository.CreateEvent(createEventModel);
                return StatusCode(StatusCodes.Status200OK, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("CreateClub")]
        public IActionResult CreateClub([FromBody] CreateClubModel createClubModel)
        {
            if (createClubModel == null)
            {
                return BadRequest("Invalid club data.");
            }

            try
            {
                string result = _clubRepository.CreateClub(createClubModel);
                return StatusCode(StatusCodes.Status200OK, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("UpdateEventStatus")]
        public IActionResult UpdateEventStatus([FromQuery] int eventId, [FromQuery] string action)
        {
            if (eventId <= 0 || string.IsNullOrEmpty(action))
            {
                return BadRequest("Invalid event ID or action.");
            }

            try
            {
                string result = _clubRepository.UpdateEventStatus(eventId, action);

                if (result == "Success")
                    return StatusCode(StatusCodes.Status200OK, new { Message = $"Success" });
                else if (result == "Invalid action. Must be 'approve' or 'reject'.")
                    return BadRequest(new { Message = result });
                else
                    return NotFound(new { Message = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Internal server error.", Details = ex.Message });
            }
        }

        [HttpGet("RegisterMember")]
        public IActionResult RegisterMember(string userId, int clubId)
        {
            string result = _clubRepository.RegisterClubMember(userId, clubId);

            if (result == "Success")
            {
                return Ok(new { message = "Success" });
            }
            else
            {
                return BadRequest(new { message = result });
            }
        }

        [HttpPut("UpdateUserRole")]
        public IActionResult UpdateUserRole([FromQuery] string userId, [FromQuery] string action)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(action))
            {
                return BadRequest("Invalid user ID or action.");
            }

            try
            {
                string result = _clubRepository.UpdateUserRole(userId, action);

                if (result == "Success")
                    return StatusCode(StatusCodes.Status200OK, new { Message = $"Success" });
                else if (result == "Invalid action. Must be 'Promote', 'Demote', or 'Remove'.")
                    return BadRequest(new { Message = result });
                else
                    return NotFound(new { Message = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Internal server error.",
                    Details = ex.InnerException?.Message ?? ex.Message
                });
            }
        }



        [HttpGet("CheckUserEventRegistration")]
        public IActionResult CheckUserEventRegistration([FromQuery] int eventId, [FromQuery] string userId)
        {
            if (eventId <= 0 || string.IsNullOrEmpty(userId))
            {
                return BadRequest("Event ID and User ID are required.");
            }

            try
            {
                bool isRegistered = _clubRepository.CheckIfUserRegisteredForEvent(eventId, userId);
                return Ok(new { isRegistered });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("CheckVoterExists")]
        public IActionResult CheckVoterExists([FromQuery] string voterId, [FromQuery] int electionId)
        {
            if (string.IsNullOrEmpty(voterId) || electionId <= 0)
            {
                return BadRequest("Voter ID and Election ID are required.");
            }

            try
            {
                bool exists = _clubRepository.CheckIfVoterExists(voterId, electionId);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("GetMessages")]
        public async Task<IActionResult> GetMessages([FromQuery] int clubId)
        {
            try
            {
                var messages = await _clubRepository.GetMessagesByClubIdAsync(clubId);
                if (messages == null) return NotFound();
                return Ok(messages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] Message message)
        {
            try
            {
                if (message == null) return BadRequest("Message data is required.");

                message.Timestamp = DateTime.UtcNow; 
                var messageId = await _clubRepository.AddMessageAsync(message);

                return CreatedAtAction(nameof(GetMessages), new { id = messageId }, new { messageId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("GetLatestElectionStatus")]
        public IActionResult GetLatestElectionStatus()
        {
            try
            {
                string status = _clubRepository.GetLatestElectionStatus();
                return Ok(status);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


    }
}
