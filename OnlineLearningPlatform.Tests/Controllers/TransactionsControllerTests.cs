using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.Presentation.Controllers;
using Xunit;

namespace OnlineLearningPlatform.Tests.Controllers
{
    public class TransactionsControllerTests
    {
        [Fact]
        public async Task Webhook_Returns_BadRequest_When_Payload_Null()
        {
            var mockPaymentService = new Mock<IPaymentService>();
            var inMemorySettings = new System.Collections.Generic.Dictionary<string, string> { { "PayOS:ChecksumKey", "secret" } };
            var config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
            var logger = new Mock<ILogger<transactionsController>>().Object;

            var controller = new transactionsController(mockPaymentService.Object, config, logger);

            // Provide a HttpContext so controller.ControllerContext is not null
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            var result = await controller.Webhook(null);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Webhook_Returns_Unauthorized_When_Signature_Invalid()
        {
            var mockPaymentService = new Mock<IPaymentService>();
            var inMemorySettings = new System.Collections.Generic.Dictionary<string, string> { { "PayOS:ChecksumKey", "secret" } };
            var config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
            var logger = new Mock<ILogger<transactionsController>>().Object;

            var controller = new transactionsController(mockPaymentService.Object, config, logger);

            // create sample webhook body
            var webhook = new PayOS.Models.Webhooks.Webhook { Data = new PayOS.Models.Webhooks.WebhookData { OrderCode = 123 } };

            // Prepare HttpContext without signature header to simulate invalid signature
            var ctx = new DefaultHttpContext();
            ctx.Request.Body = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes("{}"));
            controller.ControllerContext = new ControllerContext { HttpContext = ctx };

            var result = await controller.Webhook(webhook);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}
