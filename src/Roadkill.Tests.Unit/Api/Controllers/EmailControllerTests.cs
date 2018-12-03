using System.Threading;
using System.Threading.Tasks;
using MailKit;
using MimeKit;
using Moq;
using Roadkill.Api.Controllers;
using Roadkill.Core.Configuration;
using Xunit;

// ReSharper disable PossibleMultipleEnumeration

namespace Roadkill.Tests.Unit.Api.Controllers
{
	public class EmailControllerTests
	{
		private Mock<IMailTransport> _mailTransportMock;

		public EmailControllerTests()
		{
			_mailTransportMock = new Mock<IMailTransport>();
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
			var emailController = new EmailController(_mailTransportMock.Object, smtpSettings);

			string to = "to@example.com";
			string from = "from@example.com";
			string subject = "the subject";
			string body = "the body";

			var expectedMessage = new MimeMessage();
			expectedMessage.From.Add(MailboxAddress.Parse(from));
			expectedMessage.To.Add(MailboxAddress.Parse(to));
			expectedMessage.Subject = subject;
			expectedMessage.Body = new TextPart(body);

			_mailTransportMock.Setup(x => x.Send(expectedMessage, cancelToken, null));

			// when
			await emailController.Send(from, to, subject, body);

			// then

			_mailTransportMock.Verify(x => x.ConnectAsync(smtpSettings.Host, smtpSettings.Port, smtpSettings.UseSsl, cancelToken), Times.Once);
			_mailTransportMock.Verify(x => x.AuthenticateAsync(smtpSettings.Username, smtpSettings.Password, cancelToken), Times.Once);
			_mailTransportMock.Verify(x => x.SendAsync(It.Is<MimeMessage>(message => CheckMessageMatches(message, expectedMessage)), cancelToken, null), Times.Once);
			_mailTransportMock.Verify(x => x.DisconnectAsync(true, cancelToken), Times.Once);
		}

		private bool CheckMessageMatches(MimeMessage message, MimeMessage expectedMessage)
		{
			return message.To.ToString() == expectedMessage.To.ToString() &&
					message.From.ToString() == expectedMessage.From.ToString() &&
					message.Subject == expectedMessage.Subject &&
					message.Body.ToString() == expectedMessage.Body.ToString();
		}
	}
}