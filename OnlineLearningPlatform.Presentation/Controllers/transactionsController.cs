

using Microsoft.AspNetCore.Mvc;
using PayOS;
using PayOS.Models.Webhooks;
using OnlineLearningPlatform.BusinessObject.IServices;
using Microsoft.Extensions.Configuration;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace OnlineLearningPlatform.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class transactionsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _config;
        private readonly ILogger<transactionsController> _logger;

        public transactionsController(IPaymentService paymentService, IConfiguration config, ILogger<transactionsController> logger)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [AllowAnonymous]
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook([FromBody] Webhook webhook)
        {
            // Basic validation: ensure payload exists
            if (webhook == null || webhook.Data == null)
                return BadRequest("Invalid webhook payload");

            // Verify signature/checksum if provided via headers or webhook model
            // PayOS typically provides a checksum in header or payload; use configured checksum key to verify
            var checksumKey = _config.GetValue<string>("PayOS:ChecksumKey");

            // If webhook model contains Signature or Checksum, validate it
            var valid = true;
            try
            {
                // Attempt verification if SDK provides helper
                if (!string.IsNullOrEmpty(checksumKey))
                {
                    // Many PayOS implementations include a signature field; if present, verify
                    var signature = Request.Headers["X-Signature"].ToString();
                    if (string.IsNullOrEmpty(signature) && !string.IsNullOrEmpty(webhook.Signature))
                        signature = webhook.Signature;

                    if (!string.IsNullOrEmpty(signature))
                    {
                        // compute HMAC SHA256 of raw body using checksumKey
                        Request.EnableBuffering();
                        using var reader = new System.IO.StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
                        Request.Body.Position = 0;
                        var body = await reader.ReadToEndAsync();
                        Request.Body.Position = 0;
                        using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(checksumKey));
                        var computed = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(body))).ToLower();
                        valid = string.Equals(computed, signature.Replace("sha256=", ""), StringComparison.OrdinalIgnoreCase);
                    }
                }
            }
            catch
            {
                valid = false;
            }

            if (!valid)
                return Unauthorized("Invalid signature");

            // Map to SDK WebhookData model if needed. The PayOS SDK Webhook model may differ; our PaymentService expects WebhookData
            var data = new WebhookData
            {
                OrderCode = webhook.Data?.OrderCode ?? 0,
                Code = webhook.Data?.Code,
                Reference = webhook.Data?.Reference,
                CounterAccountNumber = webhook.Data?.CounterAccountNumber,
                CounterAccountName = webhook.Data?.CounterAccountName,
                CounterAccountBankName = webhook.Data?.CounterAccountBankName
            };

            await _paymentService.HandlePayOSWebhookAsync(data);

            // Acknowledge receipt
            return Ok(new { message = "received" });
        }
    }
}
