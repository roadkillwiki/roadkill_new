using System.Threading.Tasks;
using MailKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using Roadkill.Api.Authorization;
using Roadkill.Core.Settings;

namespace Roadkill.Api.Controllers
{
	[ApiController]
	[ApiVersion("3")]
	[Route("v{version:apiVersion}/[controller]")]
	[AuthorizeWithBearer]
	public class EmailController : ControllerBase
	{
		private readonly IMailTransport _mailTransport;
		private readonly SmtpSettings _settings;

		public EmailController(IMailTransport mailTransport, SmtpSettings settings)
		{
			_mailTransport = mailTransport;
			_settings = settings;
		}

		[HttpPost]
		public async Task<ActionResult> Send(string from, string to, string subject, string body)
		{
			var message = new MimeMessage();
			message.From.Add(MailboxAddress.Parse(from));
			message.To.Add(MailboxAddress.Parse(to));
			message.Subject = subject;
			message.Body = new TextPart(body);

			await _mailTransport.ConnectAsync(_settings.Host, _settings.Port, _settings.UseSsl);

			if (!string.IsNullOrEmpty(_settings.Username))
			{
				await _mailTransport.AuthenticateAsync(_settings.Username, _settings.Password);
			}

			await _mailTransport.SendAsync(message);
			await _mailTransport.DisconnectAsync(true);

			return Ok();
		}
	}
}
