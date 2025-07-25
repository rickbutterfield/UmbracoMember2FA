using Google.Authenticator;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using UmbracoMember2FA.Core.Models;

namespace UmbracoMember2FA.Core.TwoFactorProviders
{
    /// <summary>
    /// App Authenticator implementation of the ITwoFactorProvider
    /// </summary>
    public class UmbracoMemberAppAuthenticator : ITwoFactorProvider
    {
        /// <summary>
        /// The unique name of the ITwoFactorProvider. This is saved in a constant for reusability.
        /// </summary>
        public const string Name = "UmbracoMemberAppAuthenticator";

        private readonly IMemberService _memberService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoMemberAppAuthenticator"/> class.
        /// </summary>
        public UmbracoMemberAppAuthenticator(IMemberService memberService)
            => _memberService = memberService;

        /// <summary>
        /// The unique provider name of ITwoFactorProvider implementation.
        /// </summary>
        /// <remarks>
        /// This value will be saved in the database to connect the member with this  ITwoFactorProvider.
        /// </remarks>
        public string ProviderName => Name;

        /// <summary>
        /// Returns the required data to setup this specific ITwoFactorProvider implementation. In this case it will contain the url to the QR-Code and the secret.
        /// </summary>
        /// <param name="userOrMemberKey">The key of the user or member</param>
        /// <param name="secret">The secret that ensures only this user can connect to the authenticator app</param>
        /// <returns>The required data to setup the authenticator app</returns>
        public Task<object> GetSetupDataAsync(Guid userOrMemberKey, string secret)
        {
            var member = _memberService.GetByKey(userOrMemberKey);

            var applicationName = "My Application Name";
            var twoFactorAuthenticator = new TwoFactorAuthenticator();

            SetupCode setupInfo = twoFactorAuthenticator.GenerateSetupCode(applicationName, member.Username, secret, false);

            return Task.FromResult<object>(new QrCodeSetupData()
            {
                SetupCode = setupInfo,
                Secret = secret
            });
        }

        /// <summary>
        /// Validated the code and the secret of the user.
        /// </summary>
        public bool ValidateTwoFactorPIN(string secret, string code)
        {
            var twoFactorAuthenticator = new TwoFactorAuthenticator();
            return twoFactorAuthenticator.ValidateTwoFactorPIN(secret, code);
        }

        /// <summary>
        /// Validated the two factor setup
        /// </summary>
        /// <remarks>Called to confirm the setup of two factor on the user. In this case we confirm in the same way as we login by validating the PIN.</remarks>
        public bool ValidateTwoFactorSetup(string secret, string token) => ValidateTwoFactorPIN(secret, token);
    }
}
