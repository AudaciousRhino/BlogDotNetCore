using BlogDotNetCode.Models.Account;
using BlogDotNetCode.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogDotNetCode.Web.Controllers
{
    // API Controller will provide free functionality such as being able to validate our models, and route to this controller, e.g. http://localhost:5000/api/Account
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        // Create the dependencies:
        private readonly ITokenService _tokenService;
        private readonly UserManager<ApplicationUserIdentity> _userManager;
        private readonly SignInManager<ApplicationUserIdentity> _signInManager;

        // Create Constructor:
        public AccountController(
            ITokenService tokenService,
            UserManager<ApplicationUserIdentity> userManager,
            SignInManager<ApplicationUserIdentity> signInManager)
        {
            // Startup/ConfigureServices will resolve:
            // - TokenService, i.e. 'services.AddScoped<ITokenService, TokenService>();'
            // - SignInManager, i.e. '.AddSignInManager<SignInManager<ApplicationUserIdentity>>();'
            _tokenService = tokenService;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // Create first Endpoint = new User/registration (e.g. http://localhost:5000/api/Account/register [Post]).
        // NOTE: We need a plain text password on new User creation.
        [HttpPost("register")]
        public async Task<ActionResult<ApplicationUser>> Register(ApplicationUserCreate applicationUserCreate)
        {
            // For User Identity, password needs to be hashed because ApplicationUserIdentity will be saved to DB:
            var applicationUserIdentity = new ApplicationUserIdentity
            {
                Username = applicationUserCreate.Username,
                Email = applicationUserCreate.Email,
                Fullname = applicationUserCreate.Fullname
            };

            // Now we can use applicationUserIdentity with ASP.NET Core Identity:
            var result = await _userManager.CreateAsync(applicationUserIdentity, applicationUserCreate.Password);

            if (result.Succeeded)
            {
                // Send App User back to the UI, with a TOKEN to say that User Authentication was a success:
                applicationUserIdentity = await _userManager.FindByNameAsync(applicationUserCreate.Username);

                ApplicationUser applicationUser = new ApplicationUser()
                {
                    ApplicationUserId = applicationUserIdentity.ApplicationUserId,
                    Username = applicationUserIdentity.Username,
                    Email = applicationUserIdentity.Email,
                    Fullname = applicationUserIdentity.Fullname,
                    Token = _tokenService.CreateToken(applicationUserIdentity)
                };

                return Ok(applicationUser);
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApplicationUser>> Login(ApplicationUserLogin applicationUserLogin)
        {
            var applicationUserIdentity = await _userManager.FindByNameAsync(applicationUserLogin.Username);

            if (applicationUserIdentity != null)
            {
                // ASP.NET Core provides the 'CheckPasswordSignInAsync() method, we need to pass in 'applicationUserIdentity' and
                // (separately) the plain-text Password, since applicationUserIdentity doesn't have the plain-text password because it isn't secured like that:
                var result = await _signInManager.CheckPasswordSignInAsync(
                    applicationUserIdentity,
                    applicationUserLogin.Password, false);

                if (result.Succeeded)
                {
                    // Return the logged-in User:
                    ApplicationUser applicationUser = new ApplicationUser
                    {
                        ApplicationUserId = applicationUserIdentity.ApplicationUserId,
                        Username = applicationUserIdentity.Username,
                        Email = applicationUserIdentity.Email,
                        Fullname = applicationUserIdentity.Fullname,
                        Token = _tokenService.CreateToken(applicationUserIdentity)
                    };

                    return Ok(applicationUser);
                }
            }

            return BadRequest("Invalid login attempt.");
        }
    }
}
