using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Extensions;
using UmbracoMember2FA.Core.TwoFactorProviders;

namespace UmbracoMember2FA.Core
{
    public class Startup : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            // Register the ITwoFactorProvider implementation
            var identityBuilder = new MemberIdentityBuilder(builder.Services);
            identityBuilder.AddTwoFactorProvider<UmbracoMemberAppAuthenticator>(UmbracoMemberAppAuthenticator.Name);
        }
    }
}
