using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Events;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectIds.Models;

namespace ProjectIds.Controllers
{
    [SecurityHeaders]
    [Authorize]
    public class ConsentController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IResourceStore _resourceStore;
        private readonly IEventService _events;

        public ConsentController(
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IResourceStore resourceStore,
            IEventService events)
        {
            _interaction = interaction;
            _clientStore = clientStore;
            _resourceStore = resourceStore;
            _events = events;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery]string returnUrl)
        {

            if (string.IsNullOrEmpty(User?.Identity.Name))
                return RedirectToAction("Login", "Account", new { returnUrl });

            var result = await ProcessConsent(returnUrl);
            if (result.HasValidationError)
                return RedirectToAction("Login", "Account", new { returnUrl });

            return Redirect(result.RedirectUri);
        }

        /// <summary>
        /// 请求验证过程
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        private async Task<ProcessConsentResult> ProcessConsent(string returnUrl)
        {
            var result = new ProcessConsentResult();

            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                result.ValidationError = "ReturnUrl Is Empty!";
                return result;
            }

            var request = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (request == null)
            {
                result.ValidationError = "Return Url Is Invalid!";
                return result;
            }

            result.ClientId = request.ClientId;
            result.RedirectUri = returnUrl;
            result.RememberConsent = false;

            var resources = await _resourceStore.FindEnabledResourcesByScopeAsync(request.ScopesRequested);
            if (resources == null)
            {
                result.ValidationError = "Resources Is Null!";
                return result;
            }
            if (!resources.IdentityResources.Any() && !resources.ApiResources.Any())
            {
                result.ValidationError = "IdentityResources && ApiResources Is Null!";
                return result;
            }

            // IdentityScopes
            result.IdentityScopes = resources.IdentityResources.Select(identity =>
            {
                return new ScopeViewModel
                {
                    Name = identity.Name,
                    DisplayName = identity.DisplayName,
                    Description = identity.Description,
                    Emphasize = identity.Emphasize,
                    Required = identity.Required,
                    Checked = true
                };
            }).ToArray();
            result.ScopesConsented.AddRange(result.IdentityScopes.Select(t => t.Name));

            // ResourceScopes
            result.ResourceScopes = resources.ApiResources.SelectMany(x => x.Scopes).Select(resource =>
            {
                return new ScopeViewModel
                {
                    Name = resource.Name,
                    DisplayName = resource.DisplayName,
                    Description = resource.Description,
                    Emphasize = resource.Emphasize,
                    Required = resource.Required,
                    Checked = true
                };
            }).ToArray();
            result.ScopesConsented.AddRange(result.ResourceScopes.Select(t => t.Name));

            if (!result.ScopesConsented.Any())
            {
                result.ValidationError = "ScopesConsented Is Null";
                return result;
            }

            // 已同意授权事件
            await _events.RaiseAsync(new ConsentGrantedEvent(User.Identity.Name, request.ClientId, request.ScopesRequested, result.ScopesConsented, result.RememberConsent));

            // 授权同意
            await _interaction.GrantConsentAsync(request, new ConsentResponse
            {
                RememberConsent = result.RememberConsent,
                ScopesConsented = result.ScopesConsented
            });

            return result;
        }
    }
}