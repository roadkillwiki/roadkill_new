using System.Threading;
using System.Threading.Tasks;
using MailKit;
using MimeKit;
using NSubstitute;
using Roadkill.Api.Controllers;
using Roadkill.Core.Settings;
using Xunit;

namespace Roadkill.Tests.Unit.Api.Controllers
{
	public class EmailControllerTests
	{
		private IMailTransport _mailTransportMock;

		public EmailControllerTests()
		{
			_mailTransportMock = Substitute.For<IMailTransport>();
		}

		[Fact]
		public async Task Send()
		{
			// given
			var cancelToken = default(CancellationToken);

			var smtpSettings = new SmtpSettings()
			{
				Host = "spammail.com",
				Port = 1234,
				Username = "gareth",
				Password = "bale100",
				UseSsl = true
			};
			var emailController = new EmailController(_mailTransportMock, smtpSettings);

			string to = "to@example.com";
			string from = "from@example.com";
			string subject = "the subject";
			string body = "the body";

			var expectedMessage = new MimeMessage();
			expectedMessage.From.Add(MailboxAddress.Parse(from));
			expectedMessage.To.Add(MailboxAddress.Parse(to));
			expectedMessage.Subject = subject;
			expectedMessage.Body = new TextPart(body);

			// when
			await emailController.Send(from, to, subject, body);

			// then
			await _mailTransportMock
				.Received(1)
				.ConnectAsync(smtpSettings.Host, smtpSettings.Port, smtpSettings.UseSsl, cancelToken);

			await _mailTransportMock
				.Received(1)
				.AuthenticateAsync(smtpSettings.Username, smtpSettings.Password, cancelToken);

			await _mailTransportMock
				.Received(1)
				.SendAsync(Arg.Is<MimeMessage>(message =>
						CheckMessageMatches(message, expectedMessage)),
					cancelToken,
					null);

			await _mailTransportMock
				.Received(1)
				.DisconnectAsync(true, cancelToken);
		}

		private static bool CheckMessageMatches(MimeMessage message, MimeMessage expectedMessage)
		{
			return message.To.ToString() == expectedMessage.To.ToString() &&
					message.From.ToString() == expectedMessage.From.ToString() &&
					message.Subject == expectedMessage.Subject &&
					message.Body.ToString() == expectedMessage.Body.ToString();
		}
	}
}
