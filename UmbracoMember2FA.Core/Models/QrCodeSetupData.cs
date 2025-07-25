using Google.Authenticator;

namespace UmbracoMember2FA.Core.Models
{
    /// <summary>
    /// Model with the required data to setup the authentication app.
    /// </summary>
    public class QrCodeSetupData
    {
        /// <summary>
        /// The secret unique code for the user and this ITwoFactorProvider.
        /// </summary>
        public string? Secret { get; init; }

        /// <summary>
        /// The SetupCode from the GoogleAuthenticator code.
        /// </summary>
        public SetupCode? SetupCode { get; init; }
    }
}
