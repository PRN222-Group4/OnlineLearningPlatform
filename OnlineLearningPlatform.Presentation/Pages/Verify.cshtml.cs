using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Responses.Certificate;

namespace OnlineLearningPlatform.Presentation.Pages
{
    [AllowAnonymous]
    public class VerifyModel : PageModel
    {
        private readonly ICertificateService _certificateService;

        public VerifyModel(ICertificateService certificateService)
        {
            _certificateService = certificateService;
        }

        public bool IsSearched { get; set; } = false;
        public bool IsValid { get; set; } = false;

        public CertificateVerificationResponse? CertificateInfo { get; set; }
        public string CertCode { get; set; } = "";
        public string ErrorMessage { get; set; } = "";

        public async Task<IActionResult> OnGetAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                IsSearched = false;
                return Page();
            }

            IsSearched = true;
            CertCode = code.ToUpper().Trim();

            var response = await _certificateService.VerifyCertificateAsync(CertCode);

            if (response.IsSuccess && response.Result != null)
            {
                IsValid = true;
                CertificateInfo = (CertificateVerificationResponse)response.Result;
            }
            else
            {
                IsValid = false;
                ErrorMessage = response.ErrorMessage ?? "Chứng chỉ không hợp lệ.";
            }

            return Page();
        }
    }
}