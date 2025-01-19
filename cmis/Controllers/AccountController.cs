using cmis.Manager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI;

namespace cmis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ILoginRepository _login_manager;
        public AccountController(ILoginRepository login_manager)
        {
            _login_manager = login_manager;
        }

        [HttpGet("Login")]
        public IActionResult Login(string user_id, string password)
        {

            try
            {
                
                var credential = _login_manager.GetLoginById(user_id);

                if (credential.password == password)
                {
                    var jwt = new Jwt();
                    var token = jwt.GenerateJwtToken();
                    return StatusCode(StatusCodes.Status200OK, token);
                }
                else
                {
                    return StatusCode(StatusCodes.Status401Unauthorized, "You are Unauthorized.");
                }

                
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
        }
    }
}
