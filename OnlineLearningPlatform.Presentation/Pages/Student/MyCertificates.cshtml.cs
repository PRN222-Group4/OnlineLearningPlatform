using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Responses;

namespace OnlineLearningPlatform.Presentation.Pages.Student
{
    [Authorize(Roles = "Student")]
    public class MyCertificatesModel : PageModel
    {
        private readonly ICertificateService _certificateService;
        private readonly IClaimService _claimService;

        public MyCertificatesModel(ICertificateService certificateService, IClaimService claimService)
        {
            _certificateService = certificateService;
            _claimService = claimService;
        }

        public List<CertificateResponse> Certificates { get; set; } = new List<CertificateResponse>();

        public async Task<IActionResult> OnGetAsync()
        {
            var claim = _claimService.GetUserClaim();
            var res = await _certificateService.GetMyCertificatesAsync(claim.UserId);

            if (res.IsSuccess && res.Result != null)
            {
                Certificates = (List<CertificateResponse>)res.Result;
            }

            return Page();
        }
    }
}