using Microsoft.AspNetCore.Mvc;

namespace SendGridEmailTest.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : Controller
    {
        private readonly SendGridServiceClient _SendGridServiceClient;
        EmailController(SendGridServiceClient SendGridServiceClient)
        {
            _SendGridServiceClient = SendGridServiceClient;
        }
        [Route("SendEmail")]
        [HttpPost]
        public async Task<ActionResult> SendEmail(EmailModel req)
        {
            try
            {
                //from value will be null 
                await _SendGridServiceClient.SendEmail(null, req.Subject, req.toEmail, null, req.Body);
                return Ok("Email Sent");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error {ex.Message}");
            }

        }
    }
}
