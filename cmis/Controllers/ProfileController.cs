using cmis.Manager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace cmis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileRepository _profileManager;

        public ProfileController(IProfileRepository profileManager)
        {
            _profileManager = profileManager;
        }

        [HttpGet("UserProfile")]
        public IActionResult GetUserProfile(string user_id)
        {
            try
            {
                var userProfile = _profileManager.GetUserProfileById(user_id);

                if (userProfile != null)
                {
                    return StatusCode(StatusCodes.Status200OK, userProfile);
                }
                else
                {
                    return StatusCode(StatusCodes.Status404NotFound, "User not found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
        }
    }
}
