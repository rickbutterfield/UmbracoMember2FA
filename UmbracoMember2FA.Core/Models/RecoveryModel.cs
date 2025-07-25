using System.ComponentModel.DataAnnotations;

namespace UmbracoMember2FA.Core.Models
{
    public class RecoveryModel
    {
        [Display(Name = "Recovery Code")]
        public string? RecoveryCode { get; set; }

        public string? RedirectUrl { get; set; }
    }
}
