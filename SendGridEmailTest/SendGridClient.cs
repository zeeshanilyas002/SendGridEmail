using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using SendGridEmailTest.Models;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace SendGridEmailTest
{
    public class EmailModel
    {
        public string toEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
    public class SendGridServiceClient
    {
        public static string _apiKey = string.Empty;
        public static string _from = "";
        public static string _to = "";
        public static string body = "";
        public static string subject = "";
        
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _HttpClient;
        private readonly SendGridCreds _SendGridCreds;

        public SendGridServiceClient(IHttpClientFactory httpClientFactory, IOptions<SendGridCreds> configs)
        {          
            _SendGridCreds = configs.Value;
            _apiKey = _SendGridCreds.ApiKey;
            _from = _SendGridCreds.fromEmail;

            //Creating HttpCLient for sendGrid
            _httpClientFactory = httpClientFactory;
            _HttpClient = _httpClientFactory.CreateClient();
        }
        public async Task SendEmail(string from, string _subject, string _to, List<string> _CcEmails, string _EmailPlainText = "", string _htmlContent = "", string attachmentPath = "", List<string> lstAtachments = null, byte[] fileBytes = null)
        {
            // Retrieve the API key.
            try
            {
                var client = new SendGridClient(_HttpClient, new SendGridClientOptions { ApiKey = _apiKey, HttpErrorAsException = true });
                // Send a Single Email using the Mail Helper, entirely with convenience methods Method 1
                var msg = new SendGridMessage();
                msg.SetFrom(_from);
                msg.SetSubject(_subject);
                bool isHtml1 = IsHtml(_EmailPlainText);
                if (isHtml1)
                {
                    msg.AddContent(MimeType.Html, _EmailPlainText);
                }
                else
                {
                    msg.AddContent(MimeType.Text, _EmailPlainText);
                }
                msg.AddTo(_to);

                // Add css Emails
                if (_CcEmails != null && _CcEmails.Count > 0)
                {
                    var CcEmails = new List<EmailAddress>();

                    foreach (var email in _CcEmails)
                    {
                        CcEmails.Add(new EmailAddress(email));
                    }

                    var personalization = new Personalization()
                    {
                        Ccs = CcEmails
                    };
                    msg.AddCcs(CcEmails, 0, personalization);
                }

                if (fileBytes != null && fileBytes.Length > 0)
                {
                    var attachment = Convert.ToBase64String(fileBytes);
                    msg.AddAttachment("DailyActivityStats.xlsx", attachment);
                }

                if (!string.IsNullOrEmpty(attachmentPath))
                {
                    var attachmentBytes = File.ReadAllBytes(attachmentPath);
                    var attachment = Convert.ToBase64String(attachmentBytes);
                    msg.AddAttachment(Path.GetFileName(attachmentPath), attachment);
                }
                else if (lstAtachments != null && lstAtachments.Count > 0)
                {
                    foreach (var filePath in lstAtachments)
                    {
                        var attachmentBytes = File.ReadAllBytes(filePath);
                        var attachment = Convert.ToBase64String(attachmentBytes);
                        msg.AddAttachment(Path.GetFileName(filePath), attachment);
                    }
                }

                var response = await client.SendEmailAsync(msg);
            }
            catch (Exception e)
            {
                Console.Write(e.ToString());
            }
        }
        public bool IsHtml(string input)
        {
            string htmlPattern = @"<[^>]+>";
            return Regex.IsMatch(input, htmlPattern);
        }
    }
}
