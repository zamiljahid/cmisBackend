using cmis.Manager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace cmis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly IMenuRepository _menuRepository;

        public MenuController(IMenuRepository menuRepository)
        {
            _menuRepository = menuRepository;
        }

        [HttpGet("MenusByRole")]
        public IActionResult GetMenusByRole(string role_id)
        {
            try
            {
                var menus = _menuRepository.GetMenusByRoleId(role_id);

                if (menus != null && menus.Count > 0)
                {
                    return StatusCode(StatusCodes.Status200OK, menus);
                }
                else
                {
                    return StatusCode(StatusCodes.Status404NotFound, "No menus found for the given role.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
        }
    }
}
