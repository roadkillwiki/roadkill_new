using System.Threading.Tasks;

namespace Roadkill.Api.Client
{
	public interface IEmailClient
	{
		Task Send(string from, string to, string subject, string body);
	}
}
