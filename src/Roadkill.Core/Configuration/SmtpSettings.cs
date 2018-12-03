using Microsoft.Extensions.Options;

namespace Roadkill.Core.Configuration
{
	public class SmtpSettings : IOptions<SmtpSettings>
	{
		public string Host { get; set; }
		public int Port { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }

		public SmtpSettings Value => this;
		public bool UseSsl { get; set; }
	}
}