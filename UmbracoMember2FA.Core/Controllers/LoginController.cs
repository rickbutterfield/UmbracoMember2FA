using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.ActionsResults;
using Umbraco.Cms.Web.Common.Models;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Extensions;
using UmbracoMember2FA.Core.Models;

namespace UmbracoMember2FA.Core.Controllers
{
    public class LoginController : SurfaceController
    {
        private readonly IMemberManager _memberManager;
        private readonly IMemberSignInManager _memberSignInManager;
        private readonly ITwoFactorLoginService _twoFactorLoginService;
        public readonly MemberSignInManager _umbracoMemberSignInManager;

        public LoginController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IPublishedUrlProvider publishedUrlProvider,
            IMemberSignInManager memberSignInManager,
            IMemberManager memberManager,
            ITwoFactorLoginService twoFactorLoginService,
            IMemberUserStore memberUserStore,
            UserManager<MemberIdentityUser> memberUserManager,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<MemberIdentityUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<MemberIdentityUser>> logger,
            IAuthenticationSchemeProvider schemes,
            IUserConfirmation<MemberIdentityUser> confirmation,
            IMemberExternalLoginProviders memberExternalLoginProviders,
            IEventAggregator eventAggregator,
            IOptions<SecuritySettings> securitySettings,
            IRequestCache requestCache)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _memberSignInManager = memberSignInManager;
            _memberManager = memberManager;
            _twoFactorLoginService = twoFactorLoginService;

            _umbracoMemberSignInManager = new MemberSignInManager(
                memberUserManager,
                contextAccessor,
                claimsFactory,
                optionsAccessor,
                logger,
                schemes,
                confirmation,
                memberExternalLoginProviders,
                eventAggregator,
                securitySettings,
                requestCache);
        }

        public async Task<IActionResult> Login([Bind(Prefix = "loginModel")] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            Microsoft.AspNetCore.Identity.SignInResult signInResult = await _memberSignInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, false);

            if (signInResult.Succeeded)
            {
                TempData["LoginSuccess"] = true;
                if (!model.RedirectUrl.IsNullOrWhiteSpace())
                {
                    return Redirect(Url.IsLocalUrl(model.RedirectUrl) ? model.RedirectUrl : CurrentPage.AncestorOrSelf(1).Url(PublishedUrlProvider));
                }

                return RedirectToCurrentUmbracoUrl();
            }

            if (signInResult.RequiresTwoFactor)
            {
                MemberIdentityUser? memberIdentityUser = await _memberManager.FindByNameAsync(model.Username);

                if (memberIdentityUser == null)
                {
                    return new ValidationErrorResult("No local member found for username " + model.Username);
                }

                // If you've already got a large number of users, I would suggest adding this code on your login 
                var existingLogins = await _memberManager.GetLoginsAsync(memberIdentityUser);
                var existing2faLogin = existingLogins.FirstOrDefault(x => x.LoginProvider == "[AspNetUserStore]");

                if (existing2faLogin == null)
                {
                    await _memberManager.AddLoginAsync(memberIdentityUser, new UserLoginInfo("[AspNetUserStore]", memberIdentityUser.Key.ToString(), "[AspNetUserStore]"));
                }

                IEnumerable<string> providerNames = await _twoFactorLoginService.GetEnabledTwoFactorProviderNamesAsync(memberIdentityUser.Key);
                ViewData.SetTwoFactorProviderNames(providerNames);
            }
            else if (signInResult.IsLockedOut)
            {
                ModelState.AddModelError("loginModel", "Member is locked out");
            }
            else if (signInResult.IsNotAllowed)
            {
                ModelState.AddModelError("loginModel", "Member is not allowed");
            }
            else
            {
                ModelState.AddModelError("loginModel", "Invalid username or password");
            }

            return CurrentUmbracoPage();
        }

        public async Task<IActionResult> UseRecoveryCode([Bind(Prefix = "recoveryModel")] RecoveryModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            var user = await _memberSignInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException("Unable to load two-factor authentication user.");
            }

            var recoveryCode = model.RecoveryCode.Replace(" ", string.Empty);
            var result = await _umbracoMemberSignInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            await _memberManager.GetUserIdAsync(user);

            if (result.Succeeded)
            {
                return LocalRedirect(model.RedirectUrl ?? Url.Content("~/"));
            }
            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
                return CurrentUmbracoPage();
            }
        }
    }
}
