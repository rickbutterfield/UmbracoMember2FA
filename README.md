# Umbraco Member Two-Factor Authentication (2FA) with Recovery Codes

Reference implementation and code examples for implementing two-factor authentication for Umbraco 13 members, including recovery codes support. This repository accompanies a detailed blog post explaining how to extend Umbraco's built-in 2FA functionality with proper recovery code generation and validation.

This is reference code to go along with the blog post.

It uses the [Clean](https://github.com/prjseal/Clean) starter kit and [uSync](https://github.com/KevinJump/uSync) for quick running of the code.

## 🚀 Features

- **App-based 2FA**: Support for authenticator apps (Google Authenticator, Microsoft Authenticator, etc.)
- **QR Code Setup**: Easy setup with QR code generation for authenticator apps
- **Recovery Codes**: Secure backup codes for account recovery when authenticator is unavailable
- **Custom Controllers**: Full control over login, registration, and 2FA flows
- **Existing Member Support**: Handles both new registrations and existing member accounts

## 📋 Prerequisites

- Umbraco 13.x
- .NET 8
- SQL Server or SQLite database

## 🛠 Getting Started

This repository contains reference code to accompany the blog post on implementing 2FA for Umbraco members. Use this code as a starting point for your own implementation.

1. **Review the Implementation**: Examine the code structure and implementation patterns
2. **Copy Relevant Code**: Take the components you need for your own project
3. **Adapt to Your Needs**: Modify the code to fit your specific requirements

With uSync out of the box, you can use the following credentials to log in:

Username: `admin@example.com`  
Password: `1234567890`

### Key Files to Review

- `UmbracoMemberAppAuthenticator.cs` - The core 2FA provider implementation
- `LoginController.cs` - Login flow with 2FA and recovery code support  
- `RegisterController.cs` - Registration with 2FA setup
- Models folder - Required data models for 2FA functionality

## 💡 Key Implementation Details

### Recovery Codes Support

The main challenge addressed in this project is enabling recovery codes for Umbraco members. This requires:

1. **Adding External Login Reference**: 
   ```csharp
   await _memberManager.AddLoginAsync(identityUser, 
       new UserLoginInfo("[AspNetUserStore]", identityUser.Key.ToString(), "[AspNetUserStore]"));
   ```

2. **For New Registrations**: Add the login reference during member creation
3. **For Existing Members**: Check and add the reference during login if missing

### Login Flow with 2FA

```csharp
var result = await _memberSignInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, false);

if (result.RequiresTwoFactor)
{
    // Handle 2FA requirement
    var providerNames = await _twoFactorLoginService.GetEnabledTwoFactorProviderNamesAsync(memberIdentityUser.Key);
    ViewData.SetTwoFactorProviderNames(providerNames);
    // Redirect to 2FA verification page
}
```

### Recovery Code Validation

```csharp
public async Task<IActionResult> UseRecoveryCode(RecoveryModel model)
{
    var user = await _memberSignInManager.GetTwoFactorAuthenticationUserAsync();
    var recoveryCode = model.RecoveryCode.Replace(" ", string.Empty);
    var result = await _umbracoMemberSignInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);
    
    if (result.Succeeded)
    {
        return LocalRedirect(model.RedirectUrl ?? Url.Content("~/"));
    }
    // Handle errors...
}
```

## 🤝 Contributing

This repository serves as reference code for the blog post on Umbraco member 2FA implementation. It's based on real-world client requirements and provides:

- **Working Examples**: Proven code patterns that solve real problems
- **Learning Resource**: Detailed implementation for educational purposes
- **Starting Point**: Foundation code for your own 2FA implementations

Feel free to:
- Use this code in your own projects
- Report issues with the examples
- Suggest improvements to the reference implementation
- Share your own adaptations and improvements

## 📖 Additional Resources

- [Umbraco 2FA Documentation](https://docs.umbraco.com/umbraco-cms/13.latest/reference/security/two-factor-authentication)
- [ASP.NET Core Identity 2FA](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/2fa)
- [Google Authenticator Library](https://github.com/BrandonPotter/GoogleAuthenticator)

## 📝 License

This reference code is provided as-is for educational and reference purposes. Use at your own discretion in production environments.
