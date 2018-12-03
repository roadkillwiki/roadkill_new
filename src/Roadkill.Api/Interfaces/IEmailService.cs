using System.Threading.Tasks;

namespace Roadkill.Api.Interfaces
{
	public interface IEmailService
	{
		Task Send(string from, string to, string subject, string body);
	}
}