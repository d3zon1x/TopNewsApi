using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalterApi.Core.DTO_s.User;
using WalterApi.Core.Entities.User;

namespace WalterApi.Core.Services
{
    public class UserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IMapper _mapper;
        private readonly EmailService _emailService;
        private readonly IConfiguration _config;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserService(RoleManager<IdentityRole> roleManager, IConfiguration configuration, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IMapper mapper, EmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
            _config = configuration;
            _emailService = emailService;
            _roleManager = roleManager;
        }


        public async Task<ServiceResponse> GetAllAsync()
        {
            List<AppUser> users = await _userManager.Users.ToListAsync();
            List<UsersDto> mappedUsers = users.Select(u => _mapper.Map<AppUser, UsersDto>(u)).ToList();

            for (int i = 0; i < users.Count; i++)
            {
                mappedUsers[i].Role = (await _userManager.GetRolesAsync(users[i])).FirstOrDefault();
            }


            return new ServiceResponse
            {
                Success = true,
                Message = "All users loaded.",
                Payload = mappedUsers
            };
        }

        public async Task LogoutUserAsync()
        {
            await _signInManager.SignOutAsync();
        }
        public async Task SendConfirmationEmailAsync(AppUser user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = Encoding.UTF8.GetBytes(token);
            var validEmailToken = WebEncoders.Base64UrlEncode(encodedToken);

            string url = $"{_config["HostSetting:URL"]}/Dashboard/ConfirmEmail?userId={user.Id}&token={validEmailToken}";
            string emailBody = $"<h1>Confirm your email please.</h1><a href='{url}'>Confirm now</a>";
            await _emailService.SendEmail(user.Email, "TopNews Email confirmation", emailBody);
        }
        public async Task<ServiceResponse> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "unknown user"
                };
            }
            var decodedToken = WebEncoders.Base64UrlDecode(token);
            string normalToken = Encoding.UTF8.GetString(decodedToken);
            var result = await _userManager.ConfirmEmailAsync(user, normalToken);
            if (result.Succeeded)
            {
                return new ServiceResponse
                {
                    Success = true,
                    Message = "User`s email confirmed succesfully"
                };
            }
            return new ServiceResponse
            {
                Success = false,
                Message = "User`s email not confirmed",
                Payload = result.Errors.Select(e => e.Description)
            };
        }
        public async Task<ServiceResponse> AddNewUserAsync(CreateUserDTO user)
        {
            if (user != null)
            {
                //AppUser mappedUser = User.Select(u => _mapper.Map<AppUser, UsersDTO>(u)).ToList();
                AppUser mappedUser = _mapper.Map<CreateUserDTO, AppUser>(user);
                var result = await _userManager.CreateAsync(mappedUser, user.Password);
                if (result.Succeeded)
                {
                    _userManager.AddToRoleAsync(mappedUser, user.Role).Wait();

                    //  Email sender
                    //await _emailService.SendEmail(user.Email, "Welcome", "Welcome to our site");
                    await SendConfirmationEmailAsync(mappedUser);

                    return new ServiceResponse
                    {
                        Success = true,
                        Message = "New user succesfully added.",
                        Payload = user
                    };
                }
                else
                {
                    List<IdentityError> errors = result.Errors.ToList();
                    string error = "";
                    foreach (var err in errors)
                    {
                        error = error + err.Description.ToList();
                    }
                    return new ServiceResponse
                    {
                        Success = false,
                        Message = "Errors.",
                        Payload = error
                    };
                }
            }
            return new ServiceResponse
            {
                Success = false,
                Message = "Something went wrong during adding user :( ."
            };

        }
        public async Task<ServiceResponse> EditUser(EditUserDTO model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            if (user.Email != model.Email)
            {
                user.EmailConfirmed = false;
                user.Email = model.Email;
                user.UserName = model.Email;
                await SendConfirmationEmailAsync(user);
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;

            var roles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, roles);

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);

                return new ServiceResponse
                {
                    Message = "User successfully updated.",
                    Success = true
                };
            }

            List<IdentityError> errorList = result.Errors.ToList();
            string errors = "";

            foreach (var error in errorList)
            {
                errors = errors + error.Description.ToString();
            }
            return new ServiceResponse
            {
                Message = errors,
                Success = false
            };
        }
        public async Task<ServiceResponse> GetUserForEditAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "unknown user"
                };
            }

            EditUserDTO mappedUser = _mapper.Map<AppUser, EditUserDTO>(user);

            return new ServiceResponse
            {
                Success = true,
                Message = "User mapped.",
                Payload = mappedUser
            };
        }
        public async Task<ServiceResponse> DeleteUserAsync(string id)
        {
            AppUser user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "unknown user"
                };
            }
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return new ServiceResponse
                {
                    Success = true,
                    Message = "User deleted succesfully!"
                };
            }
            else
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "something went wrong during deleting user",
                    Payload = result.Errors.Select(e => e.Description)
                };
            }
        }

        public async Task<List<IdentityRole>> LoadRoles()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return roles;
        }
    }
}
