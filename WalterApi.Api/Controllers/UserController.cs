using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WalterApi.Core.DTO_s.User;
using WalterApi.Core.Services;
using WalterApi.Core.Validations;

namespace WalterApi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        public UserController(UserService userService)
        {
            _userService = userService;
        }
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _userService.GetAllAsync();
            return Ok(result.Payload);
        }

        [HttpPost("AddUser")]
        public async Task<IActionResult> AddUser(CreateUserDTO model)
        {
            var validator = new CreateUserValidation();
            var validationReuslt = await validator.ValidateAsync(model);
            if (validationReuslt.IsValid)
            {
                var result = await _userService.AddNewUserAsync(model);
                if (result.Success)
                {
                    return Ok("User created");
                }
                else
                {
                    return Ok("Something went wrong!");
                }
            }

            return Ok(validationReuslt.Errors.ToList());
        }
        private async Task LoadRoles()
        {
            var result = await _userService.LoadRoles();
        }
        [HttpPost("EditUser")]
        public async Task<IActionResult> EditUser(string id)
        {
            await LoadRoles();
            var user = await _userService.GetUserForEditAsync(id);
            if (user.Success)
            {
                return Ok(user.Payload);
            }
            return Ok(user.Errors.ToList());
        }
        [HttpPost("DeleteUser")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (result.Success)
            {
                return Ok("User deleted!");
            }
            return Ok(result.Errors.ToList());
        }
    }
}
