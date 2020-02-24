using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4;
using IdentityServer4.Events;
using IdentityServer4.Services;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjectIds.Models;

namespace ProjectIds.Controllers
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly TestUserStore _users;
        private readonly IEventService _events;
        private readonly IIdentityServerInteractionService _interaction;

        public AccountController(IIdentityServerInteractionService interaction, IEventService events, TestUserStore users = null)
        {
            _users = users ?? new TestUserStore(Config.Users());
            _events = events;
            _interaction = interaction;
        }

        [HttpGet]
        public IActionResult Login([FromQuery]string returnUrl)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
                return Redirect(returnUrl);

            LoginViewModel vm = new LoginViewModel()
            {
                ReturnUrl = returnUrl
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!_users.ValidateCredentials(model.Username, model.Password))
            {
                model.Username = string.Empty;
                model.Password = string.Empty;
                return View(model);
            }

            model.Password = string.Empty;

            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);
            var user = _users.FindByUsername(model.Username);
            await _events.RaiseAsync(new UserLoginSuccessEvent(user.Username, user.SubjectId, user.Username, clientId: context?.ClientId));

            AuthenticationProperties props = null;
            if (model.RememberLogin)
            {
                props = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
                };
            };
            var isuser = new IdentityServerUser(user.SubjectId)
            {
                DisplayName = user.Username
            };

            await HttpContext.SignInAsync(isuser, props);

            if (context != null)
                return Redirect(model.ReturnUrl);

            await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials", clientId: context?.ClientId));

            return View(model);
        }
    }
}