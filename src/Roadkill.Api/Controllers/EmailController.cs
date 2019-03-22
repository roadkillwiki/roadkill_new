using System.Threading.Tasks;
using MailKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MimeKit;
using Roadkill.Api.Common.Services;
using Roadkill.Core.Settings;

namespace Roadkill.Api.Controllers
{
	[Authorize]
	[Route("[controller]")]
	public class EmailController : Controller, IEmailService
	{
		private readonly IMailTransport _mailTransport;
		private readonly SmtpSettings _settings;

		public EmailController(IMailTransport mailTransport, SmtpSettings settings)
		{
			_mailTransport = mailTransport;
			_settings = settings;
		}

		[HttpPost]
		public async Task Send(string from, string to, string subject, string body)
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
		}
	}
}
