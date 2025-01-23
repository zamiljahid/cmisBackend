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
    }
}
