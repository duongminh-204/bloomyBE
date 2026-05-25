using BloomyBE.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloomyBE.Controllers
{
    [ApiController]
    [Route("api/payment-settings")]
    [AllowAnonymous]
    public class PaymentSettingsController : ControllerBase
    {
        private readonly IPaymentSettingsService _paymentSettings;

        public PaymentSettingsController(IPaymentSettingsService paymentSettings)
        {
            _paymentSettings = paymentSettings;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var settings = await _paymentSettings.GetAsync();
            return Ok(settings);
        }
    }
}
